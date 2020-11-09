using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Zen.Base;
using Zen.Base.Extension;
using Zen.Base.Module.Log;
using Zen.Storage.Provider.File;
using Zen.Storage.Provider.File.FileSystem;

namespace Zen.Provider.GitHub.Storage
{
    [FileSystemFileStorage(Descriptor = "GitHub File Storage")]
    public abstract class GitHubFileStorage : FileStoragePrimitive
    {
        internal GitHubFileStorageConfigurationAttribute Configuration;

        protected GitHubFileStorage()
        {
            Configuration =
                GetType().GetCustomAttributes(typeof(GitHubFileStorageConfigurationAttribute), false)
                    .Select(i => (GitHubFileStorageConfigurationAttribute) i).FirstOrDefault() ??
                new GitHubFileStorageConfigurationAttribute();
            if (Configuration.ClientName == null) Configuration.ClientName = Host.ApplicationAssemblyName;

            var basePath = Configuration.UseSystemTempSpace
                ? Path.GetTempPath()
                : Path.Combine(Host.BaseDirectory, "cache", "git");

            LocalStoragePath = Path.Combine(basePath, (GetType().FullName + Configuration.Repository).Md5Hash());
        }

        public string LocalStoragePath { get; }

        public void CheckStatus(bool forceUpdate = false)
        {
            var mustClone = false;

            Log.KeyValuePair(GetType().Name, $"Repository: {Configuration.Url}", Message.EContentType.MoreInfo);
            Log.Info(LocalStoragePath);

            try
            {
                // Try to create the local git clone storage, if not present;
                Directory.CreateDirectory(LocalStoragePath);

                Credentials GitHubCredentialsProvider(string url, string usernameFromUrl,
                    SupportedCredentialTypes types)
                {
                    return new UsernamePasswordCredentials {Username = Configuration.Token, Password = ""};
                }

                var pullOptions = new PullOptions
                {
                    MergeOptions = new MergeOptions {FastForwardStrategy = FastForwardStrategy.Default},
                    FetchOptions = new FetchOptions
                    {
                        CredentialsProvider = GitHubCredentialsProvider
                    }
                };

                var cloneOptions = new CloneOptions
                {
                    CredentialsProvider = GitHubCredentialsProvider
                };

                var signature = new Signature(Host.ApplicationAssemblyName, "none@none.com", DateTimeOffset.Now);

                try
                {
                    // Is it initialized?
                    using var localRepo = new Repository(LocalStoragePath);

                    if (Configuration.Branch != null)
                    {
                        var currentBranch = localRepo.Branches[Configuration.Branch];

                        if (currentBranch == null)
                        {
                            localRepo.CreateBranch(Configuration.Branch);
                            currentBranch = localRepo.Branches[Configuration.Branch];
                        }

                        if (currentBranch.FriendlyName != Configuration.Branch)
                            currentBranch = Commands.Checkout(localRepo, currentBranch);
                    }

                    Log.Add(LocalStoragePath);

                    var status = localRepo.RetrieveStatus();

                    // Always try to Fetch changes.
                    var remote = localRepo.Network.Remotes.FirstOrDefault();
                    var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification).ToList();
                    Commands.Fetch(localRepo, remote.Name, refSpecs, pullOptions.FetchOptions, null);

                    try
                    {
                        var mergeResult = Commands.Pull(localRepo, signature, pullOptions);

                        if (mergeResult.Status == MergeStatus.UpToDate)
                            Log.KeyValuePair(GetType().Name, "Repository is up-to-date",
                                Message.EContentType.MoreInfo);
                        else
                            Log.KeyValuePair(GetType().Name,
                                mergeResult?.Commit?.Message.Replace("\n", " | ") ?? "Fetch/Pull finished.",
                                Message.EContentType.MoreInfo);
                    }
                    catch (Exception e)
                    {
                        Log.Add(e);
                    }

                    // Commit/push stragglers.

                    if (status.IsDirty)
                    {

                        Log.KeyValuePair(GetType().Name, $"{status.Count()} uncommitted change(s)", Message.EContentType.MoreInfo);


                        //try
                        //{
                        //    var commitOptions = new CommitOptions();
                        //    localRepo.Commit(Host.ApplicationAssemblyName + " automatic sync", signature, signature, commitOptions);
                        //}
                        //catch (Exception) { }

                        //var targetSpec = @"refs/heads/" + Configuration.Branch.Split('/')[1];

                        //localRepo.Network.Push(remote, targetSpec, new PushOptions {CredentialsProvider = GitHubCredentialsProvider});
                    }
                }
                catch (RepositoryNotFoundException e)
                {
                    Log.KeyValuePair(GetType().Name, "Local repo not found. Attempting to create.",
                        Message.EContentType.Maintenance);
                    mustClone = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                if (!mustClone) return;

                try
                {
                    Log.KeyValuePair("Cloning", Configuration.Url, Message.EContentType.MoreInfo);
                    Log.KeyValuePair("To", LocalStoragePath, Message.EContentType.MoreInfo);

                    // Easy. Let's try to clone from source.
                    Repository.Clone(Configuration.Url, LocalStoragePath, cloneOptions);

                    Log.KeyValuePair("Status", "Success", Message.EContentType.MoreInfo);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override IFileStorage ResolveStorage()
        {
            CheckStatus(true);

            return this;
        }

        #region Overrides of FileStoragePrimitive

        public override Task<Dictionary<string, IStorageEntityDescriptor>> Collection(string referencePath = null)
        {
            var path = Path.Join(LocalStoragePath, referencePath);

            var res = new List<IStorageEntityDescriptor>();

            res.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly)
                .Select(dir => new DirectoryInfo(dir))
                .Select(dirInfo => new GitHubDirectoryDescriptor
                {
                    StorageName = dirInfo.Name,
                    StoragePath = dirInfo.Parent?.FullName,
                    Creation = dirInfo.CreationTime
                })
                .Cast<IStorageEntityDescriptor>()
                .ToList());

            res.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Select(file => new FileInfo(file))
                .Select(fileInfo => new GitHubFileDescriptor
                {
                    FileSize = fileInfo.Length,
                    StorageName = fileInfo.Name,
                    StoragePath = fileInfo.DirectoryName,
                    Creation = fileInfo.CreationTime
                })
                .Cast<IStorageEntityDescriptor>()
                .ToList());

            return Task.FromResult(res.ToDictionary(i => i.StorageName, i => i));
        }

        #endregion

        #region Overrides of FileStoragePrimitive

        public override Task<Stream> Fetch(IFileDescriptor definition)
        {
            var fullPath = Path.Combine(definition.StoragePath, definition.StorageName);
            var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            Stream castBuffer = fileStream;

            return Task.FromResult(castBuffer);
        }

        public override async Task<string> Store(IFileDescriptor definition, Stream source)
        {
            var fullPath = Path.Combine(LocalStoragePath, definition.StoragePath.Replace("/", "\\"),
                definition.StorageName);
            await using var fs = File.OpenWrite(fullPath);

            source.Seek(0, SeekOrigin.Begin);
            await source.CopyToAsync(fs);
            fs.Close();

            return fullPath;
        }

        #endregion
    }
}
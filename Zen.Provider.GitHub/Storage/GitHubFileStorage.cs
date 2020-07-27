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
            Configuration = GetType().GetCustomAttributes(typeof(GitHubFileStorageConfigurationAttribute), false).Select(i => (GitHubFileStorageConfigurationAttribute) i).FirstOrDefault() ?? new GitHubFileStorageConfigurationAttribute();
            if (Configuration.ClientName == null) Configuration.ClientName = Host.ApplicationAssemblyName;

            LocalStoragePath = Path.Combine(Host.BaseDirectory, "cache", "git", (GetType().FullName + Configuration.Repository).Md5Hash());
        }

        public string LocalStoragePath { get; }

        public void CheckStatus(bool forceUpdate = false)
        {
            var mustClone = false;

            Log.KeyValuePair(GetType().Name, $"Cloning: {Configuration.Url}", Message.EContentType.MoreInfo);
            Log.KeyValuePair(GetType().Name, $"To     : {LocalStoragePath}", Message.EContentType.MoreInfo);

            try
            {
                // Try to create the local git clone storage, if not present;
                Directory.CreateDirectory(LocalStoragePath);

                try
                {
                    // Is it initialized?
                    using var localRepo = new Repository(LocalStoragePath);

                    var status = localRepo.RetrieveStatus();
                    if (status.IsDirty)
                    {
                        var remote = localRepo.Network.Remotes.FirstOrDefault();
                        if (remote != null)
                        {
                            Log.KeyValuePair(GetType().Name, "Remote repo has pending changes.", Message.EContentType.MoreInfo);

                            try
                            {
                                string logMessage = null;

                                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                                Commands.Fetch(localRepo, remote.Name, refSpecs, null, logMessage);

                                Log.KeyValuePair(GetType().Name, logMessage?? "Fetch finished.", Message.EContentType.MoreInfo);
                            } catch (Exception e) { Log.Add(e); }
                        }
                    }
                } catch (RepositoryNotFoundException e)
                {
                    Log.KeyValuePair(GetType().Name, "Local repo not found. Attempting to create.", Message.EContentType.Maintenance);
                    mustClone = true;
                } catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                if (mustClone)
                    try
                    {
                        Log.KeyValuePair("Cloning", Configuration.Url, Message.EContentType.MoreInfo);
                        Log.KeyValuePair("To", LocalStoragePath, Message.EContentType.MoreInfo);

                        // Easy. Let's try to clone from source.
                        Repository.Clone(Configuration.Url, LocalStoragePath);

                        Log.KeyValuePair("Status", "Success", Message.EContentType.MoreInfo);
                    } catch (Exception e) { Console.WriteLine(e); }
            } catch (Exception e) { Console.WriteLine(e); }
        }

        public override IFileStorage ResolveStorage()
        {
            CheckStatus(true);

            return this;
        }

        #region Overrides of FileStoragePrimitive

        public override Task<List<IStorageEntityDescriptor>> Collection(string referencePath = null)
        {
            var path = Path.Join(LocalStoragePath, referencePath);

            var res = new List<IStorageEntityDescriptor>();

            res.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly)
                             .Select(dir => new DirectoryInfo(dir))
                             .Select(dirInfo => new GitHubDirectoryDescriptor {StorageName = dirInfo.Name, StoragePath = dirInfo.Parent?.FullName, Creation = dirInfo.CreationTime})
                             .Cast<IStorageEntityDescriptor>()
                             .ToList());

            res.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                             .Select(file => new FileInfo(file))
                             .Select(fileInfo => new GitHubFileDescriptor {FileSize = fileInfo.Length, StorageName = fileInfo.Name, StoragePath = fileInfo.DirectoryName, Creation = fileInfo.CreationTime})
                             .Cast<IStorageEntityDescriptor>()
                             .ToList());

            return Task.FromResult(res);
        }

        #endregion
    }
}
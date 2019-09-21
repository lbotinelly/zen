using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Zen.Module.Cloud.AWS.Connectors
{
    public class S3Connector
    {
        private readonly string _defaultBucket;

        public S3Connector(string systemName, string defaultBucket = null)
        {
            Client = new AmazonS3Client(new StoredProfileAWSCredentials(), RegionEndpoint.GetBySystemName(systemName));
            _defaultBucket = defaultBucket;
        }

        public IAmazonS3 Client { get; }

        public bool Exists(string key, string bucket = null)
        {
            return ExistsAsync(key, bucket).Result;
        }

        public async Task<bool> ExistsAsync(string key, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;

            try
            {
                await Client.GetObjectMetadataAsync(bucket, key);
                return true;
            }
            catch (Exception) { return false; }
        }

        public void Delete(string key, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;

            Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = key
            });
        }

        public string Put(string key, Stream content, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;
            var call = PutNativeAsync(key, content, bucket);
            call.Wait();

            return call.Result.ETag;
        }

        public async Task<string> PutAsync(string key, Stream content, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;
            var call = await PutNativeAsync(key, content, bucket);

            return call.ETag;
        }

        public Stream Get(string key, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;

            var response = GetNativeAsync(key, bucket).Result;

            return response.ResponseStream;
        }

        public async Task<Stream> GetAsync(string key, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;

            var response = await GetNativeAsync(key, bucket);

            return response.ResponseStream;
        }

        #region Native methods

        private Task<GetObjectResponse> GetNativeAsync(string key, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;

            return Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucket,
                Key = key
            });
        }

        private Task<PutObjectResponse> PutNativeAsync(string key, Stream content, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;

            return Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                AutoCloseStream = true,
                InputStream = content
            });
        }

        #endregion

        #region Helpers

        public void PutString(string key, string content, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;
            PutNativeAsync(key, new MemoryStream(Encoding.UTF8.GetBytes(content)), bucket).Wait();
        }

        public string GetString(string key, string bucket = null)
        {
            bucket = bucket ?? _defaultBucket;

            var response = GetNativeAsync(key, bucket).Result;
            return new StreamReader(response.ResponseStream).ReadToEnd();
        }

        #endregion
    }
}
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace Zen.Module.Cloud.AWS
{
    public class S3Connector
    {
        public S3Connector(string systemName)
        {
            Client = new AmazonS3Client(new StoredProfileAWSCredentials(), RegionEndpoint.GetBySystemName(systemName));
        }

        public IAmazonS3 Client { get; }

        public bool ObjectExists(string bucket, string key)
        {
            try
            {
                var probe = Client.GetObjectMetadataAsync(bucket, key).Result;
                return true;
            } catch (Exception e) { return false; }
        }

        private GetObjectResponse GetObjectAsync(string bucket, string key)
        {
            return Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucket,
                Key = key
            }).Result;
        }

        private async Task<PutObjectResponse> PutObjectAsync(string bucket, string key, Stream content)
        {
            return await Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                AutoCloseStream = true,
                InputStream = content
            });
        }

        public void s3DeleteKeyFromBucket(string bucket, string key)
        {
            Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = key
            });
        }

        public void s3WriteStringToBucket(string bucket, string key, string content) { PutObjectAsync(bucket, key, new MemoryStream(Encoding.UTF8.GetBytes(content))).Wait(); }

        public void s3WriteStreamToBucket(string bucket, string key, Stream content) { PutObjectAsync(bucket, key, content).Wait(); }

        public string s3ReadStringFromBucket(string bucket, string key)
        {
            var response = GetObjectAsync(bucket, key);
            return new StreamReader(response.ResponseStream).ReadToEnd();
        }

        public Stream s3ReadStreamFromBucket(string bucket, string key)
        {
            var response = GetObjectAsync(bucket, key);
            return response.ResponseStream;
        }
    }
}
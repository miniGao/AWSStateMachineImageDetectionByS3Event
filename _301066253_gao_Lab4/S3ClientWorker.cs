using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace _301066253_gao_Lab4
{
    public class S3ClientWorker
    {
        public string BucketName { get; set; }
        private readonly RegionEndpoint bucketRegion;
        public IAmazonS3 S3Client { get; set; }

        public S3ClientWorker(string bucketName)
        {
            BucketName = bucketName;
            bucketRegion = RegionEndpoint.USEast1;
            S3Client = new AmazonS3Client(GetBasicCredentials(), bucketRegion);
        }

        private BasicAWSCredentials GetBasicCredentials()
        {
            var builder = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var accessKeyID = builder.Build().GetSection("AWSCredentials").GetSection("AccessKeyID").Value;
            var secretKey = builder.Build().GetSection("AWSCredentials").GetSection("SecretAccessKey").Value;
            return new BasicAWSCredentials(accessKeyID, secretKey);
        }

        public async Task<Stream> RunGetAsync(string key)
        {
            var response = await S3Client.GetObjectAsync(BucketName, key);
            return response.ResponseStream;
        }

        public async Task RunUploadAsync(Stream stream, string key)
        {
            TransferUtility transferUtility = new TransferUtility(S3Client);
            var request = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                BucketName = BucketName,
                Key = key,
                CannedACL = S3CannedACL.PublicRead
            };
            await transferUtility.UploadAsync(request);
        }
    }
}

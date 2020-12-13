using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace _301066253_gao_Lab4
{
    public class DynamoClientWorker
    {
        private readonly RegionEndpoint dBRegion;
        public AmazonDynamoDBClient DynamoClient { get; set; }
        private DynamoDBContextConfig dBContextConfig;
        private DynamoDBContext dBContext;
        
        public DynamoClientWorker()
        {
            dBRegion = RegionEndpoint.USEast1;
            DynamoClient = new AmazonDynamoDBClient(GetBasicCredentials(), dBRegion);
            dBContextConfig = new DynamoDBContextConfig
            {
                ConsistentRead = true,
                Conversion = DynamoDBEntryConversion.V2
            };
            dBContext = new DynamoDBContext(DynamoClient, dBContextConfig);
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

        public async Task AddImageLabelAsync(ImageLabel imageLabel)
        {
            await dBContext.SaveAsync(imageLabel);
        }
    }
}

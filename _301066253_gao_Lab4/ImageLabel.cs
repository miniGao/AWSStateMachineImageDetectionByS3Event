using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace _301066253_gao_Lab4
{
    [DynamoDBTable("Lab4ImageLabels")]
    public class ImageLabel
    {
        [DynamoDBHashKey]
        public string ImageUrl { get; set; }
        public List<string> LabelAndConfidence { get; set; }
    }
}

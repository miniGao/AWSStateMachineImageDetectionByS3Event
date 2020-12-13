using System;
using System.Collections.Generic;
using System.Text;

namespace _301066253_gao_Lab4
{
    /// <summary>
    /// The state passed between the step function executions.
    /// </summary>
    public class State
    {
        public StateDetail Detail { get; set; }
        public string Error { get; set; } = "";

        public string BucketName { get; set; }
        public string Key { get; set; }
        public List<string> LabelAndConfidence { get; set; }
        public string ImageUrl {
            get
            {
                return $"https://{BucketName}.s3.amazonaws.com/{Key}";
            }
        }
    }
}

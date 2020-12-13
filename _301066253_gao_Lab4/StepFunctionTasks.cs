using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;
using GrapeCity.Documents.Imaging;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace _301066253_gao_Lab4
{
    public class StepFunctionTasks
    {
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public StepFunctionTasks()
        {
        }

        public async Task<State> ImageRecognition(State state, ILambdaContext context)
        {
            if (state != null 
                && state.Detail != null 
                && state.Detail.RequestParameters != null 
                && state.Detail.RequestParameters.BucketName != null 
                && state.Detail.RequestParameters.Key != null)
            {
                float minConfidence = 90f;
                string bucketName = state.Detail.RequestParameters.BucketName;
                string key = state.Detail.RequestParameters.Key;

                IAmazonS3 s3Client = new AmazonS3Client();
                IAmazonRekognition rekognitionClient = new AmazonRekognitionClient();
                HashSet<string> supportedImageTypes = new HashSet<string> { ".png", ".jpg", ".jpeg" };
                if (!supportedImageTypes.Contains(Path.GetExtension(key)))
                {
                    state = new State();
                    state.Error = "File type not supported";
                    return state;
                }

                var detectResponses = await rekognitionClient.DetectLabelsAsync(new DetectLabelsRequest
                {
                    MinConfidence = minConfidence,
                    Image = new Image
                    {
                        S3Object = new Amazon.Rekognition.Model.S3Object
                        {
                            Bucket = bucketName,
                            Name = key
                        }
                    }
                });

                var imageLabelAndConfidence = new List<string>();
                foreach (var label in detectResponses.Labels)
                {
                    imageLabelAndConfidence.Add($"{label.Name} : {label.Confidence.ToString()}");
                }
                state = new State();
                state.BucketName = bucketName;
                state.Key = key;
                state.LabelAndConfidence = imageLabelAndConfidence;
            }
            else
            {
                state = new State();
                state.Error = "Request Parameter is null";
            }
            return state;
        }

        public State EndAtError(State state, ILambdaContext context)
        {
            return state;
        }

        public async Task<State> AddLabelToDynamoDB(State state, ILambdaContext context)
        {
            DynamoClientWorker dynamoWorker = new DynamoClientWorker();
            ImageLabel imageLabel = new ImageLabel();
            imageLabel.ImageUrl = state.ImageUrl;
            imageLabel.LabelAndConfidence = state.LabelAndConfidence;
            await dynamoWorker.AddImageLabelAsync(imageLabel);
            return state;
        }

        public async Task<State> AddThumbnailToS3(State state, ILambdaContext context)
        {
            S3ClientWorker originWorker = new S3ClientWorker("uploadbucket-ligao-lab4");
            S3ClientWorker targetWorker = new S3ClientWorker("thumbnailbucket-ligao-lab4");

            Stream responseStream = await originWorker.RunGetAsync(state.Key);

            var originBmp = new GcBitmap();
            originBmp.Load(responseStream);
            int newWidth = (int)(originBmp.Width * 0.1);
            int newHeight = (int)(originBmp.Height * 0.1);
            var resultBmp = originBmp.Resize(newWidth, newHeight, InterpolationMode.Downscale);

            var stream = new MemoryStream();
            resultBmp.SaveAsJpeg(stream);
            await targetWorker.RunUploadAsync(stream, state.Key + ".jpeg");
            return state;
        }
    }
}

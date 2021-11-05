using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using simple_minio_api.Models;

namespace simple_minio_api.Controllers
{
    [ApiController]
    [Route("/api/v1/uploader")]
    [Produces("application/json")]
    public class UploaderController : Controller
    {
        public static string MinioUsername = "minioadmin";
        public static string MinioPassword = "minioadmin";
        public static string MinioBucketName = "hello";


        /// <summary>
        /// Create "hello" bucket before upload a file
        /// also read under if have problem to run docker-compose
        /// https://docs.min.io/docs/minio-docker-quickstart-guide.html
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [DisableRequestSizeLimit]
        [HttpPost("upload_file")]
        [ProducesResponseType(200, Type = typeof(UploadFileResponse))]
        public async Task<IActionResult> UploadTemp(IFormFile file)
        {
            try
            {
                using var clientS3 = createClient();

                var fileId = $"{Guid.NewGuid()}-{file.FileName}" ;

                using (var newMemoryStream = new MemoryStream())
                {
                    file.CopyTo(newMemoryStream);

                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = newMemoryStream,
                        Key = fileId,
                        BucketName = MinioBucketName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    var fileTransferUtility = new TransferUtility(clientS3);
                    await fileTransferUtility.UploadAsync(uploadRequest);
                }

                //var obj = new PutObjectRequest()
                //{
                //    BucketName = MinioBucketName,
                //    Key = $"{Guid.NewGuid()}.txt",
                //    ContentType = "text/plain",
                //    ContentBody = "bbbbbbbbbbb",
                //    //ObjectLockRetainUntilDate = DateTime.Now.AddMinutes(2)
                //};
                //await clientS3.PutObjectAsync(blobObj);

                var res = new UploadFileResponse()
                {
                    temp_id = fileId,
                    file_name = file.FileName,
                    temp_url = null
                };

                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost("create_new_bucket")]
        [ProducesResponseType(200, Type = typeof(UploadFileResponse))]
        public async Task<IActionResult> CreateNewBucket(string bucket_name)
        {
            try
            {
                using var clientS3 = createClient();

                await clientS3.PutBucketAsync(new PutBucketRequest()
                {
                    BucketName = bucket_name,
                });

                return Ok("Created");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete_bucket")]
        [ProducesResponseType(200, Type = typeof(UploadFileResponse))]
        public async Task<IActionResult> DeleteBucket(string bucket_name)
        {
            try
            {
                using var clientS3 = createClient();

                await clientS3.DeleteBucketAsync(new DeleteBucketRequest()
                {
                    BucketName = bucket_name,
                });

                return Ok("Deleted");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("get_bucket_names")]
        [ProducesResponseType(200, Type = typeof(UploadFileResponse))]
        public async Task<IActionResult> GetBucketNames()
        {
            try
            {
                using var clientS3 = createClient();

                var items = await clientS3.ListBucketsAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("get_objects_names")]
        [ProducesResponseType(200, Type = typeof(UploadFileResponse))]
        public async Task<IActionResult> GetObjectOfBucketName(string bucket_name)
        {
            try
            {
                using var clientS3 = createClient();

                var items = await clientS3.ListObjectsAsync(bucket_name);

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("get_object")]
        [ProducesResponseType(200, Type = typeof(UploadFileResponse))]
        public async Task<IActionResult> GetObject(string bucket_name, string key_name)
        {
            try
            {
                using var clientS3 = createClient();

                var item = await clientS3.GetObjectAsync(new GetObjectRequest()
                {
                    Key = key_name,
                    BucketName = bucket_name
                });

                var bytes = new byte[item.ResponseStream.Length];
                await item.ResponseStream.ReadAsync(bytes, 0, bytes.Length);

                return Ok("File Loaded");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        private AmazonS3Config getConfig()
        {
            var config = new AmazonS3Config
            {
                //AuthenticationRegion = RegionEndpoint.USEast1.SystemName, // Should match the `MINIO_REGION` environment variable.
                ServiceURL = "http://192.168.0.102:9000", // replace http://localhost:9000 with URL of your MinIO server
                ForcePathStyle = true // MUST be true to work correctly with MinIO server
            };

            return config;
        }

        private AmazonS3Client createClient()
        {
            var config = getConfig();

            var clientS3 = new Amazon.S3.AmazonS3Client(MinioUsername, MinioPassword, config);

            return clientS3;
        }

    }
}
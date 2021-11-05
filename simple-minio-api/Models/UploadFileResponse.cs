namespace simple_minio_api.Models
{
    public class UploadFileResponse
    {
        public string temp_id { get; set; }

        public string file_name { get; set; }

        public string temp_url { get; set; }
    }
}
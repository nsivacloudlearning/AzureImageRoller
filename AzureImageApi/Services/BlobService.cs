namespace AzureImageApi.Services
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Specialized;

    public class BlobService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobService(IConfiguration config)
        {
            var connectionString = config["AzureStorageConnectionString"];
            var containerName = config["AzureStorageContainerName"];
            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists();
        }

        public async Task UploadAsync(string fileName, Stream fileStream)
        {
            await _containerClient.UploadBlobAsync(fileName, fileStream);
        }

        public async Task<Stream> DownloadAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        public async Task<bool> DeleteImage(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            
            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }

        public IEnumerable<string> ListImages()
        {
            return _containerClient.GetBlobs()
                .Select(b => _containerClient.GetBlobClient(b.Name).Uri.ToString());
        }
    }

}

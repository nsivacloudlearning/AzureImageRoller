    using AzureImageApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureImageApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureImagesController : ControllerBase
    {
        private readonly BlobService _blobService;
        private readonly ILogger<AzureImagesController> _logger;

        public AzureImagesController(BlobService blobService, ILogger<AzureImagesController> logger)
        {
            _blobService = blobService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {           
                _logger.LogInformation("Upload request received for file {FileName}", file.FileName);

                try
                {
                    using var stream = file.OpenReadStream();
                    await _blobService.UploadAsync(file.FileName, stream);
                    return Ok("Uploaded successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
                    return StatusCode(500, "Internal server error");
                }                       
        }

        [Authorize]
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> Download(string fileName)
        {
            _logger.LogInformation("Download request received for file {FileName}", fileName);

            try
            { 

            var stream = await _blobService.DownloadAsync(fileName);
            return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)  {
                _logger.LogError(ex, "Error downloading file {FileName}", fileName);
                return NotFound("File not found");
            }
        }

        [Authorize]
        [HttpGet("list")]
        public IActionResult List()
        {
            _logger.LogInformation("List request received");

            try
            {
                var imageFiles = _blobService.ListImages();
                return Ok(imageFiles);
            }            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

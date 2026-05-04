using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetailSystem.Infrastructure.Services.Interfaces;

namespace RetailSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        public CloudinaryController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            var result = await _cloudinaryService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            return Ok(new
            {
                PublicId = result.PublicId,
                Url = result.SecureUrl.AbsoluteUri
            });
        }
    }
}

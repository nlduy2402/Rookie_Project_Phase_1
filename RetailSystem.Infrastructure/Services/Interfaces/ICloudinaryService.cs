using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Infrastructure.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
        Task<List<ImageUploadResult>> AddPhotosAsync(IEnumerable<IFormFile> files);
    }
}

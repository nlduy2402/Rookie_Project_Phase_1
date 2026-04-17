using RetailSystem.Domain.Entities;
using RetailSystem.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetailSystem.Shared.Extensions
{
    public static class ProductExtensions
    {
        public static void UpdateFromDto(this Product product, UpdateProductDTO dto)
        {
            if (dto.Name != null)
                product.Name = dto.Name;

            if (dto.Description != null)
                product.Description = dto.Description;

            if (dto.Price.HasValue)
                product.Price = dto.Price.Value;

            if (dto.Quantity.HasValue)
                product.Quantity = dto.Quantity.Value;

            if (dto.RAM != null)
                product.RAM = dto.RAM;

            if (dto.SSD != null)
                product.SSD = dto.SSD;

            if (dto.ChipSet != null)
                product.ChipSet = dto.ChipSet;

            if (dto.CategoryId.HasValue)
                product.CategoryId = dto.CategoryId.Value;

            if (dto.ImageUrls != null)
            {
                product.Images.Clear();

                product.Images = dto.ImageUrls
                    .Select(url => new ProductImage
                    {
                        Url = url
                    }).ToList();
            }

            product.LastUpdatedAt = DateTime.UtcNow;
        }
    }
}

using Domain.Entities.DeveloperEntities;
using Shared.DeveloperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    public class PropertyCountSpecifications : Specifications<Property>
    {
        public PropertyCountSpecifications(PropertySpecificationsParameters parameters)
            : base(property =>
                (!parameters.DeveloperId.HasValue || property.DeveloperId == parameters.DeveloperId.Value) &&
                (!parameters.BrokerId.HasValue || property.BrokerId == parameters.BrokerId.Value) &&
                (string.IsNullOrWhiteSpace(parameters.Government) || property.Government.ToLower().Contains(parameters.Government.ToLower().Trim())) &&
                (string.IsNullOrWhiteSpace(parameters.City) || property.City.ToLower().Contains(parameters.City.ToLower().Trim())) &&
                (!parameters.Type.HasValue || property.Type == parameters.Type.Value) &&
                (!parameters.Status.HasValue || property.Status == parameters.Status.Value) &&
                (!parameters.MinPrice.HasValue || property.Price >= parameters.MinPrice.Value) &&
                (!parameters.MaxPrice.HasValue || property.Price <= parameters.MaxPrice.Value) &&
                (!parameters.MinArea.HasValue || property.Area >= parameters.MinArea.Value) &&
                (!parameters.MaxArea.HasValue || property.Area <= parameters.MaxArea.Value) &&
                (string.IsNullOrWhiteSpace(parameters.Search) || property.Title.ToLower().Contains(parameters.Search.ToLower().Trim()) || property.Description.ToLower().Contains(parameters.Search.ToLower().Trim())))
        {
        }
    }
} 
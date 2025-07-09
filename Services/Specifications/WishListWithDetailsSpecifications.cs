using Domain.Entities;
using Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Services.Specifications
{
    public class WishListWithDetailsSpecifications : Specifications<WishListItem>
    {
        public WishListWithDetailsSpecifications(string userId) : base(w => w.UserId == userId)
        {
            AddInclude(w => w.Property);
            AddInclude(w => w.Property.Developer);
            AddInclude(w => w.Property.Broker);
            AddInclude(w => w.User);
            setOrderByDescending(w => w.CreatedAt);
        }

        public WishListWithDetailsSpecifications(int propertyId) : base(w => w.PropertyId == propertyId)
        {
            AddInclude(w => w.Property);
            AddInclude(w => w.Property.Developer);
            AddInclude(w => w.Property.Broker);
            AddInclude(w => w.User);
            setOrderByDescending(w => w.CreatedAt);
        }

        public WishListWithDetailsSpecifications(int id, string userId) : base(w => w.Id == id && w.UserId == userId)
        {
            AddInclude(w => w.Property);
            AddInclude(w => w.Property.Developer);
            AddInclude(w => w.Property.Broker);
            AddInclude(w => w.User);
        }
    }

    public class WishListCountSpecifications : Specifications<WishListItem>
    {
        public WishListCountSpecifications(string userId) : base(w => w.UserId == userId)
        {
        }

        public WishListCountSpecifications(int propertyId) : base(w => w.PropertyId == propertyId)
        {
        }
    }

    public class MostWishedPropertiesSpecifications : Specifications<WishListItem>
    {
        public MostWishedPropertiesSpecifications(int? developerId = null, int? brokerId = null, string? propertyType = null, string? government = null, string? city = null, DateTime? fromDate = null, DateTime? toDate = null) 
            : base(w => 
                (!developerId.HasValue || w.Property.DeveloperId == developerId.Value) &&
                (!brokerId.HasValue || w.Property.BrokerId == brokerId.Value) &&
                (string.IsNullOrEmpty(propertyType) || w.Property.Type.ToString() == propertyType) &&
                (string.IsNullOrEmpty(government) || w.Property.Government == government) &&
                (string.IsNullOrEmpty(city) || w.Property.City == city) &&
                (!fromDate.HasValue || w.CreatedAt >= fromDate.Value) &&
                (!toDate.HasValue || w.CreatedAt <= toDate.Value))
        {
            AddInclude(w => w.Property);
            AddInclude(w => w.Property.Developer);
            AddInclude(w => w.Property.Broker);
            setOrderByDescending(w => w.PropertyId);
        }
    }

    public class PropertyWishListUsersSpecifications : Specifications<WishListItem>
    {
        public PropertyWishListUsersSpecifications(int propertyId) : base(w => w.PropertyId == propertyId)
        {
            AddInclude(w => w.Property);
            AddInclude(w => w.User);
            setOrderByDescending(w => w.CreatedAt);
        }
    }
} 
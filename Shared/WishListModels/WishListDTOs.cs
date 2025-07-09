using System;
using System.Collections.Generic;

namespace Shared.WishListModels
{
    // Request DTOs
    public record AddToWishListDTO(int PropertyId);

    // Response DTOs
    public record WishListItemDTO
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public string PropertyDescription { get; set; }
        public decimal PropertyPrice { get; set; }
        public string PropertyGovernment { get; set; }
        public string PropertyCity { get; set; }
        public string PropertyFullAddress { get; set; }
        public decimal PropertyArea { get; set; }
        public string PropertyType { get; set; }
        public string PropertyStatus { get; set; }
        public string PropertyMainImageUrl { get; set; }
        public string? DeveloperName { get; set; }
        public string? BrokerName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record WishListCountDTO
    {
        public int TotalItems { get; set; }
        public string UserId { get; set; }
    }

    // Admin DTOs
    public record MostWishedPropertyDTO
    {
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public decimal PropertyPrice { get; set; }
        public string PropertyType { get; set; }
        public string PropertyStatus { get; set; }
        public int WishListCount { get; set; }
        public string? DeveloperName { get; set; }
        public string? BrokerName { get; set; }
    }

    public record PropertyWishListUsersDTO
    {
        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public int TotalWishListCount { get; set; }
        public List<UserWishListInfoDTO> Users { get; set; } = new();
    }

    public record UserWishListInfoDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime AddedToWishListAt { get; set; }
    }

    // Filter DTOs
    public record WishListFilterDTO
    {
        public int? PropertyId { get; set; }
        public int? DeveloperId { get; set; }
        public int? BrokerId { get; set; }
        public string? PropertyType { get; set; }
        public string? Government { get; set; }
        public string? City { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
} 
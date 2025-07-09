using System;

namespace Shared.BrokerModels
{
    public record BrokerResultDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? AgencyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Government { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record BrokerCreateDTO
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? AgencyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Government { get; set; }
        public string? LogoUrl { get; set; }
    }

    public record BrokerUpdateDTO
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string? AgencyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Government { get; set; }
        public string? LogoUrl { get; set; }
    }
} 
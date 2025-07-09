using System;

namespace Shared.DeveloperModels
{
    public record DeveloperResultDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record DeveloperCreateDTO
    {
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string LogoUrl { get; set; }
    }

    public record DeveloperUpdateDTO
    {
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string LogoUrl { get; set; }
    }
} 
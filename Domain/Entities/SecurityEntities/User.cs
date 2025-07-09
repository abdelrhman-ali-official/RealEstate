global using Microsoft.AspNetCore.Identity;
//using Domain.Entities.DoctorEntites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.SecurityEntities
{
    public enum Role : byte
    {
        Customer = 1,
        Broker= 2,
        Developer=3,
        Admin = 4
    }
    public enum Gender : byte
    {
        male = 1,
        female = 2,
        m = 1,
        f = 2
    }
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public Address Address { get; set; }
        public Gender Gender { get; set; }
        public Role UserRole { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;


    }
   
}

global using Microsoft.EntityFrameworkCore;
using Domain.Entities.DeveloperEntities;
using Domain.Entities.ProductEntities;
using Domain.Entities.SecurityEntities;
using Domain.Entities.BrokerEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public class StoreContext : IdentityDbContext<User>
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Required for Identity
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductType> ProductType { get; set; }
        public DbSet<Developer> Developers { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Broker> Brokers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<WishListItem> WishListItems { get; set; }
        public DbSet<PropertyViewHistory> PropertyViewHistories { get; set; }
        public DbSet<Domain.Entities.ChatRoom> ChatRooms { get; set; }
        public DbSet<Domain.Entities.ChatMessage> ChatMessages { get; set; }
        public DbSet<Domain.Entities.ChatMessageReaction> ChatMessageReactions { get; set; }
    }
}


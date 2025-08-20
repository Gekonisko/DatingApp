using System;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
}

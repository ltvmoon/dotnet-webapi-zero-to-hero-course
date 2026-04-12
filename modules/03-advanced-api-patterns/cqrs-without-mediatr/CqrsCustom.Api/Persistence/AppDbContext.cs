using CqrsCustom.Api.Features.Products;
using Microsoft.EntityFrameworkCore;

namespace CqrsCustom.Api.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}

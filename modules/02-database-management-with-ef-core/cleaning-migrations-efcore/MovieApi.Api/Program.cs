using Microsoft.EntityFrameworkCore;
using MovieApi.Api.Endpoints;
using MovieApi.Api.Persistence;
using MovieApi.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<MovieDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

builder.Services.AddTransient<IMovieService, MovieService>();

var app = builder.Build();

// Migrations are applied manually in this project so you can practice the cleanup
// workflows from the article (remove, revert, squash, reset). Use:
//   dotnet ef database update
// from the MovieApi.Api folder when you want to apply pending migrations.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello World!")
   .Produces(200, typeof(string));

app.MapMovieEndpoints();

app.Run();

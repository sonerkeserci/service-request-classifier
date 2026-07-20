using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RequestClassifier.Domain.Entities;
using RequestClassifier.Infrastructure.Data;
using RequestClassifier.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the ApplicationDbContext as the implementation of IApplicationDbContext
// When a service requests IApplicationDbContext, it will receive an instance of ApplicationDbContext
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>(); 

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

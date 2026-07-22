using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RequestClassifier.Domain.Entities;
using RequestClassifier.Infrastructure.Data;
using RequestClassifier.Application.Interfaces;
using RequestClassifier.Application.Services;
using RequestClassifier.Infrastructure.Data.Seed;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the ApplicationDbContext as the implementation of IApplicationDbContext
// When a service requests IApplicationDbContext, it will receive an instance of ApplicationDbContext
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

// Register the ServiceRequestService as the implementation of IServiceRequestService
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
// Register the DepartmentService as the implementation of IDepartmentService
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
// Register the RequestCategoryService as the implementation of IRequestCategoryService
builder.Services.AddScoped<IRequestCategoryService, RequestCategoryService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Used for one-time database seeding. Comment out after the initial run.
/*
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await DatabaseSeeder.SeedAsync(context);
}
*/

app.Run();

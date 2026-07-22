using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RequestClassifier.Domain.Entities;
using RequestClassifier.Infrastructure.Data;
using RequestClassifier.Application.Interfaces;
using RequestClassifier.Application.Services;
using RequestClassifier.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
// Register the authentication service.
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Read JWT configuration values from appsettings.json.
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Configure JWT Bearer authentication for incoming API requests.
builder.Services
    .AddAuthentication(options =>
    {
        // Use JWT Bearer as the default authentication scheme.
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

        // Use JWT Bearer when an unauthenticated request must be challenged.
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Define how incoming JWT tokens will be validated.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Ensure the token was issued by the expected issuer.
            ValidateIssuer = true,

            // Ensure the token was created for the expected audience.
            ValidateAudience = true,

            // Reject expired tokens.
            ValidateLifetime = true,

            // Validate the token signature using the configured secret key.
            ValidateIssuerSigningKey = true,

            // Expected token issuer.
            ValidIssuer = jwtSettings["Issuer"],

            // Expected token audience.
            ValidAudience = jwtSettings["Audience"],

            // Secret key used to verify the token signature.
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),

            // Do not allow extra time after the token expiration date.
            ClockSkew = TimeSpan.Zero
        };
    });



builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create the initial application roles and administrator account.
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    await IdentitySeeder.SeedAsync(
        userManager,
        roleManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();    // Authenticate the user from the JWT token before authorization is checked.
app.UseAuthorization();     // Check whether the authenticated user has permission to access the endpoint.

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

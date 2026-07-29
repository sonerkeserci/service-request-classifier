using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.OpenApi;
using RequestClassifier.Application.Interfaces;
using RequestClassifier.Application.Services;
using RequestClassifier.Domain.Entities;
using RequestClassifier.Infrastructure.Data;
using RequestClassifier.Infrastructure.Data.Seed;
using RequestClassifier.ML.Models;
using RequestClassifier.ML.Services;
using System.Text;
using RequestClassifier.Application.Settings;

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

builder.Services.AddSwaggerGen(options =>
{
    // Define JWT Bearer authentication in the Swagger document
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter the JWT token."
    });

    // Allow Swagger requests to include the JWT Bearer token.
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [
            new OpenApiSecuritySchemeReference(
                "Bearer",
                document)
        ] = []
    });
});

/* ML scope */

var modelPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "MLModels",
    "service-request-model.zip");

// Stop application startup when the trained model file is missing.
if (!File.Exists(modelPath))
{
    throw new FileNotFoundException(
        $"The ML model could not be found at: {modelPath}");
}

// Load the model once during application startup so its output schema
// can be inspected and category names can be matched with Score indexes.
var metadataMlContext = new MLContext();

var metadataModel = metadataMlContext.Model.Load(
    modelPath,
    out var modelInputSchema);

// Generate the output schema produced by the trained model.
var modelOutputSchema = metadataModel.GetOutputSchema(modelInputSchema);

// Find the Score column containing one score for every category.
var scoreColumn = modelOutputSchema["Score"];

// VBuffer is ML.NET's vector structure.
// ReadOnlyMemory<char> represents each category name as read-only character data.
VBuffer<ReadOnlyMemory<char>> scoreSlotNames = default;

scoreColumn.GetSlotNames(ref scoreSlotNames);

var modelCategoryNames = scoreSlotNames
    .DenseValues()
    .Select(categoryName => categoryName.ToString()) // Convert model category names into normal strings while preserving exactly the same order used by the Score vector.
    .ToArray();

// Ensure that category metadata exists before predictions are served.
if (modelCategoryNames.Length == 0)
{
    throw new InvalidOperationException(
        "No category names were found in the ML model Score metadata.");
}

// Register model metadata as a singleton because it never changes while the application is running.
builder.Services.AddSingleton(
    new ServiceRequestModelMetadata
    {
        CategoryNames = modelCategoryNames
    });

// Register a thread-safe prediction engine pool that loads the trained model.
builder.Services
    .AddPredictionEnginePool<
        ServiceRequestTrainingData,
        ServiceRequestPrediction>()
    .FromFile(
        modelName: ServiceRequestPredictor.ModelName,
        filePath: modelPath,
        watchForChanges: false);

// Register the service used by the application to request category predictions.
builder.Services.AddScoped<
    IServiceRequestPredictor,
    ServiceRequestPredictor>();

// Bind the MachineLearning section from appsettings.json
// to the MachineLearningSettings class.
builder.Services.Configure<MachineLearningSettings>(
    builder.Configuration.GetSection("MachineLearning"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed departments, categories, roles, and the initial administrator account.
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await DatabaseSeeder.SeedAsync(context);

    await IdentitySeeder.SeedAsync(userManager, roleManager);
}

app.UseHttpsRedirection();

app.UseAuthentication();    // Authenticate the user from the JWT token before authorization is checked.
app.UseAuthorization();     // Check whether the authenticated user has permission to access the endpoint.

app.MapControllers();

app.Run();

using Microsoft.EntityFrameworkCore;
using ERPChatbotAssistant.Server.Data;
using ERPChatbotAssistant.Server.Services;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;


var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Console.WriteLine(env);

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "ERP Chatbot Assistant API", 
        Version = "v1",
        Description = "API for the ERP Chatbot Assistant"
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5174",
            "https://localhost:5174",
            "http://localhost:3000",
            "https://localhost:3000"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Configure HttpClient for ModelTrainingService
builder.Services.AddHttpClient<ModelTrainingService>(client =>
{
    client.BaseAddress = new Uri("https://openrouter.ai/");
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddScoped<ModelTrainingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Chatbot Assistant API V1");
        c.RoutePrefix = "swagger";
    });
    
    // Disable HTTPS redirection in development
    app.UseDeveloperExceptionPage();
}

// Use CORS
app.UseCors("AllowFrontend");

// Use HTTPS redirection in production only
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

// Configure the port
app.Urls.Add("http://localhost:5000");
app.Urls.Add("https://localhost:5001");

// Add error handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred");
        
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred. Please try again later." });
    }
});
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var modelTrainingService = services.GetRequiredService<ModelTrainingService>();
//    await modelTrainingService.UpdateAllTrainingDataEmbeddings();
//}
app.Run();

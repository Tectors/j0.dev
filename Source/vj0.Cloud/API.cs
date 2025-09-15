using System.Runtime.InteropServices;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.EntityFrameworkCore;

using Serilog;

using vj0.Cloud.Controllers;

namespace vj0.Cloud;

public partial class CloudWebAPI : ObservableObject
{
    private WebApplication? app;
    
    [ObservableProperty] private bool isRunning;
    [ObservableProperty] private bool hasErrored;
    
    public event Action<string, string, string>? OnError;
    public event Action<string>? OnInitialized;
    
    public async Task Run()
    {
        ResetStatus();
        
        await StopAsync();
        
        var builder = WebApplication.CreateBuilder([]);
        var services = builder.Services;

        builder.Services.AddControllers().AddApplicationPart(typeof(CloudApiController).Assembly);
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        builder.Logging.AddFilter((_, _) => false);
        
        services.AddDbContext<DbContext>(opt => opt.UseInMemoryDatabase("vj0.Cloud.Controllers"));
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            });
        });

        app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        app.Use(async (context, next) =>
        {
            await next();

            var path = context.Request.Path + context.Request.QueryString;
            var statusCode = context.Response.StatusCode;

            Log.Information("[vj0.Cloud]: {Path} | {StatusCode}", path, statusCode);
        });

        var URL = $"http://localhost:1500";
        
        Log.Information($"Framework: {RuntimeInformation.FrameworkDescription}");
        Log.Information($"vj0.Cloud is running in the background: {URL}");
        
        try
        {
            IsRunning = true;
            
            await app.RunAsync(URL);
        }
        catch (IOException ex) when (ex.Message.Contains("address already in use", StringComparison.OrdinalIgnoreCase))
        {
            Log.Error($"Failed to start vj0.Cloud: Address already in use ({URL})");
            
            IsRunning = false;
            HasErrored = true;
            
            OnError?.Invoke("Failed to start API", $"Failed to start the Cloud Web API", $"Address already in use ({URL})");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unexpected error while starting vj0.Cloud");
        }
    }
    
    public async Task StopAsync()
    {
        if (app is not null)
        {
            Log.Information("Stopping vj0.Cloud...");
            await app.StopAsync();

            ResetStatus();
            
            Log.Information("vj0.Cloud stopped");
        }
    }

    private void ResetStatus()
    {
        IsRunning = false;
        HasErrored = false;
    }
}

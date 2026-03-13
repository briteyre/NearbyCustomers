using CoreCodeCamp;
using CoreCodeCamp.Data;
using CoreCodeCamp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Linq;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        // Only add SQL Server in non-test environments
        if (!builder.Environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddSqlServerDbContext<CampContext>("CodeCamp");
        }

        builder.Services.AddScoped<ICampRepository, CampRepository>();
        builder.Services.AddScoped<ICampService, CampService>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        // Configure JWT Bearer options for detailed error messages
        builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
                OnForbidden = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Forbidden - User authenticated but lacks required permissions");
                return Task.CompletedTask;
            }
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new CoreCodeCamp.Infrastructure.NullableDateTimeConverter());
        });

        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<CoreCodeCamp.Services.Validators.CreateSpeakerRequestValidator>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
        {
              policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
          });
        });


        builder.Services.AddSwaggerDocumentation();

        var app = builder.Build();

        await app.EnsureDatabaseAsync();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors("AllowAll");

        // Add JSON exception handling middleware early in the pipeline
        app.UseMiddleware<CoreCodeCamp.Infrastructure.JsonExceptionHandlingMiddleware>();

        // Serve minimal frontend (SPA) from wwwroot
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");

        app.UseSwaggerDocumentation();

        app.UseAuthentication();
        app.UseAuthorization();

        // Minimal API Endpoints
        app.MapGet("/api/values", async (ICampService campService) =>
        {
            var camps = await campService.GetAllCampsAsync();
            return Results.Ok(camps.Select(c => c.Name));
        })
        .WithName("GetCamps")
        .AllowAnonymous();

        app.MapGet("/api/camps", async (ICampService campService) =>
        {
            var camps = await campService.GetAllCampsAsync();
            return Results.Ok(camps.Select(c => new { c.CampId, c.Name, c.City, c.EventDate, c.Length }));
        })
        .WithName("GetCampDetails")
        .AllowAnonymous();

        app.MapPost("/api/values", async (CreateCampRequest request, ICampService campService) =>
        {
            var camp = await campService.CreateCampAsync(request);
            return Results.Created($"/api/values/{camp.City}", new { success = true });
        })
        .AddEndpointFilter(async (context, next) => await ValidationFilter<CreateCampRequest>(context, next))
        .WithName("CreateCamp")
        .AllowAnonymous();

        app.MapPut("/api/values/{city}", async (string city, UpdateCampRequest request, ICampService campService) =>
        {
            var result = await campService.UpdateCampAsync(city, request);
            if (result)
            {
                return Results.Ok(new { success = true });
            }
            return Results.NotFound(new { success = false });
        })
        .AddEndpointFilter(async (context, next) => await ValidationFilter<UpdateCampRequest>(context, next))
        .WithName("UpdateCamp")
        .RequireAuthorization();

        app.MapPost("/api/speakers", async (CreateSpeakerRequest request, ICampService campService) =>
        {
            var speaker = await campService.CreateSpeakerAsync(request);
            return Results.Created($"/api/speakers/{speaker.SpeakerId}", new { success = true });
        })
        .AddEndpointFilter(async (context, next) => await ValidationFilter<CreateSpeakerRequest>(context, next))
        .WithName("CreateSpeaker")
        .AllowAnonymous();

        app.MapPut("/api/speakers/{speakerId}", async (int speakerId, UpdateSpeakerRequest request, ICampService campService) =>
        {
            var result = await campService.UpdateSpeakerAsync(speakerId, request);
            if (result)
            {
                return Results.Ok(new { success = true });
            }
            return Results.NotFound(new { success = false });
        })
        .AddEndpointFilter(async (context, next) => await ValidationFilter<UpdateSpeakerRequest>(context, next))
        .WithName("UpdateSpeaker")
        .RequireAuthorization();

        app.MapGet("/api/speakers", async (ICampService campService) =>
        {
            var speakers = await campService.GetAllSpeakersAsync();
            return Results.Ok(speakers);
        })
        .WithName("GetSpeakers")
        .AllowAnonymous();

        app.MapDelete("/api/speakers/{firstName}/{lastName}", async (string firstName, string lastName, ICampService campService) =>
        {
            var deleted = await campService.DeleteSpeakerByNameAsync(firstName, lastName);
            if (deleted)
            {
                return Results.Ok(new { success = true });
            }
            return Results.NotFound(new { success = false });
        })
        .WithName("DeleteSpeaker")
        .AllowAnonymous();

        app.MapDefaultEndpoints();

        // Add a reusable validation filter for minimal API endpoints
        static async ValueTask<object?> ValidationFilter<T>(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
            if (validator is not null)
            {
                var request = context.Arguments.OfType<T>().FirstOrDefault();
                if (request is not null)
                {
                    var result = await validator.ValidateAsync(request);
                    if (!result.IsValid)
                    {
                        // Group errors by property and return a ValidationProblem (detailed per-property errors)
                        var errors = result.Errors
                            .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "_" : e.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                        return Results.ValidationProblem(errors);
                    }
                }
            }
            return await next(context);
        }

        await app.RunAsync();
    }
}

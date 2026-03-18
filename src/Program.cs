using CoreCodeCamp;
using CoreCodeCamp.Data;
using CoreCodeCamp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using FluentValidation;
using FluentValidation.AspNetCore;
using Polly;
using Polly.Timeout;

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

        builder.Services.AddDistributedMemoryCache();

        // Bind cache settings from configuration (section: "Cache")
        builder.Services.Configure<CoreCodeCamp.Services.CacheSettings>(builder.Configuration.GetSection("Cache"));

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

        // Configure Ollama settings and service
        var ollamaSettings = new OllamaSettings();
        builder.Configuration.GetSection("Ollama").Bind(ollamaSettings);
        builder.Services.AddSingleton(ollamaSettings);
        // Create a per-client Polly timeout policy (60s) to override shorter defaults for Ollama
        var ollamaTimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(60), TimeoutStrategy.Pessimistic);

        builder.Services.AddHttpClient<IOllamaService, OllamaService>(c =>
        {
            c.BaseAddress = new Uri(ollamaSettings.BaseUrl);
            // increase HttpClient timeout for Ollama requests
            c.Timeout = TimeSpan.FromSeconds(60);
        })
        .AddPolicyHandler(ollamaTimeoutPolicy);

        // Register the simple in-memory knowledge base for local experiments
        builder.Services.AddSingleton<IKnowledgeBase, InMemoryKnowledgeBase>();

        var app = builder.Build();

        await app.EnsureDatabaseAsync();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors("AllowAll");

        // Add JSON exception handling middleware early in the pipeline
        app.UseMiddleware<CoreCodeCamp.Infrastructure.JsonExceptionHandlingMiddleware>();

        // Add Chaos middleware to inject latency/errors when enabled in configuration
        app.UseMiddleware<CoreCodeCamp.Infrastructure.ChaosMiddleware>();

        // Serve minimal frontend (SPA) from wwwroot
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");

        app.UseSwaggerDocumentation();

        app.UseAuthentication();
        app.UseAuthorization();

        // Minimal API Endpoints
        app.MapGet("/api/camps", async (ICampService campService) =>
        {
            var camps = await campService.GetAllCampsAsync();
            return Results.Ok(camps.Select(c => new { c.CampId, c.Name, c.City, c.EventDate, c.Length }));
        })
        .WithName("GetCamps")
        .AllowAnonymous();

        app.MapPost("/api/camps", async (CreateCampRequest request, ICampService campService) =>
        {
            var camp = await campService.CreateCampAsync(request);
            return Results.Created($"/api/camps/{camp.City}", new { success = true });
        })
        .AddEndpointFilter(async (context, next) => await ValidationFilter<CreateCampRequest>(context, next))
        .WithName("CreateCamp")
        .AllowAnonymous();

        app.MapPut("/api/camps/{city}", async (string city, UpdateCampRequest request, ICampService campService) =>
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

        app.MapPost("/api/llm/chat", async (ChatRequest request, IOllamaService ollama) =>
        {
            var reply = await ollama.AskAsync(request.Message);
            return Results.Ok(new { reply });
        })
        .WithName("Chat")
        .AllowAnonymous();

        // Simple admin endpoints to manage the in-memory knowledge base used for experiments.
        app.MapPost("/api/llm/docs", async (KnowledgeDocument doc, IKnowledgeBase kb) =>
        {
            if (string.IsNullOrWhiteSpace(doc?.Id)) return Results.BadRequest(new { error = "id required" });
            await kb.AddDocumentAsync(doc.Id, doc.Text ?? string.Empty);
            return Results.Created($"/api/llm/docs/{doc.Id}", doc);
        })
        .WithName("AddKnowledgeDocument")
        .AllowAnonymous();

        app.MapGet("/api/llm/docs", async (IKnowledgeBase kb) =>
        {
            var docs = await kb.ListDocumentsAsync();
            return Results.Ok(docs);
        })
        .WithName("ListKnowledgeDocuments")
        .AllowAnonymous();

        // Upload a document file (text/markdown/json/html) and add its text to the knowledge base.
        app.MapPost("/api/llm/docs/upload", async (HttpContext ctx, IKnowledgeBase kb) =>
        {
            var request = ctx.Request;
            if (!request.HasFormContentType) return Results.BadRequest(new { error = "Expected multipart/form-data" });

            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();
            if (file is null) return Results.BadRequest(new { error = "file required" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            // Only support simple text-based files for this demo
            var allowed = new[] { ".txt", ".md", ".json", ".html", ".htm" };
            if (!allowed.Contains(ext))
            {
                return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
            }

            string content;
            using (var sr = new StreamReader(file.OpenReadStream()))
            {
                content = await sr.ReadToEndAsync();
            }

            var id = Path.GetFileNameWithoutExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(id)) id = Guid.NewGuid().ToString("N");

            await kb.AddDocumentAsync(id, content);

            return Results.Created($"/api/llm/docs/{id}", new KnowledgeDocument(id, content));
        })
        .WithName("UploadKnowledgeDocument")
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

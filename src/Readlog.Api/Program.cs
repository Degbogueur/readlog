using Microsoft.EntityFrameworkCore;
using Readlog.Api.Extensions;
using Readlog.Api.Handlers;
using Readlog.Application;
using Readlog.Infrastructure;
using Readlog.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddRepositories();
builder.Services.AddServices();

builder.Services.AddAuthentication(builder.Configuration)
                .AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Readlog API</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #fff;
        }
        
        .container {
            text-align: center;
            padding: 2rem;
            max-width: 600px;
        }
        
        .logo {
            font-size: 4rem;
            margin-bottom: 1rem;
        }
        
        h1 {
            font-size: 2.5rem;
            margin-bottom: 0.5rem;
            background: linear-gradient(90deg, #e94560, #ff6b6b);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }
        
        .version {
            color: #8892b0;
            font-size: 0.9rem;
            margin-bottom: 2rem;
        }
        
        .status {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            background: rgba(255, 255, 255, 0.1);
            padding: 0.75rem 1.5rem;
            border-radius: 50px;
            margin-bottom: 2rem;
        }
        
        .status-dot {
            width: 10px;
            height: 10px;
            background: #4ade80;
            border-radius: 50%;
            animation: pulse 2s infinite;
        }
        
        @keyframes pulse {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.5; }
        }
        
        .description {
            color: #a8b2d1;
            line-height: 1.6;
            margin-bottom: 2rem;
        }
        
        .links {
            display: flex;
            gap: 1rem;
            justify-content: center;
            flex-wrap: wrap;
        }
        
        .link {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1.5rem;
            border-radius: 8px;
            text-decoration: none;
            font-weight: 500;
            transition: all 0.3s ease;
        }
        
        .link-primary {
            background: linear-gradient(90deg, #e94560, #ff6b6b);
            color: #fff;
        }
        
        .link-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(233, 69, 96, 0.3);
        }
        
        .link-secondary {
            background: rgba(255, 255, 255, 0.1);
            color: #fff;
            border: 1px solid rgba(255, 255, 255, 0.2);
        }
        
        .link-secondary:hover {
            background: rgba(255, 255, 255, 0.2);
            transform: translateY(-2px);
        }
        
        .tech-stack {
            margin-top: 3rem;
            padding-top: 2rem;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
        }
        
        .tech-stack h3 {
            color: #8892b0;
            font-size: 0.8rem;
            text-transform: uppercase;
            letter-spacing: 2px;
            margin-bottom: 1rem;
        }
        
        .tech-badges {
            display: flex;
            gap: 0.5rem;
            justify-content: center;
            flex-wrap: wrap;
        }
        
        .badge {
            background: rgba(255, 255, 255, 0.1);
            padding: 0.4rem 0.8rem;
            border-radius: 4px;
            font-size: 0.8rem;
            color: #ccd6f6;
        }
        
        .footer {
            margin-top: 3rem;
            color: #495670;
            font-size: 0.8rem;
        }
        
        .footer a {
            color: #e94560;
            text-decoration: none;
        }
        
        .footer a:hover {
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="logo">📚</div>
        <h1>Readlog API</h1>
        <p class="version">v1.0.0</p>
        
        <div class="status">
            <span class="status-dot"></span>
            <span>API is running</span>
        </div>
        
        <p class="description">
            A RESTful API for managing your book reviews and reading lists. 
            Built with Clean Architecture, Domain-Driven Design, and CQRS patterns.
        </p>
        
        <div class="links">
            <a href="/swagger" class="link link-primary">
                📖 API Documentation
            </a>
            <a href="https://github.com/Degbogueur/readlog" class="link link-secondary" target="_blank">
                ⭐ GitHub Repository
            </a>
        </div>
        
        <div class="tech-stack">
            <h3>Built with</h3>
            <div class="tech-badges">
                <span class="badge">.NET 8</span>
                <span class="badge">ASP.NET Core</span>
                <span class="badge">Entity Framework Core</span>
                <span class="badge">SQL Server</span>
                <span class="badge">MediatR</span>
                <span class="badge">JWT Auth</span>
            </div>
        </div>
        
        <div class="footer">
            Made with ❤️ by <a href="https://github.com/Degbogueur" target="_blank">Komi Degbo</a>
        </div>
    </div>
</body>
</html>
""", "text/html")).ExcludeFromDescription();

app.Run();

public partial class Program { }
using FedePortfolio.Web.Components;
using FedePortfolio.Web.Models;
using FedePortfolio.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IGitHubService, GitHubService>();

builder.Services.AddSingleton<MarkdownRenderer>();
builder.Services.AddSingleton<INotesService, NotesService>();
builder.Services.AddSingleton(sp => BuildSiteMetadata(sp, builder.Configuration, builder.Environment));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>();

app.Run();

static SiteMetadata BuildSiteMetadata(IServiceProvider sp, IConfiguration cfg, IHostEnvironment env)
{
    var projects = new List<Project>
    {
        new()
        {
            Name = "ForgeManager",
            Url = "https://github.com/FedeCF03/ForgeManager",
            Description = "API distribuida integrada con microservicios. Comunicación en tiempo real y persistencia SQL.",
            Stack = ".NET, ASP.NET Core, SQL Server, SignalR"
        },
        new()
        {
            Name = "SGE — Sistema de Gestión Escolar",
            Url = "https://github.com/FedeCF03",
            Description = "Proyecto académico (UNLP). Arquitectura con perfiles, roles y control de accesos para integridad de datos.",
            Stack = "C#, .NET, SQL"
        }
    };

    var experiences = new List<ExperienceItem>
    {
        new()
        {
            SortOrder = 1,
            Company = "Trabajo Independiente — Herrería de obra",
            Role = "Gestión de proyectos y logística",
            Period = "2018 – Actualidad",
            Description = "Planificación técnica, cálculo de materiales, presupuestos y armado logístico para entregas en obra. Diagnóstico y resolución de imprevistos sobre la marcha.",
            Stack = "Presupuestos, planificación, trato con clientes"
        },
        new()
        {
            SortOrder = 2,
            Company = "Facultad de Informática — UNLP",
            Role = "Estudiante avanzado de Informática",
            Period = "Mar 2023 – Actualidad",
            Description = "Materias de transmisión de datos, protocolos de transporte, seguridad informática y administración de arquitecturas distribuidas. Estudiante de 4to año.",
            Stack = "Redes, sistemas operativos, arquitectura distribuida"
        },
        new()
        {
            SortOrder = 3,
            Company = "ForgeManager",
            Role = "Proyecto personal",
            Period = "2024 – Actualidad",
            Description = "API distribuida integrada con microservicios. Configuración del flujo de datos internos, comunicación en tiempo real mediante protocolos web y persistencia en SQL.",
            Stack = ".NET, microservicios, WebSockets, SQL"
        }
    };

    var socials = new List<SocialLink>
    {
        new() { Label = "Email", Handle = "fedecf03@gmail.com", Url = "mailto:fedecf03@gmail.com" },
        new() { Label = "GitHub", Handle = "@FedeCF03", Url = "https://github.com/FedeCF03", Rel = "me" },
        new() { Label = "LinkedIn", Handle = "Federico Caltabiano", Url = "https://www.linkedin.com/in/federico-caltabiano-folino/", Rel = "me" }
    };

    var commit = Environment.GetEnvironmentVariable("GIT_COMMIT")
                 ?? cfg["Site:Commit"]
                 ?? env.ContentRootPath;
    var lastMod = File.GetLastWriteTimeUtc(System.Reflection.Assembly.GetExecutingAssembly().Location);

    return new SiteMetadata
    {
        Name = "Federico Caltabiano Folino",
        Role = "Estudiante avanzado de Informática · Infraestructura & Networking",
        Location = "La Plata, Buenos Aires",
        Tagline = "Estudiante de informática construyendo infraestructura, redes y herramientas en La Plata.",
        HeroDescription = "Aprendiz perpetuo de redes, sistemas distribuidos y software que resuelve problemas reales. Me obsesionan los protocolos, el routing y los sistemas que simplemente andan.",
        Email = "fedecf03@gmail.com",
        GitHubUsername = "FedeCF03",
        Projects = projects,
        Experiences = experiences.OrderBy(e => e.SortOrder).ToList(),
        Socials = socials,
        SiteVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
        CommitSha = commit,
        LastModified = lastMod
    };
}

public partial class Program;

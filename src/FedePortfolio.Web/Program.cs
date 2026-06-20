using FedePortfolio.Web.Components;
using FedePortfolio.Web.Models;
using FedePortfolio.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents();
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
            Name = "NetworkMonitorTUI",
            Url = "https://github.com/FedeCF03/NetworkMonitorTUI",
            Description = "Monitor de red en tiempo real para la terminal (TUI) escrito en C#. Permite el seguimiento de tráfico, ping y diagnóstico de conectividad directamente desde la consola Linux sin entorno gráfico.",
            Stack = "C#, .NET, TUI (Terminal.Gui), Linux, Networking"
        },
        new()
        {
            Name = "SGE — Sistema de Gestión Escolar",
            Url = "https://github.com/FedeCF03",
            Description = "Proyecto académico (UNLP). Diseño de arquitectura monolítica con control de accesos basado en roles y perfiles, garantizando consistencia, concurrencia e integridad de datos.",
            Stack = "C#, .NET, Entity Framework, SQL Server"
        }
    };

  var experiences = new List<ExperienceItem>
    {
        new()
        {
            SortOrder = 1,
            Company = "Trabajo Independiente — Herrería de obra",
            Role = "Gestión de Proyectos, Logística y Diseño Técnico",
            Period = "2018 – Actualidad",
            Description = "Liderazgo operativo en diseño y construcción de estructuras metálicas a medida. Responsable del cálculo técnico de materiales, presupuestos, optimización de recursos y logística de entrega. Resolución ágil de contingencias críticas en obra y gestión directa con clientes.",
            Stack = "Planificación técnica, Gestión de recursos, Resolución de problemas, Negociación"
        },
        new()
        {
            SortOrder = 2,
            Company = "Facultad de Informática — UNLP",
            Role = "Estudiante Avanzado de Informática",
            Period = "Mar 2023 – Actualidad",
            Description = "Formación sólida orientada al backend y la infraestructura. Foco en transmisión de datos, protocolos de transporte, programación paralela/concurrente (OpenMP, Pthreads), sistemas operativos y administración de sistemas paralelos.",
            Stack = "Redes (TCP/IP), Linux/Unix, Sistemas Distribuidos, Programación Concurrente"
        }
    };

  var socials = new List<SocialLink>
    {
        new() { Label = "Email", Handle = "fedecf03@gmail.com", Url = "mailto:fedecf03@gmail.com" },
        new() { Label = "GitHub", Handle = "@FedeCF03", Url = "https://github.com/FedeCF03", Rel = "me" },
        new() { Label = "LinkedIn", Handle = "Federico Caltabiano", Url = "https://www.linkedin.com/in/federico-caltabiano/", Rel = "me" }
    };

  var commit = Environment.GetEnvironmentVariable("GIT_COMMIT")
                ?? cfg["Site:Commit"]
                ?? env.ContentRootPath;
  var lastMod = File.GetLastWriteTimeUtc(System.Reflection.Assembly.GetExecutingAssembly().Location);

  return new SiteMetadata
  {
    Name = "Federico Caltabiano",
    Role = "Estudiante Avanzado de Informática · Infraestructura, Redes & Backend",
    Location = "La Plata, Buenos Aires",
    Tagline = "Sistemas, redes e infraestructura.",
    HeroDescription = "Estudiante avanzado en la UNLP con foco en sistemas operativos, redes y desarrollo backend. Tengo experiencia práctica gestionando recursos, planificando proyectos independientes y resolviendo imprevistos bajo presión.",
    Email = "fedecf03@gmail.com",
    Projects = projects,
    Experiences = experiences.OrderBy(e => e.SortOrder).ToList(),
    Socials = socials,
    SiteVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
    CommitSha = commit,
    LastModified = lastMod
  };
}

public partial class Program;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FedePortfolio.Web.Components;
using FedePortfolio.Web.Models;
using FedePortfolio.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddSingleton(sp => BuildSiteMetadata(sp, builder.Configuration, builder.HostEnvironment));

await builder.Build().RunAsync();

static SiteMetadata BuildSiteMetadata(IServiceProvider sp, IConfiguration cfg, IWebAssemblyHostEnvironment env)
{
  var projects = new List<Project>
    {
        new()
        {
            Name = "Alquileres Express",
            Url = "https://github.com/FedeCF03/Alquileres_Express",
            Description = "API RESTful en C# y Entity Framework Core para la gestión integral de alojamientos, usuarios y disponibilidad. Modelado de dominio, validación de reservas concurrentes y persistencia relacional en SQL Server.",
            Stack = "C#, ASP.NET Core, EF Core, SQL Server, REST API"
        },
        new()
        {
            Name = "msprime-benchmarks",
            Url = "https://github.com/FedeCF03/msprime-benchmarks",
            Description = "Suite de benchmarking y profiling en Python para evaluar el rendimiento de simulaciones genéticas poblacionales con msprime y tskit. Automatización de corridas en paralelo, métricas reproducibles y análisis de escalabilidad en Linux.",
            Stack = "Python, msprime, tskit, Bash, Linux, HPC"
        }
    };

  var experiences = new List<ExperienceItem>
    {
        new()
        {
            SortOrder = 1,
            Company = "Instituto de Investigación III-LIDI — UNLP",
            Role = "Desarrollador de Software / Investigador (PPS)",
            Period = "Ago. 2026 – Actualidad",
            Description = "Desarrollo de herramientas de benchmarking y profiling en Python para evaluar algoritmos paralelos en simulaciones genéticas complejas (msprime, tskit). Análisis de métricas de rendimiento, memoria y escalabilidad en entornos HPC, con pipelines reproducibles.",
            Stack = "Python, msprime, tskit, HPC, Benchmarking, Linux"
        },
        new()
        {
            SortOrder = 2,
            Company = "Desarrollo de Software Freelance / Consultoría Técnica",
            Role = "Desarrollador Backend / Full-Stack",
            Period = "Ene. 2025 – Actualidad",
            Description = "Diseño de aplicaciones web y microservicios con principios SOLID, Clean Architecture y buenas prácticas de modularidad. Gestión del ciclo completo de proyectos: relevamiento, modelado relacional, despliegues CI/CD y auditoría de datos.",
            Stack = "C#, ASP.NET Core, SQL Server, Docker, CI/CD, CQRS/MediatR"
        },
        new()
        {
            SortOrder = 3,
            Company = "Facultad de Informática — UNLP",
            Role = "Licenciatura en Informática · Analista TIC",
            Period = "2023 – Presente",
            Description = "Formación centrada en ciencias de la computación, arquitectura de software, bases de datos y sistemas paralelos, con título intermedio de Analista TIC.",
            Stack = "Algoritmos, Sistemas Paralelos, Bases de Datos, Arquitectura de Software"
        }
    };

  var socials = new List<SocialLink>
    {
        new() { Label = "Email", Handle = "fedecf03@gmail.com", Url = "mailto:fedecf03@gmail.com" },
        new() { Label = "GitHub", Handle = "@FedeCF03", Url = "https://github.com/FedeCF03", Rel = "me" },
        new() { Label = "LinkedIn", Handle = "Federico Caltabiano", Url = "https://www.linkedin.com/in/federico-caltabiano/", Rel = "me" }
    };

  var commit = cfg["Site:Commit"] ?? "local";

  return new SiteMetadata
  {
    Name = "Federico Caltabiano",
    Role = "Estudiante de Informática · Backend, sistemas paralelos & HPC",
    Location = "La Plata, Buenos Aires",
    Tagline = "Backend, sistemas paralelos e infraestructura.",
    HeroDescription = "Estudiante avanzado de la Licenciatura en Informática (UNLP) con foco en algoritmia, sistemas paralelos y desarrollo de software. Trabajo con .NET, APIs, SQL y Docker sobre Linux, y participo en investigación aplicada a optimización computacional.",
    Email = "fedecf03@gmail.com",
    Projects = projects,
    Experiences = experiences.OrderBy(e => e.SortOrder).ToList(),
    Socials = socials,
    CvPath = "Federico_Caltabiano.pdf",
    SiteVersion = "1.0.0",
    CommitSha = commit,
    LastModified = null
  };
}

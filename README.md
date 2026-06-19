# Federico Caltabiano Folino · Portfolio

Portfolio personal construido con **Blazor Web App** (.NET 10) y **Static SSR**. Diseño brutalista/monocromático, CSS puro scoped por componente, contenido en markdown.

## Stack

- **.NET 10** + Blazor Web App (Static SSR, sin interactividad de cliente)
- **CSS puro** con design tokens en `wwwroot/app.css` y archivos `.razor.css` por componente
- **Markdig** para renderizar notas en markdown
- **YamlDotNet** para parsear frontmatter
- **HttpClient** + **IMemoryCache** para integrarse con la API de GitHub (con caché de 30 min)
- Sin JavaScript de cliente, sin WebAssembly, sin Tailwind

## Estructura

```
src/FedePortfolio.Web/
├── Components/
│   ├── App.razor                  # HTML shell + <head>
│   ├── Routes.razor               # Router
│   ├── Layout/MainLayout.razor    # Header + main + footer
│   └── Pages/
│       ├── Home.razor             # /
│       ├── Notes.razor            # /notes
│       ├── NoteDetail.razor       # /notes/{slug}
│       ├── NotFound.razor         # /not-found
│       └── Error.razor            # /error
├── Services/
│   ├── SiteMetadata.cs            # Datos del sitio
│   ├── NotesService.cs            # Lee Content/notes/*.md
│   ├── GitHubService.cs           # PRs propios + OSS via API
│   ├── MarkdownRenderer.cs        # Markdig pipeline
│   └── FrontmatterParser.cs       # YamlDotNet helper
├── Models/                        # Note, PullRequest, Project, etc.
├── Content/notes/                 # *.md con frontmatter
└── wwwroot/
    ├── app.css                    # Design tokens + reset
    └── favicon.svg
```

## Agregar una nota

Crear un archivo `Content/notes/YYYY-MM-DD-slug.md`:

```markdown
---
title: "Título de la nota"
date: 2026-06-19
tags: [networking, linux]
---

Contenido en markdown. Se renderiza con Markdig (soporta tablas, code blocks con highlighting, listas, blockquotes, etc.)
```

El slug se infiere del nombre del archivo y la fecha se toma del frontmatter (o del prefijo `YYYY-MM-DD` del nombre si no hay frontmatter).

## Ejecutar localmente

```sh
cd src/FedePortfolio.Web
dotnet run
# http://localhost:5151
```

## Build de producción

```sh
dotnet publish src/FedePortfolio.Web -c Release -o publish
```

El output `publish/wwwroot/` es un sitio estático que se puede servir desde cualquier host (GitHub Pages, Cloudflare Pages, Netlify, Azure Static Web Apps, S3 + CloudFront, etc.).

## Variables de entorno

- `GIT_COMMIT` — SHA del commit actual, mostrado en el footer. Inyectar en CI.

## Deploy sugerido

- **Cloudflare Pages** o **Netlify**: conectar el repo, build command `dotnet publish src/FedePortfolio.Web -c Release -o publish`, publish directory `publish/wwwroot`.
- **GitHub Pages**: agregar un workflow que corra el publish y suba `wwwroot/` a la rama `gh-pages`.
- **Azure Static Web Apps**: build command `dotnet publish`, artifact path `publish/wwwroot`.

## Licencia

MIT.

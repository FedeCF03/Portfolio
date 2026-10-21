# Federico Caltabiano · Portfolio

Portfolio personal construido con **Blazor WebAssembly** (.NET 10) y **CSS puro**. Diseño editorial minimalista, tipografías self-hosted y modo claro/oscuro automático.

## Stack

- **.NET 10** + Blazor WebAssembly
- **CSS puro** con design tokens en `wwwroot/app.css` y archivos `.razor.css` por componente
- Tipografías self-hosted: **Instrument Serif** (display) e **Inter** (texto)
- Sin frameworks de CSS, sin JS de cliente adicional y sin dependencias de CDNs

## Diseño

- Paleta cálida de papel y tinta con un acento terracota
- Modo oscuro automático vía `prefers-color-scheme`
- Jerarquía editorial: serif para títulos, sans para lectura y mono para metadatos
- Microinteracciones y animaciones sutiles, respetando `prefers-reduced-motion`
- Layout responsive sin JavaScript
- Descarga del CV en PDF desde el header, el hero y el bloque de contacto

## Estructura

```
src/FedePortfolio.Web/
├── Components/
│   ├── App.razor                  # Router
│   ├── Layout/MainLayout.razor    # Header + main + footer
│   └── Pages/
│       ├── Home.razor             # /
│       ├── NotFound.razor         # /not-found
│       └── Error.razor            # /error
├── Models/                        # Proyectos, experiencia y links
├── Services/
│   ├── SiteMetadata.cs            # Datos del sitio
│   └── FrontmatterParser.cs       # Helper de YAML
└── wwwroot/
    ├── app.css                    # Tokens, tipografías y reset
    ├── fonts/                     # WOFF2 self-hosted
    ├── Federico_Caltabiano.pdf    # CV descargable
    └── favicon.svg
```

El contenido del portfolio (perfil, proyectos, experiencia y educación) se edita en `Program.cs`. El CV descargable se sirve desde `wwwroot/Federico_Caltabiano.pdf`.

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
- **GitHub Pages**: el workflow ya publica `publish/wwwroot` en la rama `gh-pages` y ajusta el `base href` al subpath del repositorio.
- **Azure Static Web Apps**: build command `dotnet publish`, artifact path `publish/wwwroot`.
- **VM de Azure / VPS**: ver [docs/DEPLOY_AZURE_VM.md](docs/DEPLOY_AZURE_VM.md) para montar el sitio con Nginx, HTTPS y actualizaciones.

## Licencia

MIT.

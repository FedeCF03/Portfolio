# Montar el portfolio en una VM de Azure (o VPS Linux)

Guía para publicar este portfolio en una máquina virtual Linux de **Azure** usando **Nginx**, un dominio propio y HTTPS. Los mismos pasos sirven para cualquier VPS con Ubuntu o Debian.

El proyecto es **Blazor WebAssembly**: el resultado de `dotnet publish` es un sitio estático. En producción **no hace falta ejecutar .NET ni mantener un proceso corriendo**; Nginx sirve directamente los archivos de `wwwroot`.

## Resumen rápido

1. Abrir los puertos **22, 80 y 443** en el NSG de Azure.
2. Apuntar el dominio (registro A) a la IP pública de la VM.
3. Instalar Nginx y .NET 10 en la VM.
4. Clonar el repo y publicar con `dotnet publish`.
5. Copiar `publish/wwwroot` a `/var/www/portfolio`.
6. Configurar Nginx y emitir el certificado con Certbot.

## 1. Requisitos

- VM de Azure con Ubuntu 24.04 LTS (o Debian 12) y acceso por SSH con `sudo`.
- Un dominio o subdominio, por ejemplo `portfolio.tudominio.com`.
- Registro DNS **A** apuntando a la IP pública de la VM. Si la VM tiene IPv6, agregar también el registro **AAAA**.
- Puertos **22**, **80** y **443** habilitados en el Network Security Group (NSG) de Azure.

La guía usa `portfolio.tudominio.com` como ejemplo: reemplazalo por tu dominio real.

---

## 2. Configurar la VM en Azure

Si la VM ya existe, revisá estos puntos en el portal de Azure antes de instalar nada. Si está detenida, inicíala desde **Información general → Iniciar**.

Para compilar en la VM, **2 GB de RAM** (por ejemplo, `Standard_B2s`) es cómodo. Si compilás en tu máquina y solo subís los archivos, una `Standard_B1s` alcanza.

### 2.1. Puertos de entrada (NSG)

En **VM → Redes → Reglas de puerto de entrada** del Network Security Group asociado, asegurate de tener:

| Puerto | Protocolo | Origen | Uso |
| --- | --- | --- | --- |
| 22 | TCP | Tu IP (recomendado) | SSH |
| 80 | TCP | Cualquiera | HTTP y validación de Certbot |
| 443 | TCP | Cualquiera | HTTPS |

No hace falta abrir otros puertos. Si limitás el 22 a tu IP, tené en cuenta que al cambiar de red puede que necesites actualizar la regla.

También podés abrir los puertos con Azure CLI:

```bash
az vm open-port --resource-group TU_RESOURCE_GROUP --name TU_VM --port 80 --priority 1001
az vm open-port --resource-group TU_RESOURCE_GROUP --name TU_VM --port 443 --priority 1002
```

Si las reglas ya existen, no hace falta ejecutar estos comandos.

### 2.2. IP pública

En **Información general → Dirección IP pública**, comprobá que la IP sea **estática**. Si es dinámica, cambiala a estática en la configuración de la IP pública; así el registro DNS no se rompe si reiniciás la VM.

Para obtenerla con Azure CLI:

```bash
az vm show -d -g TU_RESOURCE_GROUP -n TU_VM --query publicIps -o tsv
```

### 2.3. DNS

- **Con dominio propio:** creá un registro **A** para `portfolio.tudominio.com` apuntando a la IP pública. Si el dominio está en Azure DNS, creá el registro en la zona DNS correspondiente; si está en otro proveedor, hacelo en su panel. Si usás Cloudflare, dejá el proxy en **DNS only** (nube gris) hasta emitir el certificado; después podés activar el proxy.
- **Sin dominio propio:** podés usar el nombre DNS de Azure (`TU_VM.TU_REGION.cloudapp.azure.com`) para probar por HTTP. Para HTTPS con Let's Encrypt usá un dominio propio.

### 2.4. Conectarse por SSH

```bash
ssh azureuser@IP_PUBLICA
```

Reemplazá `azureuser` por el usuario administrador que elegiste al crear la VM.

---

## 3. Preparar la VM

Una vez conectado por SSH, actualizar el sistema e instalar las herramientas necesarias:

```bash
sudo apt update
sudo apt upgrade -y
sudo apt install -y git curl rsync nginx ufw
```

### Firewall

El NSG de Azure ya filtra el tráfico de entrada. UFW es una segunda capa dentro de la VM. Si usás UFW, habilitá SSH antes que Nginx para no perder la conexión:

```bash
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw enable
sudo ufw status
```

> Si tu SSH usa un puerto distinto de 22, permitilo explícitamente antes de activar UFW.

---

## 4. Instalar .NET 10

La VM solo necesita el SDK para **compilar** el proyecto. La app publicada se sirve como archivos estáticos.

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
echo 'export PATH="$HOME/.dotnet:$PATH"' >> "$HOME/.bashrc"
source "$HOME/.bashrc"
dotnet --version
```

El comando debe devolver una versión `10.x`. Si ya tenés .NET 10 instalado globalmente, podés omitir este paso.

---

## 5. Obtener y compilar el portfolio

Crear las carpetas y clonar el repositorio:

```bash
sudo mkdir -p /srv/portfolio /var/www/portfolio
sudo chown -R "$USER":"$USER" /srv/portfolio /var/www/portfolio

git clone https://github.com/FedeCF03/Portfolio.git /srv/portfolio
```

Compilar en Release:

```bash
cd /srv/portfolio
dotnet publish src/FedePortfolio.Web/FedePortfolio.Web.csproj -c Release -o /tmp/portfolio-publish
```

Copiar únicamente el sitio estático a la carpeta que va a servir Nginx:

```bash
rsync -a --delete /tmp/portfolio-publish/wwwroot/ /var/www/portfolio/
```

Verificar que el `base href` sea `/`:

```bash
grep '<base href=' /var/www/portfolio/index.html
# Debe mostrar: <base href="/" />
```

Ajustar permisos:

```bash
find /var/www/portfolio -type d -exec chmod 755 {} \;
find /var/www/portfolio -type f -exec chmod 644 {} \;
```

> Si `dotnet publish` muestra el aviso de `wasm-tools`, el publish igual funciona. Instalarlo es opcional y solo mejora la optimización del tamaño: `dotnet workload install wasm-tools`.

### Opción alternativa: compilar en tu máquina

Si preferís no instalar el SDK en la VM, compilá localmente y subí el resultado:

```bash
dotnet publish src/FedePortfolio.Web/FedePortfolio.Web.csproj -c Release -o publish
rsync -avz --delete publish/wwwroot/ azureuser@IP_PUBLICA:/var/www/portfolio/
```

En este caso, en la VM solo necesitás Nginx y que `/var/www/portfolio` exista y pertenezca al usuario con el que hacés `rsync`:

```bash
ssh azureuser@IP_PUBLICA 'sudo mkdir -p /var/www/portfolio && sudo chown -R azureuser:azureuser /var/www/portfolio'
```

---

## 6. Configurar Nginx

Crear el sitio:

```bash
sudo tee /etc/nginx/sites-available/portfolio >/dev/null <<'EOF'
server {
    listen 80;
    # Si la VM tiene IPv6, agregar: listen [::]:80;
    server_name portfolio.tudominio.com;

    root /var/www/portfolio;
    index index.html;

    # Fallback para el Router de Blazor.
    location / {
        try_files $uri $uri/ /index.html;
    }

    # El HTML no se cachea para que los cambios se vean al instante.
    location = /index.html {
        add_header Cache-Control "no-cache";
    }

    # Assets de Blazor. El build les agrega un hash al nombre.
    location /_framework/ {
        try_files $uri =404;
        add_header Cache-Control "public, max-age=31536000, immutable";
    }

    # Asegurar el MIME correcto de WebAssembly.
    location ~* \.wasm$ {
        default_type application/wasm;
        try_files $uri =404;
        add_header Cache-Control "public, max-age=31536000, immutable";
    }

    # El CSS cambia entre deploys y no tiene hash en el nombre.
    location ~* \.css$ {
        try_files $uri =404;
        add_header Cache-Control "no-cache";
    }

    # JavaScript, fuentes e imágenes.
    location ~* \.(js|woff2|svg|png|jpe?g|webp|ico)$ {
        try_files $uri =404;
        add_header Cache-Control "public, max-age=604800";
    }

    # El CV se descarga como archivo adjunto.
    location = /Federico_Caltabiano.pdf {
        try_files $uri =404;
        add_header Content-Disposition 'attachment; filename="Federico_Caltabiano.pdf"';
    }

    gzip on;
    gzip_static on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/css application/javascript application/json application/wasm image/svg+xml;
}
EOF
```

`gzip_static` usa los archivos `.gz` que genera `dotnet publish`. Si tu Nginx no incluye ese módulo, quitá esa línea y dejá solo `gzip on`.

Activar el sitio y recargar Nginx:

```bash
sudo ln -sf /etc/nginx/sites-available/portfolio /etc/nginx/sites-enabled/portfolio
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl reload nginx
```

Si `nginx -t` falla, revisar el mensaje antes de recargar.

Probar Nginx directamente con la IP, sin depender del DNS:

```bash
curl -I -H 'Host: portfolio.tudominio.com' http://IP_PUBLICA
```

Debe devolver `200 OK`. Después, cuando el DNS ya apunte a la VM:

```bash
curl -I http://portfolio.tudominio.com
```

> Si vas a probar con el nombre DNS de Azure (`TU_VM.TU_REGION.cloudapp.azure.com`), agregalo también al `server_name` del bloque de Nginx.

---

## 7. Activar HTTPS

Antes de pedir el certificado, comprobar que el dominio ya resuelve a la VM:

```bash
getent hosts portfolio.tudominio.com
```

Debe devolver la IP pública de la VM. Si no devuelve nada, esperar la propagación del DNS o revisar el registro A/AAAA.

Instalar Certbot y generar el certificado:

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d portfolio.tudominio.com
```

Si también usás `www`, agregar el otro dominio:

```bash
sudo certbot --nginx -d portfolio.tudominio.com -d www.portfolio.tudominio.com
```

Certbot modifica la configuración de Nginx y programa la renovación automática. Verificar:

```bash
sudo certbot renew --dry-run
```

Probar el sitio final:

```bash
curl -I https://portfolio.tudominio.com
curl -I https://portfolio.tudominio.com/Federico_Caltabiano.pdf
```

Abrir el dominio en el navegador y comprobar:

- La home carga con estilos y tipografías.
- El CV se descarga.
- Una ruta como `https://portfolio.tudominio.com/not-found` muestra la página 404 de la app (no la de Nginx).

---

## 8. Actualizar el sitio

Cada vez que haya cambios:

```bash
cd /srv/portfolio
git pull --ff-only
dotnet publish src/FedePortfolio.Web/FedePortfolio.Web.csproj -c Release -o /tmp/portfolio-publish
rsync -a --delete /tmp/portfolio-publish/wwwroot/ /var/www/portfolio/
```

### Script de deploy

Crear un comando reutilizable:

```bash
sudo tee /usr/local/bin/deploy-portfolio >/dev/null <<'EOF'
#!/usr/bin/env bash
set -euo pipefail

export PATH="$HOME/.dotnet:$PATH"
cd /srv/portfolio

git pull --ff-only
dotnet publish src/FedePortfolio.Web/FedePortfolio.Web.csproj -c Release -o /tmp/portfolio-publish
rsync -a --delete /tmp/portfolio-publish/wwwroot/ /var/www/portfolio/

find /var/www/portfolio -type d -exec chmod 755 {} \;
find /var/www/portfolio -type f -exec chmod 644 {} \;
EOF
sudo chmod +x /usr/local/bin/deploy-portfolio
```

Después, para actualizar:

```bash
deploy-portfolio
```

> El script asume que `/var/www/portfolio` pertenece al usuario que ejecuta el deploy, tal como se configuró en el paso 5.

---

## 9. Deploy automático desde GitHub (opcional)

Si querés que cada push a `main` actualice la VM, podés usar una GitHub Action con SSH.

1. Crear una clave SSH exclusiva para GitHub Actions.
2. Agregar la clave pública a `~/.ssh/authorized_keys` del usuario de la VM.
3. Guardar en GitHub, en **Settings → Secrets and variables → Actions**:
   - `AZURE_VM_HOST`: IP o dominio de la VM.
   - `AZURE_VM_USER`: usuario SSH.
   - `AZURE_VM_SSH_KEY`: clave privada completa.
4. Agregar un workflow como `.github/workflows/deploy-azure-vm.yml`:

```yaml
name: Deploy to Azure VM

on:
  push:
    branches: [main]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Ejecutar deploy en la VM
        uses: appleboy/ssh-action@v1
        with:
          host: ${{ secrets.AZURE_VM_HOST }}
          username: ${{ secrets.AZURE_VM_USER }}
          key: ${{ secrets.AZURE_VM_SSH_KEY }}
          script: /usr/local/bin/deploy-portfolio
```

> Si restringiste el puerto 22 del NSG a tu IP, GitHub Actions no va a poder conectarse. Para este workflow, permití los rangos de IP de GitHub Actions o usá un runner self-hosted.

Este workflow es independiente del deploy a GitHub Pages que ya existe en el repositorio.

---

## 10. Problemas comunes

### `dotnet: command not found`

Cargar el PATH o volver a iniciar sesión:

```bash
export PATH="$HOME/.dotnet:$PATH"
```

### La página queda en blanco o falla WebAssembly

Verificar el MIME del archivo `.wasm`:

```bash
WASM_FILE=$(basename "$(find /var/www/portfolio/_framework -maxdepth 1 -name '*.wasm' | head -n 1)")
curl -I "https://portfolio.tudominio.com/_framework/$WASM_FILE"
```

Debe devolver `Content-Type: application/wasm`. Si no, revisar el bloque `location ~* \.wasm$` de Nginx y recargar:

```bash
sudo nginx -t && sudo systemctl reload nginx
```

### 404 al refrescar una ruta

Falta el fallback de SPA. La configuración debe incluir:

```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

### `403 Forbidden`

Revisar permisos:

```bash
sudo chown -R "$USER":"$USER" /var/www/portfolio
find /var/www/portfolio -type d -exec chmod 755 {} \;
find /var/www/portfolio -type f -exec chmod 644 {} \;
```

### Cambios que no aparecen

El HTML se sirve con `no-cache`, pero el navegador puede conservar assets viejos. Hacer una recarga forzada. Si el problema persiste, verificar que `rsync` haya copiado la nueva build:

```bash
ls -lh /var/www/portfolio/_framework/ | head
```

---

## 11. Usar un subdirectorio en lugar de un dominio

La configuración anterior asume que el portfolio se sirve en la raíz de un dominio o subdominio, por ejemplo:

```text
https://portfolio.tudominio.com/
```

Ese es el escenario recomendado porque el `index.html` ya tiene:

```html
<base href="/" />
```

Si en cambio querés servirlo como `https://tudominio.com/portfolio/`, hay que:

1. Cambiar el `base href` en `src/FedePortfolio.Web/wwwroot/index.html` a:

```html
<base href="/portfolio/" />
```

2. Publicar y copiar el resultado a una carpeta `/var/www/portfolio/`.
3. Configurar Nginx con un `location /portfolio/` y el fallback correspondiente.

Para evitar errores de rutas y assets, usar un subdominio propio suele ser más simple.

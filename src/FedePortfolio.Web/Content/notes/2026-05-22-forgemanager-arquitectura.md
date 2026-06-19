---
title: "Arquitectura de ForgeManager: API distribuida con microservicios"
date: 2026-05-22
tags: [dotnet, arquitectura, microservices]
---

**ForgeManager** es un proyecto personal en el que vengo trabajando de forma intermitente desde el año pasado. La idea es construir una API distribuida para gestión de proyectos con un backend dividido en microservicios.

## Topología

```
                ┌────────────┐
                │  Web SPA   │
                └─────┬──────┘
                      │ HTTPS
                ┌─────▼──────┐
                │ API Gateway│  (YARP)
                └─┬───────┬──┘
                  │       │
        ┌─────────▼─┐   ┌─▼──────────┐
        │  Projects │   │  Identity  │
        │   API     │   │    API     │
        └─────┬─────┘   └──────┬─────┘
              │               │
        ┌─────▼─────┐   ┌─────▼──────┐
        │ Postgres  │   │  Postgres  │
        └───────────┘   └────────────┘
```

Cada servicio tiene su propia base de datos. No hay joins cruzados ni esquemas compartidos. La consistencia entre límites de servicio se resuelve con **outbox pattern** + un worker que publica eventos a un broker.

## Stack

- **.NET 10** + ASP.NET Core minimal APIs
- **YARP** como API Gateway (load balancing, rate limiting, auth passthrough)
- **Postgres** como store por servicio
- **SignalR** para notificaciones en tiempo real al cliente
- **Docker Compose** para desarrollo local
- **OpenTelemetry** para trazas distribuidas (Jaeger en local)

## Comunicación en tiempo real

Para los eventos que necesitan empujarse al cliente, expongo un hub SignalR detrás del gateway. Cada cliente se suscribe a un grupo por proyecto y los eventos llegan a través de un *backplane* con Redis.

```csharp
public class ProjectHub : Hub
{
    public async Task JoinProject(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, projectId);
    }

    public async Task NotifyChange(string projectId, ChangeDto change)
    {
        await Clients.OthersInGroup(projectId)
            .SendAsync("ProjectChanged", change);
    }
}
```

## Decisiones que no me arrepiento

- **No usar un ESB**: prefiero HTTP síncrono para queries y eventos asíncronos solo cuando hace falta.
- **Outbox sí o sí**: implementar el patrón outbox apenas se tienen dos servicios que comparten estado. Sin él, los eventos se pierden en el peor momento.
- **Trazas desde el día 1**: OpenTelemetry con propagación W3C me ahorró horas cuando algo se rompía entre el gateway y un servicio.

## Lo que sigue

Estoy trabajando en:

- Un servicio de notificaciones por email desacoplado (worker que consume la cola de eventos).
- Migrar la autenticación a OIDC con Keycloak.
- Documentar todo en un OpenAPI generado automáticamente con `Swashbuckle` + una UI de Redoc servida desde el gateway.

---
title: "SGE: cómo modelamos perfiles y roles en un sistema de gestión escolar"
date: 2026-04-30
tags: [dotnet, sql, arquitectura, académico]
---

**SGE** (Sistema de Gestión Escolar) es un proyecto académico de la Facultad de Informática (UNLP). La consigna era construir un sistema para administración de una escuela secundaria: alumnos, materias, calificaciones, asistencia y reportes para directivos.

El desafío interesante no fue técnico en el sentido tradicional, sino **modelar bien los permisos**. Una escuela tiene una jerarquía sutil: preceptor, directivo, profesor, administrativo, alumno. Cada uno tiene visibilidad sobre distintos subconjuntos de datos y puede ejecutar acciones diferentes.

## Modelo de identidad

La decisión clave fue separar **autenticación** de **autorización** desde el día uno.

- **Identidad**: el usuario tiene un `Identity` con credenciales y un `Person` asociado.
- **Rol**: un usuario puede tener varios `Role` simultáneos (`Alumno`, `Profesor`, etc.).
- **Permisos**: los roles se traducen a una colección de `Permission` que la API chequea en cada endpoint.

```csharp
public class User
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string Email { get; set; } = "";
    public List<Role> Roles { get; set; } = new();
    public List<Permission> EffectivePermissions() =>
        Roles.SelectMany(r => r.Permissions).Distinct().ToList();
}
```

## Reglas de alcance

Lo más jugoso fue modelar **reglas de alcance** (scope rules): no alcanza con tener un permiso, hay que definir sobre qué entidades se aplica.

Un profesor puede *calificar*, pero solo a los alumnos de las materias que dicta este cuatrimestre. Un preceptor puede *ver asistencia*, pero solo del curso del que es tutor.

Para eso usamos un **evaluador de políticas** que recibe el `User`, la `Action` y la `Entity` objetivo:

```csharp
public interface IPolicy
{
    bool Evaluate(User user, Action action, Entity target);
}

public class CanGradePolicy : IPolicy
{
    public bool Evaluate(User u, Action a, Entity e) =>
        a == Actions.Grade &&
        e is CourseGroup group &&
        u.HasRole("Profesor") &&
        u.IsAssignedTo(group.Subject, group.Term);
}
```

## Capa de aplicación

El controller queda finito: levanta el `User` desde el contexto HTTP, delega a un `Authorizer` que ejecuta las policies relevantes y solo después invoca el caso de uso.

```csharp
[HttpPost("grades")]
public async Task<IActionResult> PostGrade(GradeDto dto)
{
    var user = await GetCurrentUser();
    var courseGroup = await _repo.GetCourseGroup(dto.CourseGroupId);

    if (!_auth.Evaluate(user, Actions.Grade, courseGroup))
        return Forbid();

    await _grades.Add(Grade.FromDto(dto));
    return NoContent();
}
```

## Lo que aprendí

- **Políticas como datos > if anidados**: tener un set de policies testeables unitariamente hace que agregar un nuevo rol sea sumar entries, no reescribir lógica.
- **El modelo de alcance cambia la UX**: muchas pantallas se simplificaron cuando dejamos de esconder campos por rol y empezamos a *filtrar la lista* según el alcance del usuario.
- **Auditoría desde el inicio**: registramos quién hizo qué y cuándo. En un sistema con datos de menores, esto no es opcional.

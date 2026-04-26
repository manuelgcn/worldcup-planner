# SolutionCopa

Proyecto en C#/.NET para la gestión de equipos de fútbol.

## Estructura
- `Domain`: entidades y reglas de negocio
- `DataAccess`: acceso a datos
- `Services`: lógica de aplicación
- `Controllers`: controladores
- `UI`: interfaz de usuario
- `TestDomain`: pruebas unitarias

## Reglas del dominio
La entidad `Team` valida:
- nombre obligatorio
- nombre entre 1 y 60 caracteres
- ranking FIFA obligatorio
- confederación obligatoria
- atributo `EsFicticio` como parte del modelo

## Pruebas
El proyecto usa MSTest para los tests nuevos.

### Ejecutar tests
```powershell
dotnet test
```


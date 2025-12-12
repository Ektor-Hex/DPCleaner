# DPCleaner

Programa para eliminación recursiva y selectiva de archivos y utilidades de prueba.

## Descripción

DPCleaner es una utilidad escrita en C# diseñada para eliminar archivos de forma recursiva según patrones y reglas configurables. Incluye un script para generar archivos de prueba y una solución Visual Studio (`DPCleaner.sln`) para compilar y extender la herramienta.

> Antes de ejecutar operaciones destructivas, revisa cuidadosamente los patrones y prueba en entornos controlados.

## Características

- Borrado recursivo de archivos en directorios.
- Filtros por nombre/extensión (wildcards).
- Script para generar archivos de prueba: `GenerateTestFiles.ps1`.
- Orientado a ejecución en entornos Windows (.NET).

## Requisitos

- Windows 10/11
- .NET SDK (consulta la versión target en `DPCleaner.sln` / `.csproj`)
- PowerShell (para `GenerateTestFiles.ps1`)

## Instalación

Clona el repositorio y compílalo con .NET SDK:

```powershell
git clone https://github.com/Ektor-Hex/DPCleaner.git
cd DPCleaner
dotnet build DPCleaner.sln
```

O abre `DPCleaner.sln` en Visual Studio y compila desde el IDE.

## Uso

1. Revisa los parámetros de la aplicación o la configuración (según implementación).
2. Opcionalmente genera archivos de prueba con `GenerateTestFiles.ps1`:

```powershell
# Generar archivos de prueba (ajusta parámetros según el script)
.\GenerateTestFiles.ps1 -TargetPath .\testdata -Count 100 -Depth 3
```

3. Ejecuta la aplicación apuntando al directorio objetivo y aplica filtros (si la app soporta argumentos):

```powershell
# Ejemplo ilustrativo
DPCleaner.exe --path "C:\ruta\a\limpiar" --pattern "*.tmp" --dry-run
```

Usa `--dry-run` si está soportado para verificar antes de eliminar.

## Desarrollo

Estructura típica del repositorio:

```
DPCleaner/
├── DPCleaner/              # Código fuente (proyecto C#)
├── DPCleaner.sln           # Solución
├── GenerateTestFiles.ps1   # Script para crear archivos de prueba
└── README.md
```

Buenas prácticas:
- Añadir tests unitarios para lógica de selección y manejo de I/O.
- Implementar `dry-run` y confirmaciones antes de borrar.
- Manejar excepciones (archivo en uso, permisos insuficientes).

## Seguridad y precauciones

- El borrado es irreversible. Probar siempre con `dry-run` o en un directorio de pruebas.
- Evitar ejecutar innecesariamente como administrador.
- Validar rutas y patrones para no afectar archivos del sistema.

## Contribuciones

1. Haz fork del proyecto.
2. Crea una rama para tu feature: `git checkout -b feature/mi-cambio`.
3. Haz commits claros y descriptivos.
4. Abre un Pull Request contra `main`.

## Licencia

Si no hay una licencia en el repo, añade una explícita. Recomiendo MIT si quieres permitir uso amplio:

```
MIT License
Copyright (c) 2025 Ektor-Hex
...
```

---

*

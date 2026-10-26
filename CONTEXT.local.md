# Contexto: maui-linux (MauiGtk)

> Archivo local de contexto. No versionar (excluido en Git/SVN). Mantenido por IA.
> Última actualización: 2026-10-23

## Resumen

Backend comunitario **GTK3** para .NET MAUI, mantenido por la organización MauiGtk como fork de `dotnet/maui`. Permite ejecutar aplicaciones MAUI en Linux usando GtkSharp en lugar de Android/iOS/Windows. El repo local es el fork de `Armando1424` con `origin` = fork y `upstream` = `MauiGtk/maui-linux`; el objetivo es probar la implementación y, más adelante, enviar PRs al upstream.

## Stack

- **Lenguaje(s):** C# (mayoría), XAML, MSBuild, algún F# (`nugetPreRelease.fsx`).
- **Framework(s):** .NET MAUI 8.0 + GTK3 vía GtkSharp; capa de compatibilidad legacy (renderers) y SkiaSharp para gráficos.
- **Runtime / versión:** `net8.0-gtk` (`_MauiDotNetTfm` en `Directory.Build.props`).
- **Gestor de paquetes:** NuGet + workloads de .NET; Cake para build.
- **Dependencias clave:** `GtkSharp 3.24.24.117-develop`, manifest `gtksharp.net.sdk.gtk.manifest-8.0.200`, SkiaSharp, xUnit, NSubstitute.
- **Datos / servicios externos:** Ninguno en tiempo de ejecución; CI en GitHub Actions (`ubuntu-24.04`).

## Comandos

| Acción | Comando |
| --- | --- |
| Instalar workload GTK | Descargar el manifest nupkg y `dotnet workload install gtk --skip-manifest-update` (ver `build-gtk.yml`) |
| Build tareas MSBuild | `dotnet build Microsoft.Maui.BuildTasks.slnf` |
| Build GTK | `dotnet build -c Release Microsoft.Maui.Gtk.slnf` |
| Empaquetar NuGet | `dotnet pack Microsoft.Maui.Gtk.Packages.slnf` |
| Ejecutar sample | `dotnet run --project src/Controls/samples/Controls.Sample.Gtk/Controls.Sample.Gtk.csproj` |
| Tests Core | `dotnet test src/Core/tests/UnitTests/Core.UnitTests.csproj` |
| Tests Controls | `dotnet test src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj` |
| Build completo | `./build.sh` (Linux) o `build.cmd` (Windows, limitado) |
| Build en WSL | `wsl -d Ubuntu-24.04 -u manue -- bash -lc 'cd ~/repos/maui-linux && dotnet build -c Release Microsoft.Maui.Gtk.slnf'` |
| Ejecutar sample (WSL) | `cd ~/repos/maui-linux/src/Controls/samples/Controls.Sample.Gtk/bin/Release/net8.0-gtk/linux-x64 && dotnet Controls.Sample.Gtk.dll` |

**Entorno:** se requiere Linux o WSL2 con Ubuntu 24.04, .NET SDK 8.0.2xx y GTK3 (`libgtk-3-dev`/runtime). En Windows nativo no hay soporte directo para el TFM `-gtk`.

## Entorno de este PC (instalado y validado 2026-10-26)

- **Host:** Windows 11 Pro 26300, i7-1165G7 (4C/8T), 16 GB RAM. `C:` es SSD (238 GB, ~27 GB libres) y `D:` es HDD (932 GB, ~500 GB libres).
- **Windows nativo: NO sirve.** `dotnet build` del sample falla con `NETSDK1139: The target platform identifier gtk was not recognized`; no existe workload `gtk` en los feeds de Windows y no hay GTK3 nativo instalado.
- **WSL2: instalado y operativo.** Distro `Ubuntu-24.04` (24.04.5) importada en `D:\WSL\Ubuntu-24.04`; WSL 2.3.24, hipervisor activo, WSLg 1.0.65 (`DISPLAY=:0`). Usuario `manue` con sudo sin contraseña.
- **Toolchain Linux:** .NET SDK **8.0.200** + workload **gtk 3.24.24-rev.117-develop**, GTK3 3.24.41, `libgtksourceview-4-0`, `librsvg2-common`, `x11-utils`.
- **Repo Linux:** clon en `~/repos/maui-linux` (rama `develop`), `_IncludeAndroid` vacío. Build validado de `Microsoft.Maui.BuildTasks.slnf` y `Microsoft.Maui.Gtk.slnf -c Release`; el sample abre una ventana GTK de 800x800.
- **Limitación:** `libwebkit2gtk-4.0` ya no existe en Ubuntu 24.04, así que el sample **Blazor GTK** no puede ejecutarse (sí compila). `Controls.Sample.Gtk` no usa WebKit.
- **Rendimiento:** el VHDX vive en el HDD `D:`; apt y los builds son lentos. Si se libera espacio en `C:` (SSD), conviene mover la distro.
- **SDK en Windows:** 8.0.427 y 10.0.401; `dotnet` toma el 10.0.401 porque `global.json` solo fija `tools.dotnet`, no `sdk.version`.

## Estructura

```
.github/workflows/build-gtk.yml      CI de build/test/pack GTK
Microsoft.Maui.Gtk.slnf              solución filtrada solo para GTK
Microsoft.Maui.Gtk.Packages.slnf     proyectos empaquetables
Directory.Build.Override.props       switches _IncludeGtk/_IncludeAndroid/etc.
Status.md                            matriz de estado por control (desactualizada)
src/Core/src/Handlers/<Control>/     handlers base; implementaciones *.Gtk.cs
src/Controls/src/Core/Handlers/      handlers de Controls (Items, Shell, Shapes)
src/Controls/src/Core/Compatibility/Handlers/Gtk/  renderers legacy
src/Graphics/src/Graphics.Gtk/       backend gráfico GTK
src/Controls/samples/Controls.Sample.Gtk/  app de prueba (Program.cs)
src/Controls/tests/Core.UnitTests/   tests unitarios xUnit
src/Core/tests/UnitTests/            tests unitarios xUnit
```

- **Puntos de entrada:** `src/Controls/samples/Controls.Sample.Gtk/Program.cs` crea `SimpleSampleGtkApplication`; los handlers GTK se resuelven por el patrón handler de MAUI.
- `src/BlazorWebView/src/Gtk/` — integración BlazorWebView/WebKitGTK.

## Arquitectura

MAUI separa contratos (`src/Core`), controles (`src/Controls`) y plataformas. Cada control tiene un handler parcial por plataforma (`ButtonHandler.Gtk.cs`, `LabelHandler.Gtk.cs`, etc.) que crea el widget GTK, conecta propiedades y mapea eventos. `src/Controls/src/Core/Compatibility` conserva los renderers antiguos para APIs legacy. Graphics usa SkiaSharp con un backend GTK (`Graphics.Skia.GtkSharp`). La compilación multi-plataforma se controla con `Directory.Build.Override.props` y los `.slnf`.

## Convenciones

- **Nombres y organización:** archivos específicos de plataforma con sufijo `.Gtk.cs`; clases `partial` que completan el handler común.
- **APIs públicas:** cualquier cambio de superficie pública exige actualizar `src/*/PublicAPI/net-gtk/PublicAPI.Unshipped.txt`; el CI lo valida.
- **Formato:** `dotnet format` es estricto (workflow `dotnet-autoformat-pr.yml` y job `dotnet-format`).
- **Tests:** xUnit; unit tests multiplataforma y device/UI tests separados. Para aportes pequeños, usar los unit tests de `src/Core/tests/UnitTests` y `src/Controls/tests/Core.UnitTests`.
- **Commits/PRs:** no hay guía específica de MauiGtk; se hereda `.github/CONTRIBUTING.md` de dotnet/maui.

## Estado actual

- **Funciona:** build completo GTK y ejecución de `Controls.Sample.Gtk` validados en WSL el 2026-10-26 (tras revertir el commit roto `79b51cdf2` en `ddc754505`). Base sincronizada con el upstream en `118de7c` (2025-10-10).
- **En progreso:** upstream con 44 issues abiertos y ~10 PRs abiertos; varios PRs de nov–dic 2025 sin revisar/mergear.
- **Rama / entorno relevante:** `develop` (rama de trabajo, creada 2026-10-26 desde `master`); `master` es la default del fork y solo recibe merges. `origin/main` fue eliminada al renombrar; el upstream `MauiGtk/maui-linux` sigue usando `main`. El único commit de 2026 en el upstream es la rama automática `housekeeping/fix-codeformatting` (2026-03-24).
- **Actividad:** el sitio mauigtk.net sigue publicando (blog de 2026-03-25), pero el código del upstream (`main`) lleva ~1 año sin merges.

## Decisiones

| Fecha | Decisión | Motivo |
| --- | --- | --- |
| 2026-10-23 | Clonar el fork `Armando1424/maui-linux` y añadir `upstream` | Permite pushear ramas propias y abrir PRs; el upstream sigue siendo la fuente de verdad y de issues |
| 2026-10-23 | Renombrar la rama por defecto del fork `main` → `master` y borrar `origin/main` | Preferencia del usuario; el fork queda independiente del nombre de rama del upstream |
| 2026-10-26 | Instalar Ubuntu 24.04 en WSL2 importándola directamente a `D:\WSL\Ubuntu-24.04` | D: es donde hay espacio; C: (SSD) solo tiene ~27 GB libres |
| 2026-10-26 | Revertir `79b51cdf2` en `develop` (`ddc754505`) | Ese commit automático cambió `if (x != null) x.Prop = y;` por `x?.Prop = y;`, inválido en C#, y rompía todo el build |

## Restricciones

- No cambiar APIs públicas sin actualizar los archivos `PublicAPI`.
- `CONTEXT.local.md` está versionado por decisión explícita del usuario (2026-10-23); no meter secretos ni datos sensibles.
- No actualizar versiones de SDK/workloads sin replicar el CI (`8.0.200` + GtkSharp `3.24.24.117-develop`).
- No mezclar este backend **GTK3/GtkSharp** con el backend oficial **GTK4/gir.core** de Microsoft (`Microsoft.Maui.Platforms.Linux.Gtk4`): son stacks distintos.

## Pendientes

**Retomar aquí: adaptar los workflows al renombrado `main` → `master` (acordado, sin empezar).**

- [ ] `.github/workflows/dotnet-format-daily-main.yml` — cambiar `ref: main` (línea 18) y `base: main` (línea 49) a `master`. Es el único que falla de forma directa (checkout de una rama inexistente).
- [ ] `.github/workflows/build-gtk.yml:47` — cambiar `github.ref == 'refs/heads/main'` a `'refs/heads/master'`. Si no, deja de publicar a NuGet en silencio (los tags sí siguen funcionando).
- [ ] `.github/workflows/spellcheck.yml` — cambiar `branches: main` en `push` (línea 6) y `pull_request` (línea 12) a `master`.
- [ ] `.github/workflows/sdk-insertion-bump.yml` — trigger push a `main` (línea 7); decidir entre cambiar a `master` o borrar el workflow (probablemente no aplica en el fork y despacha a `xamarin/sdk-insertions`).
- [ ] Revisar en GitHub **Settings → Rules/Branches** que la protección de `main` se replique en `master` (las reglas no siguen al renombrar).
- [ ] Revisar si quedaron **PRs abiertos con base `main`** (quedaron huérfanos al borrar la rama; `gh` no está instalado, revisar en la web).
- [ ] En otros clones locales: `git fetch --prune` y `git checkout -b master origin/master`.

**Trabajo de fondo (sin fecha):**

- [x] Instalar la distro **Ubuntu 24.04 en WSL2**, .NET SDK 8.0.2xx, workload GTK y libs GTK3 (hecho 2026-10-26 en `D:\WSL\Ubuntu-24.04`).
- [x] Compilar `Microsoft.Maui.Gtk.slnf -c Release` y ejecutar `Controls.Sample.Gtk` (hecho 2026-10-26).
- [ ] Mover el VHDX de WSL a `C:` (SSD) cuando haya espacio, para acelerar apt/builds.
- [ ] Reportar al upstream el build roto por `79b51cdf2`; el fix ya está aplicado en `develop`.
- [ ] Revisar los issues abiertos del upstream y reproducir los más accesibles.
- [ ] Elegir un primer aporte pequeño (test o fix de un mapper) y abrir PR desde una rama del fork.
- [ ] Evaluar el backend oficial GTK4 de Microsoft (discusión dotnet/maui #339) como alternativa a futuro.

## Trampas conocidas

- **Windows nativo no compila GTK:** confirmado en este PC con `NETSDK1139` (plataforma `gtk` no reconocida). El TFM `net8.0-gtk` necesita Linux/WSL. El PR upstream #111 ("Changes to build and run it on Windows 10 with WSL") sigue abierto.
- **Manifest de GtkSharp:** la detección automática del workload falla (GtkSharp #355); hay que descargar el `.nupkg` del manifest y descomprimirlo en `sdk-manifests/8.0.200/gtksharp.net.sdk.gtk`.
- **`_IncludeAndroid` viene en `true`** en `Directory.Build.Override.props`; el CI lo desactiva con `sed` antes de compilar GTK. En local conviene ponerlo vacío.
- **`global.json` fija `tools.dotnet` en 7.0.203**, pero el build real usa 8.0.2xx; no confundir esa versión con la del SDK de compilación.
- **`README.md` y `Status.md`** son heredados de dotnet/maui y están desactualizados; no tomarlos como estado real.
- **Issues deshabilitados en el fork:** los bugs se reportan en `MauiGtk/maui-linux`, no en `Armando1424/maui-linux`.
- **PRs lentos:** el upstream tiene PRs de hace meses sin revisar; planificar aportes pequeños y con tests para facilitar el merge.
- **Workflows con `main` hardcodeado:** el fork ya usa `master`, así que los workflows heredados con `branches: main`, `ref: main` o `refs/heads/main` no se disparan o fallan. Ver la lista en **Pendientes**; los enlaces a `github.com/dotnet/maui/.../main` de docs e issues NO se tocan (son del repo original).
- **`dotnet run` no encuentra `dotnet_bot.png`:** el sample abre la imagen con ruta relativa al cwd. Ejecutar el DLL desde `bin/Release/net8.0-gtk/linux-x64`, no desde el directorio del proyecto.
- **`git push` desde WSL se queda esperando credenciales:** no hay Git Credential Manager en Linux. Pushear desde Windows o configurar un credential helper/SSH en WSL.
- **Commit `79b51cdf2` rompe el build:** convierte `if (x != null) x.Prop = y;` en `x?.Prop = y;` (inválido en C#). Ya revertido en `ddc754505`; si se resincroniza con upstream, verificar que no vuelva.
- **WebKitGTK 4.0 no está en Ubuntu 24.04:** `libwebkit2gtk-4.0-37` no existe; el sample Blazor GTK compila pero no puede ejecutarse. `Controls.Sample.Gtk` no lo necesita.
- **VHDX en HDD:** `D:` es disco mecánico; apt y los builds tardan mucho más que en el SSD `C:`.

## Bitácora

- 2026-10-23 — Análisis de fork vs upstream, clonado del fork, `upstream` configurado y contexto creado.
- 2026-10-23 — Rama local y remota renombrada `main` → `master`; `master` puesta como default en GitHub y `origin/main` eliminada. Queda pendiente adaptar 4 workflows y revisar protección/PRs.
- 2026-10-26 — Validación del PC: Windows nativo descartado (`NETSDK1139`, sin workload `gtk`); WSL2 2.3.24 operativo con WSLg pero sin distro instalada. `develop` creada como rama de trabajo. Actualizado `CONTEXT.local.md` con el entorno validado.
- 2026-10-26 — Ubuntu 24.04.5 instalado en `D:\WSL\Ubuntu-24.04`, SDK 8.0.200 + workload GTK, repo clonado en `~/repos/maui-linux`. Build GTK y ejecución de `Controls.Sample.Gtk` validados; se revirtió `79b51cdf2` (`ddc754505`) por romper la compilación.

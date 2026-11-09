# Build de .NET MAUI GTK en WSL2 y Raspberry Pi (ARM64)

Este fork usa el backend **GTK3/GtkSharp** de MAUI. El TFM es `net8.0-gtk` y
**Windows nativo no lo soporta**. La forma validada de compilar y ejecutar en un
equipo Windows es usar **WSL2 con Ubuntu 24.04**. En Linux ARM64
(Raspberry Pi 4) también se compila nativamente; ver
[Raspberry Pi 4](#raspberry-pi-4-arm64).

> Documento específico de este fork. No forma parte del upstream
> `MauiGtk/maui-linux`.

## Entorno validado

Validado el **2026-10-26**:

- Windows 11 con WSL2 y WSLg.
- Ubuntu 24.04.
- .NET SDK **8.0.200**.
- Workload **gtk 3.24.24-rev.117-develop**.
- GTK3 3.24.41, `libgtksourceview-4-0`, `librsvg2-common`, `x11-utils`.
- `_IncludeAndroid` vacío en `Directory.Build.Override.props`.

## Requisitos

- Windows 11 con virtualización habilitada.
- WSL2 con una distro Ubuntu 24.04.
- .NET SDK 8.0.2xx dentro de WSL.
- Librerías GTK3 de desarrollo.
- Repo clonado dentro de WSL (por ejemplo `~/repos/maui-linux`).

## Instalación del workload GTK

La detección automática del manifest de GtkSharp falla
([GtkSharp #355](https://github.com/GtkSharp/GtkSharp/issues/355)), así que se
instala manualmente:

```bash
DOTNET_VERSION=8.0.200
GTKSHARP_VERSION=3.24.24.117-develop
MANIFEST=gtksharp.net.sdk.gtk.manifest-${DOTNET_VERSION}

dotnet nuget add source --name nuget.org https://api.nuget.org/v3/index.json

wget "https://www.nuget.org/api/v2/package/${MANIFEST}/${GTKSHARP_VERSION}" \
  -O "${MANIFEST}.nupkg"

DOTNET_DIR=/usr/share/dotnet
WORKLOAD_MANIFEST_DIR="${DOTNET_DIR}/sdk-manifests/${DOTNET_VERSION}/gtksharp.net.sdk.gtk"

mkdir -p "${WORKLOAD_MANIFEST_DIR}"
unzip -j "${MANIFEST}.nupkg" "data/*" -d "${WORKLOAD_MANIFEST_DIR}/"
rm "${MANIFEST}.nupkg"
chmod 764 "${WORKLOAD_MANIFEST_DIR}"/*

dotnet workload search
dotnet workload install gtk --skip-manifest-update
```

Dependencias del sistema:

```bash
sudo apt update
sudo apt install -y libgtk-3-dev libgtksourceview-4-dev librsvg2-common x11-utils
```

## Preparar el repo

```bash
git clone https://github.com/Armando1424/maui-linux.git
cd maui-linux
git checkout develop

# El CI hace esto mismo antes de compilar GTK:
sed -i 's/_IncludeAndroid>true/_IncludeAndroid>/g' Directory.Build.Override.props
```

## Compilar

```bash
dotnet build Microsoft.Maui.BuildTasks.slnf
dotnet build -c Release Microsoft.Maui.Gtk.slnf
```

Para empaquetar:

```bash
dotnet fsi nugetPreRelease.fsx "" > PreReleaseVersionSuffix.txt
dotnet pack Microsoft.Maui.Gtk.Packages.slnf
```

## Ejecutar el sample

El sample abre `dotnet_bot.png` con una ruta relativa al directorio actual. Hay
que ejecutarlo desde la carpeta de salida, no desde la raíz del proyecto:

```bash
cd src/Controls/samples/Controls.Sample.Gtk/bin/Release/net8.0-gtk/linux-x64
dotnet Controls.Sample.Gtk.dll
```

Con WSLg se abre una ventana GTK de 800x800.

## Raspberry Pi 4 (ARM64)

Build validado el **2026-11-09** en una Raspberry Pi 4 (8 GB, Debian 12 ARM64).

Requisitos extra respecto a WSL:

- .NET SDK **8.0.200** instalado side-by-side. El repo no fija `sdk.version`;
  para forzarlo, agrega temporalmente a `global.json`:

  ```json
  "sdk": { "version": "8.0.200", "rollForward": "disable" }
  ```

- `libgtksourceview-4-dev` además de `libgtk-3-dev`, `librsvg2-common` y
  `x11-utils`.
- `nuget.org` como fuente NuGet. El `NuGet.config` del repo solo trae feeds de
  Azure y el restore puede fallar con `Connection reset by peer`:

  ```bash
  dotnet nuget add source https://api.nuget.org/v3/index.json \
    --name nuget.org --configfile NuGet.config
  ```

### Ajuste necesario para ARM64

El workload `GtkSharp.Sdk` declara:

```text
RuntimePackRuntimeIdentifiers="win-x64;win-x86;linux-x64;linux-x86;osx-x64"
```

No incluye `linux-arm64`. El runtime pack contiene **solo DLLs managed**, así
que se puede habilitar con un parche local:

```bash
GTK_SDK=/usr/share/dotnet/packs/GtkSharp.Sdk/3.24.24.117-develop/Sdk/Sdk.targets
sudo cp -a "$GTK_SDK" "$GTK_SDK.bak-arm64"
sudo sed -i \
  's/linux-x86;osx-x64/linux-x86;linux-arm64;osx-x64/' \
  "$GTK_SDK"
```

Después:

```bash
dotnet restore Microsoft.Maui.Gtk.slnf --disable-parallel
dotnet build -c Release Microsoft.Maui.Gtk.slnf --no-restore
```

El sample queda en:

```text
src/Controls/samples/Controls.Sample.Gtk/bin/Release/net8.0-gtk/linux-arm64/
```

Se validó el arranque con `DISPLAY=:0`/Wayland durante 15 s sin errores.

Tiempos aproximados en la Pi 4:

- `Microsoft.Maui.BuildTasks.slnf`: ~10 min.
- `Microsoft.Maui.Gtk.slnf` (incremental): ~2 min.

> El parche es local y una actualización del workload puede sobrescribirlo. Es
> candidato a reportarse en GtkSharp para que declaren `linux-arm64`.

## Limitaciones conocidas

- **ARM64:** el workload GtkSharp `3.24.24.117` no declara `linux-arm64` en
  `RuntimePackRuntimeIdentifiers`; requiere el parche local de la sección
  [Raspberry Pi 4](#raspberry-pi-4-arm64).
- **Windows nativo no sirve.** `dotnet build` falla con
  `NETSDK1139: The target platform identifier gtk was not recognized`. No hay
  workload `gtk` para Windows ni GTK3 nativo.
- **Blazor GTK:** `libwebkit2gtk-4.0` ya no existe en Ubuntu 24.04. El sample
  Blazor compila, pero no se puede ejecutar. `Controls.Sample.Gtk` no usa
  WebKit.
- **Rendimiento:** si el VHDX de WSL está en un HDD, `apt` y los builds son
  lentos. Conviene moverlo a un SSD.
- **`global.json`:** fija `tools.dotnet` en `7.0.203`, pero el build real usa
  SDK 8.0.2xx. No confundir esa versión con la del SDK de compilación.
- **Revert necesario:** el commit automático `79b51cdf2` cambió asignaciones
  válidas por `x?.Prop = y;`, inválido en C#, y rompía el build. El revert está
  en `ddc7545`, incluido en `develop`.

## Validación

El 2026-10-26 se validó:

- Build de `Microsoft.Maui.BuildTasks.slnf`.
- Build de `Microsoft.Maui.Gtk.slnf -c Release`.
- Ejecución de `Controls.Sample.Gtk` con ventana GTK funcional.
- Build ARM64 en Raspberry Pi 4 y arranque del sample (2026-11-09).

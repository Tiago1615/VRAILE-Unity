# 🧠 VRAILE Unity (Virtual Reality Artificial Intelligence Learning Environment)

Este repositorio contiene el proyecto *Unity* de la aplicación **VRAILE**, una experiencia inmersiva de Realidad Virtual con interacción basada en IA generativa. Esta aplicación permite simular escenarios clínicos donde el usuario intearctua con enfermos ficticios mediante su propia voz. Todo esto, integrando tecnologías de *Unity*, *Oculus* y servicios externos como *OpenAI* y *AWS*.

## 📑 Índice

- [📦 Requisitos](#-requisitos)
- [📒 Credenciales](#-credenciales)
- [🌍 Red](#-red)
- [⚙️ Instalación del proyecto Unity](#-instalacion-del-proyecto-unity)
  - [📁 Clonar el repositorio](#-clonar-el-repositorio)
  - [🛠️ Abrir en Unity](#️-abrir-en-unity)
  - [🔄️ Compilar y pasar la aplicación a las gafas](#-compilar-y-pasar-la-aplicacion-a-las-gafas)
  - [🧩 SDKs empleadas](#-sdks-empleadas)
- [🕹️ Ejecución](#-ejecución)
- [🚫 Exclusiones](#-exclusiones)

## 📦 Requisitos

- Unity **2021.3.45f1** (versión *LTS*)
- Dispositivo de realidad virtual compatible (Oculus Quest 2 o superior)
- Cuenta de *OpenAI* con API Key
- Cuenta de *AWS* con acceso a Polly
- Conexión a internet compartida entre el PC y el dispositivo VR

## 📒 Credenciales

Para ejecutar correctamente la aplicación, es necesario disponer de:

- Una **API Key de OpenAI**: [https://platform.openai.com/account/api-keys](https://platform.openai.com/account/api-keys)
- Un usuario y credenciales activas de **AWS Polly**: [https://aws.amazon.com/polly](https://aws.amazon.com/polly)

## 🌍 Red

La aplicación se comunica con un servidor Python que procesa la lógica de IA. Por lo tanto:

- El servidor debe estar corriendo en la misma red local.
- Las gafas de VR deben estar conectadas a esa misma red.

## ⚙️ Instalación del proyecto Unity

### 📁 Clonar el repositorio

```bash
git clone https://github.com/Tiago1615/VRAILE-Unity.git
```

> No se han incluido los archivos multimedia en este repositorio por temas de tamaños permitidos.

### 🛠️ Abrir en Unity

1. Abrir *Unity Hub*.
2. Seleccionar “Add Project” y navegar hasta la carpeta clonada.
3. Tener instalada la misma versión de *Unity* con la que fue desarrollado (**2021.3.45f1**).
4. Abrir el proyecto.

### 🔄️ Compilar y pasar la aplicación a las gafas

Para compilar la aplicación, desde el proyecto abierto, se debe ir a la sección *File* del inspector de *Unity* y seguir estos pasos:

```bash
File > Build Settings > Android > Build
```

Esto devolverá un *APK* con la aplicación ya compilada y lista para ser pasada a las gafas. Ahora, realmente para pasar el APK directamente a las gafas, se podría haber escogido la siguiente opción:

```bash
File > Build Settings > Android > Build And Run
```

Esto automáticamente copia el *APK* al dispositivo de *VR*, siempre y cuando las gafas hayan sido conectadas previamente. El problema de hacerlo así, es que al necesitar comunicarse con un servidor externo, es necesario indicar la dirección IP con la cual se van a estar comunicando las gafas. Con este propósito, ha sido precargado un archivo `config.json` en el dispositivo, será necesario actualizar este archivo cada vez que se cambie de máquina. Esto se debe a que la dirección IP de la máquina que aloja el servidor habrá cambiado y es necesario actualizar el `config.json` para una comunicación sin imprevistos.

Para esto, se ha usado la aplicación *SideQuest*. *SideQuest* es una aplicación que permite realizar transferencias de archivos desde un ordenador a las gafas. A continuación, se adjunta el enlace a la página oficial y un tutorial para instalar *APK* mediante *SideQuest*:

[SideQuest](https://sidequestvr.com/)

[Tutorial SideQuest](https://www.youtube.com/watch?v=EUUWURT9Uxc&t=119s)

Habiendo hecho esto, lo que quedaría sería crear un nuevo archivo `config.json`, que tendrá un formato tal que:

```bash
{
    "SERVER_IP": "DIRECCION_IP_DEL_SERVIDOR"
}
```

Tras tener el nuevo `config.json`, se debe subir al dispositivo mediante comandos *adb*:

```bash
adb push RUTA_AL_ARCHIVO_config.json /sdcard/Android/data/com.ULPGC.SimulacionMedica/files/config.json
```

Este tutorial explica un poco por encima cómo ejecutar esta clase de comandos desde el propio *SideQuest*

[Tutorial comandos adb](https://www.youtube.com/watch?v=3m4sSQ1XOcE)

### 🧩 SDKs empleadas

Se ha empleado la *SDK* de *Ready Player Me* para los avatares que hacen el papel de enfermo. Esta *SDK* está instalada en el proyecto, no es necesario instalarla de nuevo y aquí se puede encontrar su página oficial:

[Ready Player Me](https://readyplayer.me/es)

[Documentación Ready Player Me](https://docs.readyplayer.me/ready-player-me)

[Tutorial Ready Player Me](https://www.youtube.com/watch?v=qkZQaOS9csw)

También se ha usado la SDK *Oculus Lipsync Unity*, para las animaciones que utilizan los avatares a la hora de hablar. Esta *SDK* también está instalada en el proyecto, sin embargo, los archivos `libOVRLipSync.dylib` y `OVRLipSync.bundle` han sido omitidos en este repositorio, principalmente por su gran tamaño. Por este motivo, puede ser que se requiera volver a instalar la *SDK* para un correcto funcionamiento.

[Oculus Lipsync Unity](https://developers.meta.com/horizon/downloads/package/oculus-lipsync-unity/)

[Tutorial Oculus Lipsync Unity](https://www.youtube.com/watch?v=Q4sPGTVylnY)

## 🕹️ Ejecución

1. Poner en marcha el servidor de Python.
2. Entrar a la aplicación *VRAILE* desde el dispositivo de *VR*.
3. Comenzar la simulación.

A continuación se muestran algunas capturas de la aplicación:

![Image](https://github.com/user-attachments/assets/4be6bd1b-2660-47b5-931d-8692d0b26b4f)

![Image](https://github.com/user-attachments/assets/89b604a8-f8c2-4362-a86d-b24a043fd745)

![Image](https://github.com/user-attachments/assets/9d4bc73e-1b5a-4a67-99ec-7af2410db067)

![Image](https://github.com/user-attachments/assets/ceae2309-0c4b-412c-a2e8-09f993dd6d2a)

![Image](https://github.com/user-attachments/assets/68feff22-52fc-4b9d-90cf-e6d8712cfd45)

![Image](https://github.com/user-attachments/assets/c3291c2a-e317-4720-adab-da1affd0f6b0)

## 🚫 Exclusiones

El repositorio ignora automáticamente archivos pesados o temporales:

- Archivos `.png` y `.mp4` (por tamaño)
- Directorios `Library/`, `Temp/`, `Obj/`, `Build/`
- Archivos grandes con `.dylib`, `.bundle`, etc.

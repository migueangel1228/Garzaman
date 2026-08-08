# Garza-Man 2078: Puchi's Escape (The Goose Sanctuary)

[![Unity 2022.3+ URP](https://img.shields.io/badge/Unity-2022.3%2B%20URP-blue.svg?style=flat&logo=unity)](https://unity.com/)
[![Genre](https://img.shields.io/badge/Genre-First--Person%20Survival%20Horror-red.svg)](https://github.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20PC-lightgrey.svg)](https://github.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**Garza-Man 2078** (título de desarrollo: *Puchi's Escape: The Goose Sanctuary*) es un prototipo MVP (Minimum Viable Product) de **videojuego de terror y supervivencia en primera persona** desarrollado en Unity utilizando el Universal Render Pipeline (URP). 

El jugador encarna a **Puchi**, atrapado en un santuario con estética tribal-futurista. Para escapar con vida, debe explorar el mapa, recolectar **10 plumas sagradas** y desbloquear la puerta principal mientras evita ser capturado por un depredador mutante controlado por IA.

---

## 📸 Descripción del Juego

El juego combina mecánicas de sigilo, gestión de ruido y exploración bajo tensión constante en entornos oscuros. Inspirado en clásicos del género como *Amnesia: The Dark Descent*, *Slender* y *Alien: Isolation* (en una versión simplificada).

### 🎯 Bucle Principal de Juego (*Core Loop*)
1. **Exploración Tensa:** Explorar el santuario a oscuras utilizando la linterna.
2. **Gestión de Ruido y Visibilidad:** Alternar entre agacharse, caminar y correr para no ser detectado por la IA.
3. **Colección de Objetivos:** Localizar e interactuar con las **10 plumas sagradas** repartidas en el mapa.
4. **Zonas Seguras (*Safe Zones*):** Utilizar refugios estratégicos para romper la línea de visión de la IA.
5. **Escape:** Regresar a la puerta de salida principal una vez recolectadas todas las plumas.

---

## 🛠️ Arquitectura Técnica y Sistemas Principales

El proyecto se estructura con código C# modular enfocado en rendimiento y comportamiento reactivo de la IA.

### 1. Sistema de IA Enemiga (`GooseAI.cs`)
El ganso mutante opera mediante una **Máquina de Estados Finitos (FSM)** avanzada acoplada al sistema `NavMeshAgent` de Unity:

* **Estados de IA:**
  * `Patrol`: Recorre puntos de patrulla dinámicos de forma aleatoria sin repetir rutas consecutivas.
  * `Suspicious`: Se detiene e inspecciona la última posición de ruido/visión detectada.
  * `Investigate`: Camina hacia la última ubicación conocida del jugador y busca en la zona antes de retomar la patrulla.
  * `Chase`: Persigue directamente al jugador a mayor velocidad cuando la sospecha alcanza el 100%.
  * `Return`: Regresa a la ruta habitual de patrulla si pierde al jugador.
* **Percepción Multi-Sensorial:**
  * **Visión Cono Raycast:** Raycasting multi-punto (pies, torso, cabeza) dentro de un ángulo de visión. El rango efectivo se reduce si el jugador está agachado (65%) o se incrementa si corre (115%).
  * **Detección Auditiva Dinámica:** Radio de oído que reacciona a la velocidad del jugador (Agachado = 40% del radio base, Correr = 200% del radio base).
  * **Medidor de Sospecha Acumulativo (0-100%):** Aumenta progresivamente según la proximidad y decae cuando el jugador se oculta.
* **Recuperación y Anti-Atasco:** Manejo de rutas inalcanzables y re-muestreo de `NavMesh` para evitar que la IA quede bloqueada.
* **Audio Espacializado:** Control de ruidos de pasos y rugidos (*growls*) dinámicos según el estado de la IA.

### 2. Control del Jugador y Sigilo (`MyPlayerControls.cs`, `FlashlightController.cs`)
* Implementado con el **New Input System** (`com.unity.inputsystem`).
* Soporte para movimiento en primera persona (Caminar, Correr, Agacharse, Saltar).
* Cambio dinámico de altura del `CharacterController` al agacharse.
* Alternancia de linterna (*Flashlight*) mediante la tecla `F`.

### 3. Interacción y Gestión de Estado (`PlayerStats.cs`, `FeatherHUD.cs`, `ExitDoor.cs`, `GameManager.cs`)
* **Sistema de Interacción Base (`Interactable.cs`):** Detección de objetos mediante Raycast.
* **HUD dinámico en TextMeshPro:** Muestra el contador de plumas actualizándose mediante eventos de Unity (`UnityEvent`).
* **Zonas Seguras (`SafeZone.cs`):** Triggers que desactivan temporalmente la detección de la IA cuando el jugador está oculto.
* **GameManager Singleton:** Control de flujo de juego (Victoria / Game Over) con soporte para cinemáticas de video vía `VideoPlayer` (`StreamingAssets/gameOver.mp4` / `victory.mp4`).

---

## 🎮 Controles

| Acción | Tecla / Control | Descripción |
| :--- | :--- | :--- |
| **Movimiento** | `W` `A` `S` `D` | Desplazamiento del personaje |
| **Cámara** | `Ratón` | Mirar a alrededor (Cursor bloqueado) |
| **Correr** | `Left Shift` | Mayor velocidad / Aumenta ruido y visibilidad |
| **Agacharse** | `Left Ctrl` | Menor altura / Reduce ruido y visibilidad |
| **Saltar** | `Espacio` | Salto del personaje |
| **Linterna** | `F` | Encender / Apagar linterna |
| **Interactuar** | `E` | Recoger plumas / Abrir puerta de salida |

---

## 📁 Estructura del Repositorio

```text
Garzaman/
├── Garza-Man 2078/               # Proyecto principal de Unity
│   ├── Assets/
│   │   ├── Plans/                # Documentación del MVP (mvp-horror-goose.md)
│   │   ├── Scenes/               # Escenas del juego (Garzaman2077.unity, Menu.unity)
│   │   ├── Scripts/              # Código fuente C#
│   │   │   ├── GooseAI.cs        # FSM e IA del enemigo mutante
│   │   │   ├── GameManager.cs    # Controlador global del estado de juego
│   │   │   ├── PlayerStats.cs    # Estado del jugador y contador de plumas
│   │   │   ├── FeatherHUD.cs     # Interfaz de usuario (TextMeshPro)
│   │   │   ├── FlashlightController.cs # Control de linterna
│   │   │   ├── ExitDoor.cs       # Lógica de salida del santuario
│   │   │   ├── SafeZone.cs       # Zonas de refugio
│   │   │   └── ...
│   │   ├── StreamingAssets/      # Videos cinemáticos de Victoria y Game Over
│   │   ├── UI/                   # Sprites y layouts de interfaz
│   │   └── ...
│   ├── Packages/                 # Manifiesto de paquetes de Unity
│   └── ProjectSettings/          # Configuración del proyecto y mapa de inputs
├── .gitignore                    # Filtros de Git para proyectos Unity
└── README.md                     # Documentación general del proyecto
```

---

## ⚙️ Requisitos e Instalación

### Requisitos del Sistema
* **Unity Version:** Unity 2022.3 LTS o superior (recomendado 2022.3.x).
* **Render Pipeline:** Universal Render Pipeline (URP).
* **Paquetes Necesarios:**
  * `Input System` (`com.unity.inputsystem`)
  * `TextMeshPro` (`com.unity.textmeshpro`)
  * `AI Navigation` (`com.unity.ai.navigation`)

### Pasos para Abrir el Proyecto
1. Clona este repositorio en tu equipo local:
   ```bash
   git clone https://github.com/migueangel1228/Garzaman.git
   ```
2. Abre **Unity Hub**.
3. Haz clic en **Add** -> **Add project from disk**.
4. Selecciona la carpeta `Garza-Man 2078`.
5. Abre el proyecto con la versión adecuada de Unity.
6. En la pestaña `Project`, navega a `Assets/Scenes/` y abre la escena `Menu.unity` o `Garzaman2077.unity`.
7. Presiona **Play** en el editor de Unity.

---

## 📈 Estado Actual y Hoja de Ruta (*Roadmap*)

### 🔄 Implementado en el MVP Actual
- [x] Controlador de movimiento en primera persona con sistema de sigilo (Caminar / Correr / Agacharse).
- [x] IA enemiga completa con FSM (Patrulla, Sospecha, Investigación, Persecución, Retorno).
- [x] Percepción auditiva y visual dinámica de la IA.
- [x] Mecánica de recolección de plumas e interfaz HUD en tiempo real.
- [x] Puerta de salida con condición de victoria (10 plumas).
- [x] Zonas seguras (*Safe Zones*) funcionales.
- [x] Pantallas de Victoria y Game Over con reproductor de video.

### 🚀 Futuras Mejoras Planteadas
- [ ] Implementación de eventos sonoros de distracción (lanzar objetos para despistar a la IA).
- [ ] Variedad de animaciones avanzadas para la IA del ganso.
- [ ] Puntos de patrulla dinámicos aleatorios según la fase del juego.
- [ ] Optimización de iluminación volumétrica y post-procesado en URP.

---

## 📝 Licencia

Este proyecto se distribuye bajo la licencia **MIT**. Consulta el archivo `LICENSE` para obtener más información.

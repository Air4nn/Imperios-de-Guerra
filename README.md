# ⚔️ IMPERIOS EN GUERRA 🏰

### 🎮 Videojuego de Estrategia en Tiempo Real — RTS

> **Proyecto de Programación Avanzada**

Dos imperios ⚔️ se enfrentan por el control de un mapa 🗺️.
Los jugadores deberán administrar recursos 💰, construir edificios 🏠, entrenar unidades ⚔️ y derrotar al enemigo 🏆.

---

## 👨‍💻 Información del proyecto

| 📌                      | Información            |
| ----------------------- | ---------------------- |
| 🎮 Proyecto             | **Imperios en Guerra** |
| 💻 Lenguaje             | **C#**                 |
| 🎨 Motor                | **Unity**              |
| 🧠 Arquitectura         | **MVC**                |
| 🌐 Red                  | **TCP / Sockets**      |
| ⚡ Concurrencia          | **Thread / Task**      |
| 🗃️ Archivos            | **TXT / JSON**         |
| 🔧 IDE                  | **Visual Studio Code** |
| 📦 Control de versiones | **Git + GitHub**       |

---

# 🎯 Objetivo

Crear un videojuego RTS para computador donde **dos jugadores** puedan competir en un mismo mapa.

Cada jugador podrá:

* 💰 Administrar recursos.
* 🏠 Construir edificios.
* ⚔️ Entrenar unidades.
* 🏃 Mover unidades.
* 💥 Atacar unidades enemigas.
* 🏰 Atacar edificios.
* 🏆 Conseguir la victoria.

---

# 🧠 Arquitectura del proyecto

El proyecto utiliza el patrón **MVC — Modelo, Vista y Controlador**.

```text
                 👤 JUGADOR
                      │
                      ▼
               🎨 VISTA / UNITY
                      │
                      ▼
                🎮 CONTROLADOR
                      │
                      ▼
                 🧠 MODELO
                      │
                      ▼
                ⚙️ REGLAS DEL JUEGO
```

### 🧠 Modelo

Es el **cerebro del juego**.

Contiene:

* 👤 `Jugador`
* 🎮 `Partida`
* 🗺️ `Mapa`
* 🟩 `Celda`
* 💰 `Recurso`
* ⚔️ `Unidad`
* 🏰 `Edificio`
* 🏷️ `Edificio`

El Modelo no depende de Unity.

---

### 🎨 Vista

Es la parte que el jugador puede ver e interactuar en Unity.

Incluye:

* 💎 `BarraRecursos`
* ❤️ `BarraVida`
* 🏰 `EdificioView`
* 🏭 `FabricaVisual`
* 🎮 `GameManager`
* 🖼️ `IconosUI`
* 🎯 `IndicadorSeleccion`
* 🖱️ `InteraccionMapa`
* 🗺️ `MapaView`
* 🏆 `MenuFinal`
* 📶 `MenuRed`
* 🎛️ `PanelAcciones`
* ❓ `PanelAyuda`
* 💬 `TextoFlotante`
* 🖥️ `UIManager`
* ⚔️ `UnidadView`

---

### 🎮 Controlador

Conecta la interfaz con la lógica del juego.

Incluye:

* `JuegoController`

---

# 🗺️ El mapa

El mapa inicial tiene:

```text
15 × 15
```

Cada posición corresponde a una `Celda`.

Una celda puede contener:

```text
🟩 Terreno
💰 Recurso
⚔️ Unidad
🏠 Edificio
```

Los dos jugadores utilizan **el mismo mapa compartido**.

---

# 👥 Jugadores

Cada jugador posee:

### 💰 Recursos

* 🟡 Oro
* 🪵 Madera
* 🍎 Comida

### ⚔️ Unidades

* 🗡️ Soldado


### 🏰 Edificios

* 🏯 Centro Urbano
* 🏠 Casa


---

# ⚙️ Acciones principales

## 🏠 Construir

```text
👤 Jugador
   ↓
🎮 Controlador
   ↓
💰 ¿Tiene recursos?
   ↓
🟢 Sí
   ↓
🏠 Crear edificio
   ↓
🗺️ Ocupar celda
```

---

## ⚔️ Entrenar

```text
🏰 Edificio
   ↓
💰 Comprobar recursos
   ↓
⚔️ Crear unidad
   ↓
👤 Agregar al jugador
```

---

## 🏃 Mover

```text
⚔️ Seleccionar unidad
        ↓
🖱️ Seleccionar destino
        ↓
🧠 Validar movimiento
        ↓
🗺️ Cambiar posición
        ↓
🎨 Actualizar Unity
```

---

## 💥 Atacar

```text
⚔️ Atacante
      ↓
🎯 Objetivo
      ↓
💥 Aplicar daño
      ↓
❤️ Reducir vida
      ↓
☠️ ¿Vida = 0?
      ↓
🏆 Comprobar victoria
```

---

# 🏆 Victoria

La partida puede finalizar cuando se cumple una condición de victoria, por ejemplo:

🏰 **Destruir el Centro Urbano enemigo**

o

⚔️ **Eliminar las unidades militares enemigas**

Cuando termina:

```text
🏆 GANADOR
   ↓
💾 Guardar resultado
   ↓
📄 resultado_final.txt
```

---

# ⚡ Concurrencia

Se utilizaron tareas o hilos para procesos como:

* 💰 Recolección de recursos.
* 🏗️ Construcción.
* ⚔️ Entrenamiento.
* 🏃 Movimiento.
* 🌐 Escucha de conexiones.

Se utilizaron mecanismos de sincronización como:

```csharp
lock
```

para proteger información compartida.

⚠️ Los procesos secundarios **no modificarán directamente los objetos visuales de Unity**.

---

# 🌐 Multijugador

La comunicación entre jugadores se realizará mediante:

```text
Jugador 1
   │
   │ 🌐 TCP
   ▼
Conexión
   ▲
   │ 🌐 TCP
   │
Jugador 2
```

Las acciones podrán enviarse mediante mensajes estructurados.

Ejemplo:

```json
{
  "tipo": "MOVER",
  "jugador": 1,
  "unidad": 5,
  "x": 7,
  "y": 8
}
```

---

# 💾 Archivos

El juego generará información importante:

### ⚙️ `configuracion.txt`

Guarda la configuración inicial.

### 📜 `log_partida.txt`

Registra eventos importantes.

```text
Jugador 1 construyó una Casa.
Jugador 2 entrenó un Soldado.
Jugador 1 atacó una unidad.
```

### 🏆 `resultado_final.txt`

Guarda el resultado de la partida.

```text
Partida finalizada
Ganador: Jugador 1
```

---

# 📁 Estructura del proyecto

```text
ImperiosEnGuerra/
│
├── 📂 Assets/
│   │
│   ├── 🎬 Scenes/
│   │   └── Partida.unity
│   │
│   ├── 💻 Scripts/
│   │   ├── 🧠 Modelo/
│   │   ├── ⚙️ Servicios/
│   │   ├── 🎮 Controlador/
│   │   ├── 🎨 Vista/
│   │   ├── 🌐 Red/
│   │   └── 💾 Datos/
│   │
│   ├── ⚙️ Settings/
│   ├── 📝 Text MeshPro/
│   └── 📦 Resources/
│   └── 📚 TutorialInfo/
│
└── 📄 README.md
```

---

# 🔄 Flujo general

```text
🚀 INICIAR PARTIDA
        ↓
🗺️ Crear mapa 15×15
        ↓
👥 Crear jugadores
        ↓
💰 Asignar recursos
        ↓
🏰 Crear centros urbanos
        ↓
🎨 Mostrar partida
        ↓
      ACCIONES
        │
   ┌────┼────┬────┐
   ↓    ↓    ↓    ↓
  🏠   ⚔️   🏃   💥
Construir Entrenar Mover Atacar
   │    │    │    │
   └────┴────┴────┘
        ↓
🧠 Actualizar Modelo
        ↓
🎨 Actualizar Vista
        ↓
🏆 ¿Hay ganador?
      ↙   ↘
    ❌     ✅
    │       │
    └───→ 💾 Guardar
```

---

# 🛠️ Etapas de desarrollo

El proyecto se desarrolla progresivamente:

```text
1️⃣ 🧠 Modelo
      ↓
2️⃣ ⚙️ Reglas del juego
      ↓
3️⃣ 🎮 Controlador
      ↓
4️⃣ 🎨 Unity / Vista
      ↓
5️⃣ 🏠 Construcción
      ↓
6️⃣ ⚔️ Entrenamiento
      ↓
7️⃣ 🏃 Movimiento
      ↓
8️⃣ 💥 Combate
      ↓
9️⃣ 🏆 Victoria
      ↓
🔟 💾 Archivos
      ↓
1️⃣1️⃣ ⚡ Concurrencia
      ↓
1️⃣2️⃣ 🌐 Red
      ↓
1️⃣3️⃣ 🎨 Mejoras visuales
      ↓
1️⃣4️⃣ 🧪 Pruebas
      ↓
1️⃣5️⃣ 📦 Ejecutable final
```

---

# 🧪 Pruebas

Se comprobo lo siguiente:

| 🧪 Área         | ✅ Pruebas                                   |
| --------------- | ------------------------------------------- |
| 🧠 Modelo       | Crear jugadores, mapa, unidades y edificios |
| 💰 Recursos     | Recolectar, gastar y validar recursos       |
| 🏠 Construcción | Construcción válida e inválida              |
| ⚔️ Unidades     | Crear, mover y atacar                       |
| 💥 Combate      | Daño y destrucción                          |
| 🏆 Victoria     | Detección del ganador                       |
| ⚡ Concurrencia  | Procesos simultáneos                        |
| 🌐 Red          | Conexión y envío de acciones                |
| 💾 Archivos     | Configuración, logs y resultado             |

# 👨‍💻 Autor

* Airann Estiben Yepes Barrera 



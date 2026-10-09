# ESTRUCTURA CONCEPTUAL DE GOOGLE SHEETS
## FLAPPY SPACE CAT - DOCUMENTACIÓN GENERAL

---

Este documento describe la estructura y el diseño conceptual de la plantilla de **Google Sheets** que se compartirá con el docente, cumpliendo con la exigencia de definir en hojas de cálculo ordenadas la analítica implementada, los casos de uso principales, el modelo de monetización y el análisis del mercado.

---

## 📊 PESTAÑA 1: FUNNEL DE CONVERSIÓN Y ANALÍTICA (ONBOARDING & RETENCIÓN)

Esta hoja monitoriza el comportamiento telemétrico de los jugadores desde que instalan la aplicación hasta que se convierten en usuarios recurrentes, permitiendo auditar la efectividad de las decisiones de diseño del juego.

### Estructura de Columnas de la Pestaña 1
* **Paso**: Índice numérico del embudo.
* **Hito de Conversión**: Acción que realiza el jugador.
* **Script Responsable**: Script de C# que dispara el registro.
* **Evento Firebase Analytics**: Firma del evento registrado en la consola de la nube.
* **Tasa de Conversión Objetivo (%)**: Porcentaje ideal de usuarios que superan el paso.
* **Fuga / Punto de Fricción**: Razón por la que el usuario abandona en este punto.
* **Acción Correctiva de Diseño (LiveOps)**: Medida técnica para solucionar la fuga.

### Datos del Embudo de Conversión (Funnels)

| Paso | Hito de Conversión | Script Responsable | Evento Analytics | Tasa Obj | Fuga / Punto de Fricción | Acción Correctiva (LiveOps) |
| :--- | :--- | :--- | :--- | :---: | :--- | :--- |
| **1** | Descarga y Apertura | `FirebaseManager.cs` | `session_start` | **100%** | Tiempos de carga prolongados en el arranque o errores de red. | Optimizar assets precompilados y asegurar inicialización offline robusta. |
| **2** | Iniciar Partida | `GameManager.cs` | `inicio_juego` | **90%** | El menú principal se siente confuso, saturado o carente de llamada a la acción. | Ajustar el contraste visual del botón "Play" y añadir animaciones de pulso. |
| **3** | Primera Muerte | `GameManager.cs` | `jugador_muere` | **85%** | Curva de dificultad empinada que frustra al usuario de manera temprana. | Reducir la velocidad de inicio (`velocidadInicial`) a través de Remote Config. |
| **4** | Registro de Perfil | `AuthManager.cs` | `registro_email_exito` / `registro_google_exito` | **45%** | Pereza al escribir credenciales o desconfianza de seguridad de datos. | Incentivar mediante el premio gratis de skin Cowsmo (Vaca) en las primeras 24 horas. |
| **5** | Usuario Comprometido | `LogrosManager.cs` | Logro `AgujeroNegro` (50 partidas) | **15%** | Monotonía en el runner, falta de incentivos de progresión estéticos. | Generar y liberar nuevas skins temáticas en la tienda en caliente sin actualizar la app. |

---

## 💸 PESTAÑA 2: MATRIZ DE MONETIZACIÓN (MÉTRICAS DE RETORNO E eCPM)

Esta hoja modela el plan financiero del producto, proyectando los ingresos por publicidad y el Valor de Vida del Cliente (*LTV: Lifetime Value*).

### Estructura de Columnas de la Pestaña 2
* **Formato de Anuncio**: Tipo de bloque de AdMob.
* **ID Bloque AdMob**: Identificador de pruebas y producción.
* **Ubicación en UI**: Cuándo y dónde se despliega en la pantalla.
* **Script Responsable**: Script C# que orquesta el bloque publicitario.
* **Evento de Disparo**: Cuándo se realiza la llamada técnica.
* **eCPM Proyectado ($)**: Ingreso promedio estimado por cada 1,000 impresiones del bloque.
* **Impresiones Promedio / Sesión**: Frecuencia con la que el usuario visualiza el bloque en una partida.
* **LTV Estimado / Usuario ($)**: Proyección de ingresos directos por jugador activo a 30 días.

### Matriz Financiera de Publicidad

| Formato de Anuncio | ID Bloque AdMob (Pruebas) | Ubicación en UI | Script Responsable | Evento de Disparo | eCPM Proy | Imp / Sesión | LTV Est / Usuario |
| :--- | :--- | :--- | :--- | :--- | :---: | :---: | :---: |
| **Banner Estático** | `ca-app-pub-3940256099942544/6300978111` | Parte superior de menús activos. | `PublicidadManager.cs` | Carga al iniciar. Oculto en gameplay. | **$0.50** | 1.8 | **$0.02** |
| **Intersticial** | `ca-app-pub-3940256099942544/1033173712` | Transición al salir de partida. | `PublicidadManager.cs` | Al pulsar volver al Menú Principal o tras declinar continuar. | **$3.50** | 0.8 | **$0.08** |
| **Rewarded Video** | `ca-app-pub-3940256099942544/5224354917` | Botón +20 gemas y Revivir en Game Over. | `PublicidadManager.cs` | A demanda (resurrección o carga de gemas desde el menú). | **$8.00** | 1.2 | **$0.24** |

$$\text{LTV Proyectado Total por Usuario (30 días)} = \$0.34 \text{ USD}$$

*El modelo Free-to-Play es altamente rentable: con una base de 1,000 descargas activas recurrentes, los ingresos mensuales proyectados ascienden a $340.00 USD mediante un esquema no intrusivo y voluntario.*

---

## ⚙️ PESTAÑA 3: CASOS DE USO TÉCNICOS DEL SISTEMA

Describe de forma estructurada los flujos lógicos que interactúan entre el código y los servidores externos para asegurar que el sistema se comporte de forma estable.

### Estructura de la Pestaña 3
Cada registro en esta pestaña desglosa un caso de uso clave:
1. **ID Caso de Uso**: Identificador alfanumérico (ej: *CU-01*).
2. **Nombre**: Acción técnica a validar.
3. **Actores**: Jugador / Dispositivo / Firebase Auth / Firestore Database / Remote Config / AdMob Server.
4. **Precondición**: Qué estado debe tener el software antes de ejecutarse.
5. **Flujo Normal (Paso a Paso)**: Flujo de ejecución sin fallos.
6. **Postcondición**: Estado final resultante en local y en la nube.
7. **Flujo Excepcional (Errores)**: Cómo reacciona el software si falla la red o los servidores.

### Datos de los Casos de Uso Clave

#### ➔ CU-01: Registro de Jugador y Sincronización de Progreso
* **Actores**: Jugador ➔ Dispositivo ➔ Firebase Auth ➔ Firestore Database.
* **Precondición**: El usuario ha jugado su primera partida offline/online y acumulado gemas y récords en `SecurePrefs`.
* **Flujo Normal**:
  1. El jugador ingresa su ID y contraseña en el panel de inicio de sesión y pulsa "Registrar".
  2. `AuthManager.cs` inyecta internamente el dominio sintético `@flappyspacecat.com` y despacha a Firebase Auth.
  3. Firebase Auth retorna confirmación de cuenta con una ID de usuario (`UserId`) única en el mundo.
  4. `DatabaseManager.cs` invoca `GuardarGemasEnNube` y `GuardarMejorPuntuacionEnNube` enviando los récords locales.
  5. Firestore almacena la colección bajo fusión de documentos (`MergeAll`) para persistir la información.
* **Postcondición**: El perfil del usuario está creado de forma segura en la nube y los datos locales quedan vinculados asíncronamente a su ID.
* **Flujo Excepcional**: Si no hay conexión de datos en el paso 2, el `AuthManager` intercepta el fallo de red, notifica visualmente mediante `textoAvisos` y mantiene los récords guardados localmente de forma encriptada en `SecurePrefs` para subirlos de manera diferida en el próximo arranque con red.

#### ➔ CU-02: Mecánica de Resurrección mediante Reward Video
* **Actores**: Jugador ➔ Dispositivo ➔ AdMob SDK ➔ GameManager.
* **Precondición**: El jugador muere durante el gameplay y el panel de Game Over está activo con el botón "Revivir".
* **Flujo Normal**:
  1. El jugador pulsa el botón "Revivir".
  2. `PublicidadManager.cs` invoca `MostrarRewardedGameOver()` comprobando que el anuncio esté cacheado en memoria.
  3. El usuario visualiza el 100% de la publicidad y la API de AdMob envía la señal de validación del premio.
  4. Se ejecuta el callback `DarRecompensaRevivir()`, el cual activa `reanudarJuego = true` y despacha el evento analítico.
  5. En el hilo principal (`Update`), se procesa y llama a `GameManager.Instancia.ContinuarPartida()`.
  6. `GameManager` destruye los dos meteoritos más próximos por delante y revive al gato en su coordenada inicial.
* **Postcondición**: El jugador continúa su partida y conserva su puntuación acumulada sin fricciones.
* **Flujo Excepcional**: Si el anuncio no se encuentra cacheado debido a baja conexión, el GameManager intercepta el fallo e inicia la partida de inmediato de forma gratuita para no frustrar la experiencia de juego del usuario.

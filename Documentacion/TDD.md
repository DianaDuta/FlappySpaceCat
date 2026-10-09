# TECHNICAL DESIGN DOCUMENT (TDD)
## FLAPPY SPACE CAT

---

## 1. MÓDULO DE GAMEPLAY Y CONTROLADOR FÍSICO

Este módulo agrupa la lógica de control del personaje, el bucle principal de juego, las variables de dificultad temporal y la gestión física de colisiones y límites de escenario.

### 1.1. `ControladorJugagor.cs`
* **Responsabilidad**: Gestionar el comportamiento físico bidimensional, la detección táctil de saltos, las animaciones asociadas y la muerte del personaje principal.
* **Ciclo de Vida y Eventos**:
  * `Start()`:
    1. Resuelve las referencias locales a los componentes (`Rigidbody2D`, `Animator`, `AudioSource`).
    2. Localiza de forma responsiva el objeto de referencia `"Punto_Aparicion_Jugador"` y reposiciona el personaje.
    3. Calcula dinámicamente el límite inferior de muerte en base al tamaño de la pantalla y del sprite del gato.
    4. Carga la gravedad dinámica desde `LectorConfiguracion.Datos.gravedadJugador`.
  * `Update()`:
    1. Detecta la pulsación en el fotograma activo (`Input.GetMouseButtonDown(0)`).
    2. Si el personaje está vivo y la simulación activa (`Time.timeScale > 0f`), inyecta velocidad vertical ascendente.
    3. Controla las transiciones del Animator en base al sentido de la velocidad vertical (`linearVelocity.y`).
    4. Limita la posición en Y al límite de vuelo superior (`limiteArriba`).
    5. Dispara el Game Over si el personaje cae por debajo de `limiteAbajo`.
  * `OnCollisionEnter2D(Collision2D)`: Captura colisiones con cualquier objeto etiquetado como sólido, anulando la vida del gato cosmonauta e invocando las alertas acústicas y mecánicas.

* **API Interna (Métodos Clave)**:
  * `public void Revivir()`: Restablece la posición, rotación física, velocidades lineales y angulares a cero, reactiva el componente Animator y reestablece el Sprite de la cara original.

---

### 1.2. `GameManager.cs`
* **Responsabilidad**: Actuar como controlador central (*Singleton*) que gestiona el estado principal de la aplicación, variables globales de sesión, contadores, dificultad dinámica y orquestación de la interfaz gráfica.
* **Ciclo de Vida y Métodos Clave**:
  * `Awake()`: Inicializa la instancia global persistente, inyecta el Mixer de audio de fondo e inicializa el registro del primer arranque del usuario.
  * `Start()`: Llama a `MostrarMenuPrincipal()`.
  * `Update()`: Si el juego está activo, acumula el temporizador de fotogramas. Cada `tiempoParaAumentar` (10 segundos) incrementa la velocidad global del juego en `cantidadAumento` (0.5f), acelerando el movimiento de meteoritos y fondos dinámicos.
  * `public void GenerarJugadorConSkin()`: Destruye avatares remanentes en el escenario, lee el índice de la skin en `SecurePrefs` y realiza la instanciación dinámica del prefab de la skin activa.
  * `public void IniciarJuego()`: Resetea contadores, inicializa la velocidad al valor base, destruye obstáculos de partidas previas, genera el gato cosmonauta en su pivote e inicia el bucle físico restableciendo `Time.timeScale = 1f`.
  * `public void ActivarGameOver()`: Detiene la simulación física (`Time.timeScale = 0f`), calcula y suma de forma segura las gemas acumuladas, actualiza los récords en Firestore de manera asíncrona si hay un perfil enlazado, y muestra las ventanas informativas de Game Over e inicio de sesión si es la primera partida del usuario.
  * `public void ContinuarPartida()`: Reanuda el juego consumiendo la opción de resurrección. Utiliza un algoritmo para **ordenar los obstáculos en pantalla por posición X y eliminar los dos meteoritos adyacentes más próximos** por delante del jugador, ofreciendo un área de amortiguación segura de 2 unidades tras revivir.

---

## 2. MÓDULO DE RED, AUTENTICACIÓN Y PERSISTENCIA CLOUD

Este módulo centraliza las peticiones asíncronas hacia servidores Firebase, gestiona los accesos de usuario seguros y el guardado de datos a prueba de trampas de forma local y en la nube.

### 2.1. `AuthManager.cs`
* **Responsabilidad**: Administrar la creación y validación de perfiles de usuario utilizando Firebase Auth y Google Sign-In, enmascaramiento dinámico de contraseñas y recompensas de registro temprano.
* **API de Control de Contraseña**:
  * `public void AlternarVisibilidadPassword()`: Alterna el `contentType` del input entre texto estándar y contraseña enmascarada, forzando a TextMeshPro a redibujar el contenido al instante con `ForceLabelUpdate()`.
* **API de Autenticación**:
  * `public void RegistrarUsuarioConEmail()`:
    1. Realiza validaciones locales de formato de contraseña (mínimo 8 caracteres, al menos un dígito y una letra).
    2. Construye el correo virtual agregando el dominio sintético `@flappyspacecat.com` si se introduce un identificador plano.
    3. Invoca `CreateUserWithEmailAndPasswordAsync()`.
    4. Al completarse con éxito, sincroniza las gemas y puntuaciones locales con la nueva cuenta en Firestore y valida el premio Cow.
  * `public void RegistrarUsuarioConGoogle()`: Inicializa el SDK nativo `GoogleSignInConfiguration`, solicita el token de autenticación a los servicios de Google y valida las credenciales asíncronamente con Firebase.
  * `private void ComprobarRecompensaRegistro()`: Compara la marca temporal en local `FechaPrimeraApertura`. Si la cuenta se crea en un lapso de 24 horas o menos desde la primera apertura del juego, desbloquea de forma gratuita la skin Cow en el índice 4 de `SecurePrefs` y actualiza la UI.

---

### 2.2. `DatabaseManager.cs`
* **Responsabilidad**: Concentrar la lógica de acceso asíncrono para leer y persistir colecciones orientadas a documentos en Firebase Firestore.
* **Métodos de Escritura asíncrona**:
  * `public void GuardarGemasEnNube(string idUsuario, int cantidadGemas)`: Envía asíncronamente un diccionario a la colección `"Jugadores"` con la clave `"gemasTotales"` y la marca de tiempo de red de Firebase (`FieldValue.ServerTimestamp`), aplicando `SetOptions.MergeAll` para evitar la pérdida de otros metadatos guardados.
  * `public void GuardarMejorPuntuacionEnNube(string idUsuario, int mejorPuntuacion)`: Persiste el récord máximo en el documento del jugador.
* **Métodos de Consulta y Lectura**:
  * `public void ObtenerMejorPuntuacion(string idUsuario, Action<int> alCompletar)`: Descarga el snap del documento del jugador. Si existe el campo `"mejorPuntuacion"`, devuelve el entero correspondiente al callback; de lo contrario, retorna 0.
  * `public void ObtenerGemasTotales(string idUsuario, Action<int> alCompletar)`: Recupera la cantidad de gemas almacenadas en la nube.

---

### 2.3. `SecurePrefs.cs`
* **Responsabilidad**: Actuar como una capa de encriptación transparente sobre los datos de guardado local, evitando que programas de manipulación de memoria o archivos XML modifiquen las monedas del juego.
* **Arquitectura de Cifrado**:
  * Genera una clave simétrica unívoca vinculada al identificador del chip de hardware (`SystemInfo.deviceUniqueIdentifier`).
  * Utiliza un algoritmo de cifrado **AES-256** para codificar todas las cadenas escritas en disco.
  * Agrega una firma digital basada en **HMAC-SHA256**. Si un usuario altera el archivo de preferencias para sumarse gemas, la firma queda invalidada, lo que indica una manipulación. El módulo desecha el archivo corrupto de inmediato y regenera los valores iniciales seguros de contingencia.

---

## 3. SERVICIOS DINÁMICOS, MONETIZACIÓN Y LOGROS

Este módulo gestiona la inyección de parámetros dinámicos, la sincronización de bloques publicitarios AdMob y la administración social de logros y marcadores en Google Play.

### 3.1. `LectorConfiguracion.cs` y `RemoteConfigManager.cs`
* **Responsabilidad**: Proveer la deserialización de archivos de configuración remotos y locales en tiempo real.
* **Lógica del Parser**:
  1. `LectorConfiguracion` busca en disco el archivo `configuracion.json` dentro de `StreamingAssets`.
  2. Mediante el parser nativo, deserializa el texto plano en la clase estática `DatosJuego`:
     * `velocidadJuego`: Constante base de movimiento.
     * `frecuenciaObstaculos`: Temporizador de generación.
     * `gravedadJugador`: Escala física del Rigidbody2D del gato.
  3. `RemoteConfigManager` inicializa los valores asumiendo dichos campos locales como predeterminados y dispara la petición Fetch asíncrona hacia Firebase Remote Config. Al descargar los datos de red, los valida e inyecta dinámicamente en el `GameManager.Instancia`.

---

### 3.2. `PublicidadManager.cs`
* **Responsabilidad**: Solicitar, precargar, cachear y reproducir de forma asíncrona los bloques publicitarios de Google AdMob.
* **API de Control de Anuncios**:
  * `private void CargarBanner()`: Instancia el `BannerView` asociando el ID de AdMob en la posición superior de pantalla (`AdPosition.Top`).
  * `public void MostrarBanner()` / `OcultarBanner()`: Activan o suspenden programáticamente la visualización del banner.
  * `private void CargarIntersticial()`: Realiza la petición de precarga del bloque de pantalla completa. Suscribe el callback de cierre para recargar.
  * `public void MostrarIntersticial()`: Muestra el anuncio de transición en la salida al Menú Principal.
  * `public void MostrarAnuncioRecompensado()`: Activa el rewarded para el botón de obtención de 20 gemas, marcando la bandera interna `esDeGameOver = false`.
  * `public void MostrarRewardedGameOver()`: Activa el rewarded de Game Over para permitir revivir al jugador en su última coordenada, marcando `esDeGameOver = true`.
  * `private void DarRecompensaGemas()`: Otorga las 20 gemas estelares, dispara la telemétrica, valida logros de patrocinio e invoca los callbacks del temporizador.
  * `private void DarRecompensaRevivir()`: Marca la bandera interna `recompensaRevivirObtenida = true`. El resurgimiento y reanudación del juego se ejecutarán asíncronamente únicamente cuando el anuncio sea cerrado (`OnAdFullScreenContentClosed`), iniciando una cuenta atrás de 1.0 segundos reales desescalados (`temporizadorReanudacion`) en el hilo principal (`Update()`) antes de continuar la partida.

---

### 3.3. `LogrosManager.cs`
* **Responsabilidad**: Integrar el juego con **Google Play Games Services**, administrando la sincronización de logros offline/online y comunicándose de forma bidireccional con el servicio analítico.
* **API del Sistema de Logros**:
  * `public void IniciarGooglePlayGames()`: Inicializa el plugin `PlayGamesPlatform`, activa los registros de depuración para Android, y llama a la autenticación silenciosa `Social.localUser.Authenticate`. Si tiene éxito, invoca automáticamente a `SincronizarLogrosConGooglePlay()`.
  * `public void DesbloquearLogro(TipoLogro logro)`: Comprueba en `SecurePrefs` si el logro ya fue desbloqueado. Si no lo está, lo marca localmente, otorga un **premio incentivo de 100 gemas** a las reservas del jugador y realiza el reporte hacia los servidores de Google mediante `Social.ReportProgress`.
  * `public void SincronizarLogrosConGooglePlay()`: Realiza un recorrido lineal por los 20 logros oficiales enumerados en el TipoLogro. Si detecta un logro desbloqueado localmente en modo offline, realiza el reporte inmediato a Google Play, asegurando que el progreso nunca se pierda.

---

### 3.4. `TiendaSkinsManager.cs`
* **Responsabilidad**: Controlar la generación de la interfaz de la tienda mediante un scroll dinámico de tarjetas personalizadas.
* **API del Módulo**:
  * `private void GenerarBotonesSkins()`: Limpia el contenido UI previo del Canvas, realiza la instanciación dinámica del prefab de tarjeta para cada Skin de la base de datos y configura sus textos, iconos y sprites de botón (Comprar, Seleccionar o Equipado).
  * `public void IntentarComprarOEquipar(int indice)`:
    * Si la skin ya está desbloqueada, escribe en memoria que el índice es la skin equipada actual y actualiza el renderizador del menú principal.
    * Si la skin está bloqueada, valida si el total de gemas locales cubre el `precioGemas`. Si es exitoso, deduce el costo, desbloquea la skin, guarda en la base de datos Firestore de manera asíncrona, activa la inyección del personaje y valida la entrega de logros (`CambioLook` y `Coleccionista`).

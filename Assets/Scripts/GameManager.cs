using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using UnityEngine.Audio;

/// <summary>
/// Clase GameManager:
/// Componente único y global que gestiona el estado principal del juego.
/// Centraliza los datos, la puntuación, la dificultad y las transiciones entre interfaces.
/// Sigue el patrón de diseño Singleton.
/// </summary>
public class GameManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Instancia estática global del GameManager.
    /// </summary>
    public static GameManager Instancia;
    
    /// <summary>
    /// Contador actual de gemas recolectadas en la partida en curso.
    /// </summary>
    public int contadorGemas = 0;
    
    /// <summary>
    /// Puntuación actual basada en la cantidad de obstáculos superados.
    /// </summary>
    public int puntuacionObstaculos = 0;

    [Header("Ajustes de Dificultad (Velocidad)")]
    public float velocidadActual;
    public float velocidadInicial = 4f; 
    public float cantidadAumento = 0.5f; 
    public float tiempoParaAumentar = 10f; 
    
    private float temporizador = 0f;
    private int aumentosVelocidadPartida = 0; // Se utiliza para el logro VelocidadLuz

    [Header("Registro de tiempo")]
    public float tiempoUltimaGema = -1f; // Se utiliza para el logro AvaroDespistado

    [Header("Interfaz Gráfica (UI)")]
    public GameObject contenedorGemas;
    public TextMeshProUGUI textoGemas;
    public TextMeshProUGUI txtPuntuacionGameplay;
    public GameObject botonPausa;

    [Header("Textos Pantalla Game Over")]
    public TextMeshProUGUI txtPuntuacionActual_GameOver;
    public TextMeshProUGUI txtMejorPuntuacion_GameOver;

    [Header("Paneles")]
    public GameObject panelGameOver;
    public GameObject panelInicioSesion;
    public GameObject panelMenuPrincipal;
    public GameObject panelPausa; 

    [Header("UI: Variantes de Inicio de Sesión")]
    public TextMeshProUGUI txtTituloInicio;
    public TextMeshProUGUI txtSubtitulo;
    public GameObject bocadilloCowsmo;
    public GameObject textoCowsmo; 

    [Header("Música Ambiental")]
    public UnityEngine.Audio.AudioMixerGroup grupoMusica;
    public AudioClip musicaInGame;
    public AudioClip musicaMenu;
    private AudioSource audioSourceMusica;

    [Header("Sistema de Skins")]
    public GameObject[] prefabsSkinsJugador; 
    public Transform puntoAparicionJugador; 
    private ControladorJugagor jugadorActivo;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Inicializa la instancia Singleton y configura el sistema de audio básico.
    /// Se ejecuta antes de Start().
    /// </summary>
    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            
            // Registrar la primera vez que el jugador abre el juego para calcular las 24 horas de recompensa
            string fechaInicio = SecurePrefs.GetString("FechaPrimeraApertura", "");
            if (string.IsNullOrEmpty(fechaInicio))
            {
                SecurePrefs.SetString("FechaPrimeraApertura", System.DateTime.Now.ToString("O"));
                SecurePrefs.Save();
            }

            // Se crea un AudioSource de forma dinámica para reproducir la música de fondo
            audioSourceMusica = gameObject.AddComponent<AudioSource>();
            audioSourceMusica.loop = true;
            audioSourceMusica.volume = 1f;

            if (grupoMusica != null)
            {
                audioSourceMusica.outputAudioMixerGroup = grupoMusica;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inicializa el estado inicial del juego mostrando el menú principal.
    /// </summary>
    void Start()
    {
        MostrarMenuPrincipal();
    }

    /// <summary>
    /// Se lee la memoria local, se destruye el avatar actual y se instancia el prefab de la skin equipada.
    /// </summary>
    public void GenerarJugadorConSkin()
    {
        int indiceSkin = SecurePrefs.GetInt("SkinEquipada", 0);

        // Seguridad: Si el índice es mayor que la cantidad de prefabs disponibles, se restablece a 0
        if (prefabsSkinsJugador == null || prefabsSkinsJugador.Length == 0) return;
        if (indiceSkin >= prefabsSkinsJugador.Length) indiceSkin = 0;

        // Si ya existe un jugador activo en escena, se procede a su destrucción
        if (jugadorActivo != null)
        {
            Destroy(jugadorActivo.gameObject);
        }
        else
        {
            // Se elimina cualquier instancia residual colocada manualmente en la jerarquía
            ControladorJugagor gatoEnEscena = FindAnyObjectByType<ControladorJugagor>();
            if (gatoEnEscena != null) Destroy(gatoEnEscena.gameObject);
        }

        // Se instancia el nuevo avatar en las coordenadas establecidas
        Vector3 posicionNacimiento = puntoAparicionJugador != null ? puntoAparicionJugador.position : new Vector3(-7.35f, 0f, 0f);
        
        Debug.Log("[DEBUG_SPAWN] Generando jugador. SkinEquipada index: " + indiceSkin + 
                  ", Prefab Name: " + prefabsSkinsJugador[indiceSkin].name + 
                  ", puntoAparicionJugador assigned: " + (puntoAparicionJugador != null) + 
                  ", Spawn Position: " + posicionNacimiento);

        GameObject nuevoGato = Instantiate(prefabsSkinsJugador[indiceSkin], posicionNacimiento, Quaternion.identity);
        
        jugadorActivo = nuevoGato.GetComponent<ControladorJugagor>();
    }

    /// <summary>
    /// Cambia la pista de música actual si es diferente a la que ya se está reproduciendo.
    /// </summary>
    /// <param name="nuevaMusica">El clip de audio a reproducir.</param>
    private void CambiarMusica(AudioClip nuevaMusica)
    {
        if (audioSourceMusica == null || nuevaMusica == null || audioSourceMusica.clip == nuevaMusica) return;

        audioSourceMusica.clip = nuevaMusica;
        audioSourceMusica.Play();
    }

    /// <summary>
    /// Muestra la interfaz del menú principal, detiene el tiempo del juego y limpia la escena
    /// de elementos residuales de partidas previas.
    /// </summary>
    public void MostrarMenuPrincipal()
    {
        Time.timeScale = 0f;
        
        CambiarMusica(musicaMenu);

        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelInicioSesion != null) panelInicioSesion.SetActive(false);
        if (panelPausa != null) panelPausa.SetActive(false);
        if (botonPausa != null) botonPausa.SetActive(false);

        if (contenedorGemas != null) contenedorGemas.SetActive(false);
        else if (textoGemas != null) textoGemas.gameObject.SetActive(false);
        
        if (txtPuntuacionGameplay != null) txtPuntuacionGameplay.gameObject.SetActive(false);

        // Se genera el avatar con la skin equipada para su previsualización en el menú
        GenerarJugadorConSkin();

        DestruirElementosJuego();

        GeneradorMaestro genMaestro = FindAnyObjectByType<GeneradorMaestro>();
        if (genMaestro != null) genMaestro.Reiniciar();
    }

    /// <summary>
    /// Elimina todos los obstáculos y gemas instanciados en la escena para reiniciar el nivel.
    /// </summary>
    private void DestruirElementosJuego()
    {
        GameObject[] obstaculos = GameObject.FindGameObjectsWithTag("Obstaculo");
        foreach (GameObject obs in obstaculos)
        {
            Transform raiz = obs.transform.root;
            if (raiz.name.Contains("(Clone)"))
            {
                Destroy(raiz.gameObject);
            }
        }

        ColeccionableGema[] gemas = FindObjectsByType<ColeccionableGema>(FindObjectsSortMode.None);
        foreach (ColeccionableGema gema in gemas)
        {
            Transform raiz = gema.transform.root;
            if (raiz.name.Contains("(Clone)"))
            {
                Destroy(raiz.gameObject);
            }
        }
    }

    /// <summary>
    /// Inicia una nueva partida, restablece las variables de puntuación, revive al jugador
    /// y reactiva la escala de tiempo.
    /// </summary>
    public void IniciarJuego()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (botonPausa != null) botonPausa.SetActive(true);

        if (contenedorGemas != null) contenedorGemas.SetActive(true);
        else if (textoGemas != null) textoGemas.gameObject.SetActive(true);
        
        if (txtPuntuacionGameplay != null) txtPuntuacionGameplay.gameObject.SetActive(true);

        contadorGemas = 0;
        puntuacionObstaculos = 0;
        aumentosVelocidadPartida = 0;
        tiempoUltimaGema = -1f;
        ActualizarTextoPantalla();
        velocidadActual = velocidadInicial;
        temporizador = 0f;

        DestruirElementosJuego();

        // Se asegura de instanciar la skin correcta al iniciar la partida
        GenerarJugadorConSkin();
        if (jugadorActivo != null)
        {
            jugadorActivo.Revivir();
        }

        CambiarMusica(musicaInGame);

        Time.timeScale = 1f;

        AnalyticsManager.Instancia.RegistrarEventoSimple("inicio_juego");

        // Se registra la partida jugada para el sistema de logros
        if (LogrosManager.Instancia != null)
        {
            LogrosManager.Instancia.SumarPartidaJugada();

            int skinActiva = SecurePrefs.GetInt("SkinEquipada", 0);
            // Vaca (Cowsmo) = 4
            if (skinActiva == 4) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.MuuuyAlto);
            
            // Oso (Orion) = 3
            if (skinActiva == 3) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.OsoOrbital);
        }
    }

    /// <summary>
    /// Controla la lógica de actualización por cada fotograma.
    /// Gestiona el aumento progresivo de la dificultad basado en el tiempo transcurrido.
    /// </summary>
    void Update()
    {
        if (Time.timeScale > 0f)
        {
            temporizador += Time.deltaTime;

            if (temporizador >= tiempoParaAumentar)
            {
                velocidadActual += cantidadAumento;
                temporizador = 0f;
                aumentosVelocidadPartida++;
                
                if (LogrosManager.Instancia != null && aumentosVelocidadPartida >= 3)
                {
                    LogrosManager.Instancia.DesbloquearLogro(TipoLogro.VelocidadLuz);
                }
                
                Debug.Log("Incremento de dificultad. Nueva velocidad: " + velocidadActual);
            }
        }
    }

    /// <summary>
    /// Añade una cantidad específica de gemas al contador actual y actualiza la interfaz.
    /// </summary>
    /// <param name="cantidad">La cantidad de gemas a sumar.</param>
    public void SumarGema(int cantidad)
    {
        contadorGemas += cantidad;
        tiempoUltimaGema = Time.time; // Se guarda el tiempo para comprobar el logro de morir al coger gema

        if (LogrosManager.Instancia != null)
        {
            if (contadorGemas >= 10) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.RachaCodiciosa);
            if (contadorGemas >= 100) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.FiebreCristal);
        }

        Debug.Log("Gemas totales en la sesión: " + contadorGemas);
        ActualizarTextoPantalla();
    }

    /// <summary>
    /// Incrementa la puntuación obtenida al superar obstáculos y actualiza la interfaz.
    /// </summary>
    public void SumarPunto()
    {
        puntuacionObstaculos++;
        
        if (LogrosManager.Instancia != null)
        {
            if (puntuacionObstaculos == 1) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.PrimerosPasos);
            if (puntuacionObstaculos == 10) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.PilotoNovato);
            if (puntuacionObstaculos == 50) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.AstronautaHabil);
            if (puntuacionObstaculos == 100) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.CapitanEstelar);
            if (puntuacionObstaculos == 250) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.LeyendaCosmos);
        }

        ActualizarTextoPantalla();
    }

    /// <summary>
    /// Refresca los elementos de texto en la interfaz gráfica con los valores actuales.
    /// </summary>
    private void ActualizarTextoPantalla()
    {
        if (textoGemas != null) 
        {
            textoGemas.text = ": " + contadorGemas; 
        }

        if (txtPuntuacionGameplay != null)
        {
            txtPuntuacionGameplay.text = puntuacionObstaculos.ToString();
        }
    }

    /// <summary>
    /// Detiene el desarrollo de la partida, procesa la puntuación y las gemas,
    /// gestiona la persistencia de datos (local y en la nube) y despliega la interfaz de Game Over.
    /// </summary>
    public void ActivarGameOver()
    {
        Time.timeScale = 0f; 
        
        if (contenedorGemas != null) contenedorGemas.SetActive(false);
        else if (textoGemas != null) textoGemas.gameObject.SetActive(false);
        
        if (txtPuntuacionGameplay != null) txtPuntuacionGameplay.gameObject.SetActive(false);
        if (botonPausa != null) botonPausa.SetActive(false);

        CambiarMusica(musicaMenu);

        int gemasGuardadas = SecurePrefs.GetInt("GemasLocales", 0);
        int totalGemas = gemasGuardadas + contadorGemas;
        SecurePrefs.SetInt("GemasLocales", totalGemas);

        if (LogrosManager.Instancia != null)
        {
            LogrosManager.Instancia.SumarMuerte();
            
            if (puntuacionObstaculos == 0) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.VueloCorto);
            if (totalGemas >= 100) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.BolsillosLlenos);
            if (totalGemas >= 5000) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.MagnateGalaxia);
            
            // Si muere en un lapso menor a 0.2 segundos desde que cogió una gema
            if (tiempoUltimaGema > 0f && (Time.time - tiempoUltimaGema) <= 0.2f)
            {
                LogrosManager.Instancia.DesbloquearLogro(TipoLogro.AvaroDespistado);
            }
        }

        FirebaseUser usuario = FirebaseAuth.DefaultInstance.CurrentUser;
        if (usuario != null)
        {
            DatabaseManager.Instancia.ObtenerMejorPuntuacion(usuario.UserId, (mejorPuntuacionBD) =>
            {
                int recordLocal = SecurePrefs.GetInt("MejorPuntuacion", 0);
                if (mejorPuntuacionBD > recordLocal)
                {
                    SecurePrefs.SetInt("MejorPuntuacion", mejorPuntuacionBD);
                    SecurePrefs.Save();
                }

                if (puntuacionObstaculos > mejorPuntuacionBD)
                {
                    mejorPuntuacionBD = puntuacionObstaculos;
                    DatabaseManager.Instancia.GuardarMejorPuntuacionEnNube(usuario.UserId, mejorPuntuacionBD);
                    
                    SecurePrefs.SetInt("MejorPuntuacion", mejorPuntuacionBD);
                    SecurePrefs.Save();

                    OpcionesManager.VibrarSiEstaActivado();
                }

                if (txtPuntuacionActual_GameOver != null) 
                    txtPuntuacionActual_GameOver.text = "Puntuacion actual: " + puntuacionObstaculos.ToString();

                if (txtMejorPuntuacion_GameOver != null) 
                    txtMejorPuntuacion_GameOver.text = "Mejor puntuacion: " + mejorPuntuacionBD.ToString();
            });
        }
        else
        {
            int mejorPuntuacion = SecurePrefs.GetInt("MejorPuntuacion", 0);
            if (puntuacionObstaculos > mejorPuntuacion)
            {
                SecurePrefs.SetInt("MejorPuntuacion", puntuacionObstaculos);
                mejorPuntuacion = puntuacionObstaculos;

                OpcionesManager.VibrarSiEstaActivado();
            }
            if (txtPuntuacionActual_GameOver != null) 
                txtPuntuacionActual_GameOver.text = "Puntuacion actual: " + puntuacionObstaculos.ToString();

            if (txtMejorPuntuacion_GameOver != null) 
                txtMejorPuntuacion_GameOver.text = "Mejor puntuacion: " + mejorPuntuacion.ToString();
        }
            
        SecurePrefs.Save(); 
        Debug.Log("Estado guardado localmente. Gemas totales: " + totalGemas);

        bool primeraVez = SecurePrefs.GetInt("PrimeraVez", 1) == 1;

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);

            if (primeraVez)
            {
                if (panelInicioSesion != null)
                {
                    panelInicioSesion.SetActive(true);
                }
                SecurePrefs.SetInt("PrimeraVez", 0);  
            }

            AnalyticsManager.Instancia.RegistrarEventoSimple("jugador_muere");
            AnalyticsManager.Instancia.RegistrarEventoDificultad(velocidadActual);
        }
        
            
        SecurePrefs.Save(); 
    }

    /// <summary>
    /// Reanuda la partida desde el punto de interrupción, eliminando los obstáculos adyacentes
    /// al jugador para evitar colisiones inmediatas tras revivir.
    /// </summary>
    public void ContinuarPartida()
    {
        if (panelGameOver != null) panelGameOver.SetActive(false);

        if (contenedorGemas != null) contenedorGemas.SetActive(true);
        else if (textoGemas != null) textoGemas.gameObject.SetActive(true);
        
        if (txtPuntuacionGameplay != null) txtPuntuacionGameplay.gameObject.SetActive(true);
        if (botonPausa != null) botonPausa.SetActive(true);

        float xJugador = puntoAparicionJugador != null ? puntoAparicionJugador.position.x : -7.35f; 
        if (jugadorActivo != null)
        {
            jugadorActivo.Revivir();
            xJugador = jugadorActivo.transform.position.x;
        }

        GameObject[] obstaculos = GameObject.FindGameObjectsWithTag("Obstaculo");
        System.Array.Sort(obstaculos, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        int obstáculosDestruidos = 0;
        foreach (GameObject obs in obstaculos)
        {
            if (obs.transform.position.x >= xJugador - 2f) 
            {
                Destroy(obs);
                obstáculosDestruidos++;
                if (obstáculosDestruidos >= 2) break; 
            }
        }

        CambiarMusica(musicaInGame);
        Time.timeScale = 1f;

        if (LogrosManager.Instancia != null)
        {
            LogrosManager.Instancia.DesbloquearLogro(TipoLogro.Persistencia);
        }

        Debug.Log("Partida reanudada. Obstáculos adyacentes despejados.");
    }

    /// <summary>
    /// Sincroniza el progreso local con la base de datos en la nube y retorna a la interfaz principal.
    /// </summary>
    public void VolverMenuPrincipal()
    {
        FirebaseUser usuario = FirebaseAuth.DefaultInstance.CurrentUser;
        if (usuario != null)
        {
            int gemasLocales = SecurePrefs.GetInt("GemasLocales", 0);
            int mejorPuntuacion = SecurePrefs.GetInt("MejorPuntuacion", 0);

            if (DatabaseManager.Instancia != null)
            {
                if (gemasLocales > 0) DatabaseManager.Instancia.GuardarGemasEnNube(usuario.UserId, gemasLocales);
                if (mejorPuntuacion > 0) DatabaseManager.Instancia.GuardarMejorPuntuacionEnNube(usuario.UserId, mejorPuntuacion);
            }
            
            Debug.Log("Sincronización en la nube completada de forma asíncrona.");
        }

        MostrarMenuPrincipal();
    }

    [Header("Conexiones Extra")]
    public TiendaSkinsManager tiendaManager;

    /// <summary>
    /// Despliega la interfaz de la tienda delegando la acción en el TiendaSkinsManager.
    /// </summary>
    public void AbrirTiendaDesdeMenu()
    {
        if (tiendaManager != null)
        {
            tiendaManager.AbrirTienda();
        }
    }

    // -----------------------------------------------------------------------------
    // SISTEMA DE PAUSA
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Interrumpe el flujo normal del juego, pausa el tiempo y el audio,
    /// y despliega el menú de pausa.
    /// </summary>
    public void PausarJuego()
    {
        Time.timeScale = 0f; 
        if (audioSourceMusica != null) audioSourceMusica.Pause(); 
        if (panelPausa != null) panelPausa.SetActive(true); 
        if (botonPausa != null) botonPausa.SetActive(false); 
    }

    /// <summary>
    /// Restablece el tiempo y el audio, ocultando el menú de pausa para continuar la partida.
    /// </summary>
    public void ContinuarJuegoDesdePausa()
    {
        Time.timeScale = 1f; 
        if (audioSourceMusica != null) audioSourceMusica.UnPause(); 
        if (panelPausa != null) panelPausa.SetActive(false); 
        if (botonPausa != null) botonPausa.SetActive(true); 
    }

    /// <summary>
    /// Interrumpe la partida de manera abrupta, regresando al menú principal
    /// sin almacenar el progreso (puntuación ni gemas) obtenido durante la sesión en curso.
    /// </summary>
    public void SalirSinGuardar()
    {
        Time.timeScale = 0f;
        if (audioSourceMusica != null) audioSourceMusica.UnPause(); 

        // Fuerza el reseteo de la sesión actual para evitar que continúe
        contadorGemas = 0;
        puntuacionObstaculos = 0;
        ActualizarTextoPantalla();
        
        // Destruye explícitamente antes de volver al menú por seguridad extra
        DestruirElementosJuego();

        MostrarMenuPrincipal(); 
    }

    // -----------------------------------------------------------------------------
    // SISTEMA DE INICIO DE SESIÓN DESDE PERFIL
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Ajusta y abre el panel de inicio de sesión cuando se invoca desde el perfil.
    /// </summary>
    public void AbrirInicioSesionDesdePerfil()
    {
        Debug.Log("GameManager: AbrirInicioSesionDesdePerfil ha sido llamado.");
        ConfigurarPanelInicioSesion();
        if (panelInicioSesion != null)
        {
            panelInicioSesion.SetActive(true);
            panelInicioSesion.transform.SetAsLastSibling(); // Para asegurarnos de que se dibuja por encima de los demás paneles
            Debug.Log("GameManager: panelInicioSesion ha sido activado y puesto al frente.");

            if (panelInicioSesion.transform.parent != null && !panelInicioSesion.transform.parent.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("GameManager: ¡ATENCIÓN! El panel padre de panelInicioSesion está INACTIVO. El panel de inicio de sesión no será visible hasta que su padre se active.");
            }
        }
        else
        {
            Debug.LogError("GameManager: panelInicioSesion no está asignado en el Inspector.");
        }
    }

    /// <summary>
    /// Ajusta los textos y elementos del panel de inicio de sesión según si 
    /// han pasado o no 24 horas desde que el usuario abrió la app por primera vez.
    /// </summary>
    private void ConfigurarPanelInicioSesion()
    {
        string fechaString = SecurePrefs.GetString("FechaPrimeraApertura", "");
        bool pasadas24Horas = false;

        if (!string.IsNullOrEmpty(fechaString))
        {
            if (System.DateTime.TryParse(fechaString, null, System.Globalization.DateTimeStyles.RoundtripKind, out System.DateTime fechaPrimeraApertura))
            {
                System.TimeSpan tiempoTranscurrido = System.DateTime.Now - fechaPrimeraApertura;
                if (tiempoTranscurrido.TotalHours > 24)
                {
                    pasadas24Horas = true;
                }
            }
        }

        // Caso 2 y 3: El título cambia a "Inicio de sesion"
        if (txtTituloInicio != null) 
        {
            txtTituloInicio.text = "Inicio de sesion";
        }

        if (pasadas24Horas)
        {
            // Caso 3: Pasadas las 24 horas
            if (txtSubtitulo != null) 
            {
                txtSubtitulo.text = "¡Guarda tu progreso para conocer a más amigos espaciales!";
            }
            
            if (bocadilloCowsmo != null) bocadilloCowsmo.SetActive(false);
            if (textoCowsmo != null) textoCowsmo.SetActive(false);
        }
        else
        {
            // Caso 2: Primeras 24 horas
            if (bocadilloCowsmo != null) bocadilloCowsmo.SetActive(true);
            if (textoCowsmo != null) textoCowsmo.SetActive(true);
        }
    }
}
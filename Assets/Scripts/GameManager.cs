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

    [Header("Interfaz Gráfica (UI)")]
    public GameObject contenedorGemas;
    public TextMeshProUGUI textoGemas;
    public TextMeshProUGUI txtPuntuacionGameplay;
    
    [Header("Textos Pantalla Game Over")]
    public TextMeshProUGUI txtPuntuacionActual_GameOver;
    public TextMeshProUGUI txtMejorPuntuacion_GameOver;

    public GameObject panelGameOver;
    public GameObject panelInicioSesion;
    public GameObject panelMenuPrincipal;
    public GameObject panelPausa;
    public GameObject botonPausa;

    [Header("Música Ambiental")]
    public UnityEngine.Audio.AudioMixerGroup grupoMusica;
    public AudioClip musicaInGame;
    public AudioClip musicaMenu;
    private AudioSource audioSourceMusica;

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

        ColeccionableGema[] gemas = FindObjectsOfType<ColeccionableGema>();
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
        ActualizarTextoPantalla();
        velocidadActual = velocidadInicial;
        temporizador = 0f;

        DestruirElementosJuego();

        ControladorJugagor jugador = FindAnyObjectByType<ControladorJugagor>();
        if (jugador != null)
        {
            jugador.Revivir();
        }

        CambiarMusica(musicaInGame);

        Time.timeScale = 1f;

        AnalyticsManager.Instancia.RegistrarEventoSimple("inicio_juego");
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
        Debug.Log("Gemas totales en la sesión: " + contadorGemas);
        ActualizarTextoPantalla();
    }

    /// <summary>
    /// Incrementa la puntuación obtenida al superar obstáculos y actualiza la interfaz.
    /// </summary>
    public void SumarPunto()
    {
        puntuacionObstaculos++;
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

        if (primeraVez)
        {
            if (panelInicioSesion != null)
            {
                panelInicioSesion.SetActive(true);
            }
            SecurePrefs.SetInt("PrimeraVez", 0);
        }
        else
        {
            if (panelGameOver != null)
            {
                panelGameOver.SetActive(true);

                AnalyticsManager.Instancia.RegistrarEventoSimple("jugador_muere");
                AnalyticsManager.Instancia.RegistrarEventoDificultad(velocidadActual);
            }
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

        ControladorJugagor jugador = FindAnyObjectByType<ControladorJugagor>();
        float xJugador = -7.35f; 
        if (jugador != null)
        {
            jugador.Revivir();
            xJugador = jugador.transform.position.x;
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
        if (audioSourceMusica != null) audioSourceMusica.UnPause(); 
        MostrarMenuPrincipal(); 
    }
}
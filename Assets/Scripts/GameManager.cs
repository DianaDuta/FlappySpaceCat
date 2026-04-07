using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Auth;

/**
* CLASE GAME MANAGER:
* Es único y global del juego.
* Control del juego con el método awake()
* Centraliza los datos del juego
*/
public class GameManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    // Instancia estática
    public static GameManager Instancia;
    public int contadorGemas = 0;
    public int puntuacionObstaculos = 0; // Puntos por atravesar tuberías

    [Header("Ajustes de Dificultad (Velocidad)")]
    public float velocidadActual;
    public float velocidadInicial = 4f; // La velocidad a la que empieza el juego
    public float cantidadAumento = 0.5f; // Cuánto sube la velocidad cada vez
    public float tiempoParaAumentar = 10f; // Cada cuántos segundos sube la dificultad
    
    private float temporizador = 0f; // El cronómetro interno oculto para los cambios de velocidad

    [Header("Interfaz Gráfica (UI)")]
    [Tooltip("Arrastra aquí el objeto padre (Contador_Gemas) completo del Canvas")]
    public GameObject contenedorGemas;

    [Tooltip("Arrastra aquí el texto de las gemas del Canvas")]
    public TextMeshProUGUI textoGemas;

    [Tooltip("Texto para la Puntuación gigante mientras juegas")]
    public TextMeshProUGUI txtPuntuacionGameplay;
    
    [Header("Textos Pantalla Game Over")]
    public TextMeshProUGUI txtPuntuacionActual_GameOver;
    [Tooltip("Texto para la Mejor Puntuación (solo en Game Over)")]
    public TextMeshProUGUI txtMejorPuntuacion_GameOver;

    [Tooltip("Arrastra aquí el Panel_GameOver desde el Canvas")]
    public GameObject panelGameOver;

    [Tooltip("Arrastra aquí el Panel_InicioSesion desde el Canvas")]
    public GameObject panelInicioSesion;

    [Tooltip("Arrastra aquí el Panel_MenuPrincipal desde el Canvas")]
    public GameObject panelMenuPrincipal;

    [Header("Música Ambiental")]
    public AudioClip musicaInGame; // Arrastra tu música In-Game
    public AudioClip musicaMenu;   // Arrastra tu música de Menús/GameOver
    private AudioSource audioSourceMusica;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /*
    * Método Awake():
    * Se encarga de arrancar el juego
    */
    void Awake()
    {
        // Comprobación de que solo existe un game manager en el jego
        if (Instancia == null)
        {
            Instancia = this;
            
            // Crea un AudioSource automáticamente para la música de fondo
            audioSourceMusica = gameObject.AddComponent<AudioSource>();
            audioSourceMusica.loop = true; // Para que la música no acabe nunca
            audioSourceMusica.volume = 0.5f; // Volumen al 50%
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /* Método Start():
    * Arranca el juego mostrando el menú principal en lugar de empezar directamente.
    */
    void Start()
    {
        MostrarMenuPrincipal();
    }

    /*
    * Método CambiarMusica():
    * Reproduce la pista musical que le pasemos si no está sonando ya.
    */
    private void CambiarMusica(AudioClip nuevaMusica)
    {
        // Si no hay reproductor, o no hay música nueva, o YA está sonando esa misma canción, no hace nada
        if (audioSourceMusica == null || nuevaMusica == null || audioSourceMusica.clip == nuevaMusica) return;

        audioSourceMusica.clip = nuevaMusica;
        audioSourceMusica.Play();
    }

    /*
    * Método MostrarMenuPrincipal():
    * Activa el menú principal, oculta game over y pausa el tiempo.
    */
    public void MostrarMenuPrincipal()
    {
        Time.timeScale = 0f; // Pausa el tiempo mientras estamos en el menú
        
        // Pone la música relaja' del menú principal
        CambiarMusica(musicaMenu);

        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelInicioSesion != null) panelInicioSesion.SetActive(false);

        // Ocultar números mientras estamos en el menú
        if (contenedorGemas != null) contenedorGemas.SetActive(false);
        else if (textoGemas != null) textoGemas.gameObject.SetActive(false);
        
        if (txtPuntuacionGameplay != null) txtPuntuacionGameplay.gameObject.SetActive(false);

        // Destruir elementos sobrantes (obstáculos y gemas) de partidas anteriores
        DestruirElementosJuego();

        // Reiniciar el generador maestro para evitar arrastrar métricas de la partida anterior
        GeneradorMaestro genMaestro = FindAnyObjectByType<GeneradorMaestro>();
        if (genMaestro != null) genMaestro.Reiniciar();
    }

    /*
    * Método DestruirElementosJuego:
    * Se encarga de limpiar la escena de objetos instanciados
    */
    private void DestruirElementosJuego()
    {
        // Eliminar TODOS los obstáculos antiguos de la pantalla generados durante la partida
        GameObject[] obstaculos = GameObject.FindGameObjectsWithTag("Obstaculo");
        foreach (GameObject obs in obstaculos)
        {
            // El tag "Obstaculo" lo tienen los hijos (Tuberia Arriba/Abajo), así que miramos el nombre de la raíz (el padre principal)
            Transform raiz = obs.transform.root;
            if (raiz.name.Contains("(Clone)"))
            {
                Destroy(raiz.gameObject);
            }
        }

        // Eliminar TODAS las gemas antiguas de la pantalla.
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

    /*
    * Método IniciarJuego():
    * Se ejecuta al pulsar el botón "Jugar". Oculta el menú, reanuda el tiempo 
    * y resetea las variables a su estado inicial.
    */
    public void IniciarJuego()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);

        // Mostrar números in-game
        if (contenedorGemas != null) contenedorGemas.SetActive(true);
        else if (textoGemas != null) textoGemas.gameObject.SetActive(true);
        
        if (txtPuntuacionGameplay != null) txtPuntuacionGameplay.gameObject.SetActive(true);

        // Reiniciar variables
        contadorGemas = 0;
        puntuacionObstaculos = 0;
        ActualizarTextoPantalla();
        velocidadActual = velocidadInicial;
        temporizador = 0f;

        // Asegurarnos de vaciar todo al empezar la partida por si acaso
        DestruirElementosJuego();

        // Reiniciar posición del jugador
        ControladorJugagor jugador = FindAnyObjectByType<ControladorJugagor>();
        if (jugador != null)
        {
            jugador.Revivir();
        }

        // Pone la música intensa del juego
        CambiarMusica(musicaInGame);

        //Tiempo = velocidad normal
        Time.timeScale = 1f;

        // FUNNEL INICIO JUEGO
        AnalyticsManager.Instancia.RegistrarEventoSimple("inicio_juego");
    }

    /*
    * Método Update():
    * Se ejecuta cada frame.
    * Controla el aumento progresivo de dificultad.
    */
    void Update()
    {
        // Solo cuenta el tiempo si el juego no está en Game Over (timeScale > 0)
        if (Time.timeScale > 0f)
        {
            // Suma el tiempo que ha pasado desde el último frame
            temporizador += Time.deltaTime;

            // Si el cronómetro llega al límite marcado
            if (temporizador >= tiempoParaAumentar)
            {
                // Sube la velocidad
                velocidadActual += cantidadAumento;
                
                // Resetea el cronómetro a 0 para que vuelva a contar
                temporizador = 0f;
                
                Debug.Log("¡Subida de dificultad! Nueva velocidad: " + velocidadActual);
            }
        }
    }

    /*
    * Método SumarGema:
    * @param int cantidad: número de gemas que posee el jugador
    * Se encarga de ir sumando la puntuación que el jugador obtiene al recoger gemas en el juego
    */
    public void SumarGema(int cantidad)
    {
        contadorGemas += cantidad;
        Debug.Log("Gemas totales: " + contadorGemas);
        // Actualiza el texto
        ActualizarTextoPantalla();
    }

    /*
    * Método SumarPunto:
    * Se encarga de sumar puntos al atravesar obstáculos
    */
    public void SumarPunto()
    {
        puntuacionObstaculos++;
        ActualizarTextoPantalla();
    }

    /*
    * Método ActualizarTextoPantalla():
    * Cambia el texto existente en la pantalla.
    */
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

    /*
    * Método ActivarGameOver():
    * Congela el juego, detiene el tiempo y enciende la interfaz de Game Over.
    */
    public void ActivarGameOver()
    {
        // Congela todas las físicas y mvtos del juego
        Time.timeScale = 0f; 
        
        // Ocultar los textos en vivo para que el Game Over se vea limpio
        if (contenedorGemas != null) contenedorGemas.SetActive(false);
        else if (textoGemas != null) textoGemas.gameObject.SetActive(false);
        
        if (txtPuntuacionGameplay != null) txtPuntuacionGameplay.gameObject.SetActive(false);

        // Vuelve la música del menú al morir
        CambiarMusica(musicaMenu);

        // Gemas guardadas de partidas anteriores
        int gemasGuardadas = SecurePrefs.GetInt("GemasLocales", 0);
        
        // Suma las gemas de esta partida a las que ya teníamos
        int totalGemas = gemasGuardadas + contadorGemas;
        SecurePrefs.SetInt("GemasLocales", totalGemas);

        // Manejo del récord con Firebase
        FirebaseUser usuario = FirebaseAuth.DefaultInstance.CurrentUser;
        if (usuario != null)
        {
            // Pide a Firebase la mejor puntuación asíncronamente
            DatabaseManager.Instancia.ObtenerMejorPuntuacion(usuario.UserId, (mejorPuntuacionBD) =>
            {
                // Si la puntuación de esta partida es mayor al récord histórico...
                if (puntuacionObstaculos > mejorPuntuacionBD)
                {
                    mejorPuntuacionBD = puntuacionObstaculos; // ¡Nuevo récord!
                    DatabaseManager.Instancia.GuardarMejorPuntuacionEnNube(usuario.UserId, mejorPuntuacionBD);
                }

                // --- ACTUALIZAR TEXTOS DEL GAME OVER ---
                if (txtPuntuacionActual_GameOver != null) 
                    txtPuntuacionActual_GameOver.text = "Puntuacion actual: " + puntuacionObstaculos.ToString();

                if (txtMejorPuntuacion_GameOver != null) 
                    txtMejorPuntuacion_GameOver.text = "Mejor puntuacion: " + mejorPuntuacionBD.ToString();
            });
        }
        else
        {
            // Opcional: Para gente que juegue como "Anónimo" o sin internet
            int mejorPuntuacion = SecurePrefs.GetInt("MejorPuntuacion", 0);
            if (puntuacionObstaculos > mejorPuntuacion)
            {
                SecurePrefs.SetInt("MejorPuntuacion", puntuacionObstaculos);
                mejorPuntuacion = puntuacionObstaculos;
            }
            // --- ACTUALIZAR TEXTOS DEL GAME OVER ---
            if (txtPuntuacionActual_GameOver != null) 
                txtPuntuacionActual_GameOver.text = "Puntuacion actual: " + puntuacionObstaculos.ToString();

            if (txtMejorPuntuacion_GameOver != null) 
                txtMejorPuntuacion_GameOver.text = "Mejor puntuacion: " + mejorPuntuacion.ToString();
        }
            
        //Guarda todos los cambios
        SecurePrefs.Save();
        

        // Guarda el nuevo total en el disco duro del móvil
        SecurePrefs.Save(); 
        
        Debug.Log("Juego Guardado Localmente. Gemas totales: " + totalGemas);

        // Comprueba si es la primera vez que juega
        bool primeraVez = SecurePrefs.GetInt("PrimeraVez", 1) == 1;

        if (primeraVez)
        {
            // Si es la primera vez, activa el panel de tutorial
            if (panelInicioSesion != null)
            {
                panelInicioSesion.SetActive(true);
            }
            // Marca que ya no es la primera vez
            SecurePrefs.SetInt("PrimeraVez", 0);
        }
        else
        {
            // Si no es la primera vez, aparece el panel de GameOver
            if (panelGameOver != null)
            {
                panelGameOver.SetActive(true);

                //FUNNEL RETENCIÓN Y FUNNEL DIFICULTAD
                // Registra la muerte para el Funnel de Retención
                AnalyticsManager.Instancia.RegistrarEventoSimple("jugador_muere");

                // Registra la velocidad exacta de la muerte para el Funnel de Dificultad
                AnalyticsManager.Instancia.RegistrarEventoDificultad(velocidadActual);
            }
        }
            
        //Guarda todos los cambios
        SecurePrefs.Save();
        

        // Guarda el nuevo total en el disco duro del móvil
        SecurePrefs.Save(); 
        
        Debug.Log("Juego Guardado Localmente. Gemas totales: " + totalGemas);
    }

    /*
    * Método ContinuarPartida():
    * Se llama después de ver el anuncio del botón "Continuar". Oculta la pantalla de derrota,
    * limpia los obstáculos cercanos y reanuda el tiempo de juego.
    */
    public void ContinuarPartida()
    {
        // Oculta el panel de Game Over
        if (panelGameOver != null) panelGameOver.SetActive(false);

        // Volver a encender los textos
        if (contenedorGemas != null) contenedorGemas.SetActive(true);
        else if (textoGemas != null) textoGemas.gameObject.SetActive(true);
        
        if (txtPuntuacionGameplay != null) txtPuntuacionGameplay.gameObject.SetActive(true);

        // Busca al jugador y lo devuelve a su posición original, reviviéndolo
        ControladorJugagor jugador = FindAnyObjectByType<ControladorJugagor>();
        float xJugador = -7.35f; // Posición X por defecto
        if (jugador != null)
        {
            jugador.Revivir();
            xJugador = jugador.transform.position.x;
        }

        // DESTRUIR EL OBSTÁCULO DEL CHOQUE Y EL SIGUIENTE
        GameObject[] obstaculos = GameObject.FindGameObjectsWithTag("Obstaculo");
        // Ordenamos los obstáculos de izquierda a derecha (por su posición en X)
        System.Array.Sort(obstaculos, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        int obstáculosDestruidos = 0;
        foreach (GameObject obs in obstaculos)
        {
            // Buscamos los obstáculos que estén interactuando con el jugador o justo a su derecha
            if (obs.transform.position.x >= xJugador - 2f) 
            {
                Destroy(obs);
                obstáculosDestruidos++;
                // Rompemos el bucle al haber destruido 2
                if (obstáculosDestruidos >= 2) break; 
            }
        }

        // Vuelve a la música intensa al revivir (continuar)
        CambiarMusica(musicaInGame);

        // Pone el tiempo a su velocidad normal
        Time.timeScale = 1f;

        Debug.Log("¡Partida reanudada tras el anuncio! Obstáculos cercanos despejados.");
    }

    /*
    * Método VolverMenuPrincipal():
    * Guarda las gemas en la nube si hay usuario logueado, borra localmente
    * y devuelve al jugador a la pantalla de inicio principal.
    */
    public void VolverMenuPrincipal()
    {
        // 1. Guardar en Firebase y borrar memoria local si está logueado
        FirebaseUser usuario = FirebaseAuth.DefaultInstance.CurrentUser;
        if (usuario != null)
        {
            int gemasLocales = SecurePrefs.GetInt("GemasLocales", 0);
            if (gemasLocales > 0 && DatabaseManager.Instancia != null)
            {
                DatabaseManager.Instancia.GuardarGemasEnNube(usuario.UserId, gemasLocales);
            }
            
            // Borrar de la memoria local para la siguiente partida desde 0
            SecurePrefs.SetInt("GemasLocales", 0);
            SecurePrefs.Save();
            Debug.Log("Volviendo al menú: Datos guardados en la nube y memoria local borrada.");
        }

        // 2. Volvemos al menú sin destrozar la memoria
        MostrarMenuPrincipal();
    }
}
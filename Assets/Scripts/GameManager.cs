using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
    //de momento 0, porteriormente se introducirá de un JSON
    public int contadorGemas = 0;

    [Header("Ajustes de Dificultad (Velocidad)")]
    public float velocidadActual;
    public float velocidadInicial = 4f; // La velocidad a la que empieza el juego
    public float cantidadAumento = 0.5f; // Cuánto sube la velocidad cada vez
    public float tiempoParaAumentar = 10f; // Cada cuántos segundos sube la dificultad
    
    private float temporizador = 0f; // El cronómetro interno oculto para los cambios de velocidad

    [Header("Interfaz Gráfica (UI)")]
    [Tooltip("Arrastra aquí el texto de las gemas del Canvas")]
    public TextMeshProUGUI textoGemas;

    [Tooltip("Arrastra aquí el Panel_GameOver desde el Canvas")]
    public GameObject panelGameOver;

    [Tooltip("Arrastra aquí el Panel_InicioSesion desde el Canvas")]
    public GameObject panelInicioSesion;

    [Tooltip("Arrastra aquí el Panel_MenuPrincipal desde el Canvas")]
    public GameObject panelMenuPrincipal;

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
    * Método MostrarMenuPrincipal():
    * Activa el menú principal, oculta game over y pausa el tiempo.
    */
    public void MostrarMenuPrincipal()
    {
        Time.timeScale = 0f; // Pausa el tiempo mientras estamos en el menú
        
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelInicioSesion != null) panelInicioSesion.SetActive(false);
    }

    /*
    * Método IniciarJuego():
    * Se ejecuta al pulsar el botón "Jugar". Oculta el menú, reanuda el tiempo 
    * y resetea las variables a su estado inicial.
    */
    public void IniciarJuego()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);

        // Reiniciar variables
        contadorGemas = 0;
        ActualizarTextoPantalla();
        velocidadActual = velocidadInicial;
        temporizador = 0f;

        // Reiniciar posición del jugador
        ControladorJugagor jugador = FindAnyObjectByType<ControladorJugagor>();
        if (jugador != null)
        {
            jugador.Revivir();
        }

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
    * Método ActualizarTextoPantalla():
    * Cambia el texto existente en la pantalla.
    */
    private void ActualizarTextoPantalla()
    {
        if (textoGemas != null) 
        {
            textoGemas.text = ": " + contadorGemas; 
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

        // Gemas guardadas de partidas anteriores
        int gemasGuardadas = PlayerPrefs.GetInt("GemasLocales", 0);
        
        // Suma las gemas de esta partida a las que ya teníamos
        int totalGemas = gemasGuardadas + contadorGemas;
        PlayerPrefs.SetInt("GemasLocales", totalGemas);

        // Comprueba si es la primera vez que juega
        bool primeraVez = PlayerPrefs.GetInt("PrimeraVez", 1) == 1;

        if (primeraVez)
        {
            // Si es la primera vez, activa el panel de tutorial
            if (panelInicioSesion != null)
            {
                panelInicioSesion.SetActive(true);
            }
            // Marca que ya no es la primera vez
            PlayerPrefs.SetInt("PrimeraVez", 0);
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
        PlayerPrefs.Save();
        

        // Guarda el nuevo total en el disco duro del móvil
        PlayerPrefs.Save(); 
        
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

        // Pone el tiempo a su velocidad normal
        Time.timeScale = 1f;

        Debug.Log("¡Partida reanudada tras el anuncio! Obstáculos cercanos despejados.");
    }

    /*
    * Método VolverMenuPrincipal():
    * Al pulsar el botón (por ejemplo Rendirse), volvemos a mostrar el menú principal.
    */
    public void VolverMenuPrincipal()
    {
        MostrarMenuPrincipal();
    }
}
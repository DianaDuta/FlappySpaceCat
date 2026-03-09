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
    * Obligamos al texto a mostrar un "0" al comienzo de la partida.
    */
    void Start()
    {
        //Tiempo = velocidad normal
        Time.timeScale = 1f;
        ActualizarTextoPantalla();

        velocidadActual = velocidadInicial; // Inicia con la velocidad base
        temporizador = 0f; // Reinicia el cronómetro
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
            }
        }
            
        //Guarda todos los cambios
        PlayerPrefs.Save();
        

        // Guarda el nuevo total en el disco duro del móvil
        PlayerPrefs.Save(); 
        
        Debug.Log("Juego Guardado Localmente. Gemas totales: " + totalGemas);
    }

    /*
    * Método VolverMenuPrincipal():
    * Al pulsar el botón, vuelve a la pantalla de inicio del juego.
    * Debe estar descongelado el tiempo para animaciones y efectos visuales.
    */
    public void VolverMenuPrincipal()
    {
        Time.timeScale = 1f; // Descongela el tiempo
        SceneManager.LoadScene("MenuPrincipal");
    }
}
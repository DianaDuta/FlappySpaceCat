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

    [Header("Interfaz Gráfica (UI)")]
    [Tooltip("Arrastra aquí el texto de las gemas del Canvas")]
    public TextMeshProUGUI textoGemas;

    [Tooltip("Arrastra aquí el Panel_GameOver desde el Canvas")]
    public GameObject panelGameOver;

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
        
        // Enciende el panel de Game Over (versión corta: panelGameOver?.SetActive(true))
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
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
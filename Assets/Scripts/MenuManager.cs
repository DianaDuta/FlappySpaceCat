using UnityEngine;
using UnityEngine.SceneManagement; // Para viajar entre escenas

/**
* CLASE MENU MANAGER:
* Controla la lógica y los botones de la pantalla de inicio.
*/
public class MenuManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /*
    * Método CargarJuego():
    * Se ejecuta al pulsar el botón de "JUGAR".
    * Carga la escena de juego.
    */
    public void CargarJuego()
    {
        // Asegura que el tiempo esté en marcha
        Time.timeScale = 1f; 
        
        // Carga la escena "Juego"
        SceneManager.LoadScene("Juego"); 
    }
}
using UnityEngine;
using UnityEngine.SceneManagement; 

/// <summary>
/// Administra la navegación y las interacciones del menú principal de la aplicación,
/// sirviendo como puente entre las distintas interfaces (tienda, juego, configuración).
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Managers")]
    public TiendaSkinsManager tiendaManager;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Restablece la escala de tiempo a su valor normal y carga la escena principal de juego.
    /// Invocado por los disparadores de inicio de partida en la UI.
    /// </summary>
    public void CargarJuego()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Juego"); 
    }

    /// <summary>
    /// Despliega la interfaz de la tienda de aspectos a través de su gestor dedicado.
    /// </summary>
    public void AbrirTienda()
    {
        if (tiendaManager != null)
        {
            tiendaManager.AbrirTienda();
        }
        else
        {
            Debug.LogWarning("Fallo de referencia: TiendaSkinsManager no asignado.");
        }
    }
}
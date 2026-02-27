using UnityEngine;

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

    /*
    * Método SumarGema:
    * @param int cantidad: número de gemas que posee el jugador
    * Se encarga de ir sumando la puntuación que el jugador obtiene al recoger gemas en el juego
    */
    public void SumarGema(int cantidad)
    {
        contadorGemas += cantidad;
        Debug.Log("Gemas totales: " + contadorGemas);
    }
}
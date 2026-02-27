using UnityEngine;
/*
* CLASE COLECCIONABLE GEMA:
* Se encarga de establecer el valor de la gema, identificar si ha colisionado con el jugador.
* Suma los puntos y la destruye tras la colisión.
*/
public class ColeccionableGema : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    [Tooltip("Puntos que da esta gema al recogerla")]
    public int valorGema = 1;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------
    /*
    * MÉTODO ONTRIGGERENTER2D:
    * Se dispara de forma automática cuando un jugador atraviesa la gema.
    * Primero identifica que sea el jugador, luego suma los puntos y la destruye.
    */
    private void OnTriggerEnter2D(Collider2D colision)
    {
        // Comprobar si es el jugador
        if (colision.CompareTag("Player"))
        {
            // Suma los puntos
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.SumarGema(valorGema);
            }

            // Destruye la gema
            Destroy(gameObject);
        }
    }
}
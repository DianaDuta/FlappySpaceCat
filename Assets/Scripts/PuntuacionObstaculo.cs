using UnityEngine;

/// <summary>
/// Actúa como un volumen invisible (Trigger) entre los obstáculos que detecta 
/// cuando el jugador los atraviesa satisfactoriamente para contabilizar la puntuación.
/// </summary>
public class PuntuacionObstaculo : MonoBehaviour
{
    private bool puntuado = false; 

    /// <summary>
    /// Detecta la intrusión del jugador en el volumen y aumenta el contador de puntos global,
    /// asegurando que el evento se dispare únicamente una vez por instancia.
    /// </summary>
    /// <param name="collision">Datos del objeto que irrumpió en el trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!puntuado && collision.CompareTag("Player"))
        {
            puntuado = true;
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.SumarPunto();
            }
        }
    }
}

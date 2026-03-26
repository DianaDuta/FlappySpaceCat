using UnityEngine;

/*
* CLASE PUNTUACIÓN OBSTÁCULO:
* Script que se añade a un BoxCollider2D (Trigger) invisible
* colocado en el hueco entre el obstáculo superior y el inferior.
* Detecta si el jugador lo ha atravesado para sumar un punto.
*/
public class PuntuacionObstaculo : MonoBehaviour
{
    private bool puntuado = false; // Evita que un mismo obstáculo sume 2 veces

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se asegura de que quien atraviesa es el jugador y que no haya puntuado ya
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

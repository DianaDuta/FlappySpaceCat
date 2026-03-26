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

    [Header("Efectos de Sonido")]
    public AudioClip sonidoRecoger; // Arrastra aquí el sonido de la gema
    [Range(0f, 1f)]
    public float volumenSonido = 1f; // Controla lo fuerte que suena (1 es el máximo)

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

            // Reproducir sonido antes de destruir el objeto
            // Truco para juegos 2D: lo reproducimos en la posición de la cámara, porque si no Unity 
            // le aplica sonido 3D y se escucha muy lejos (bajito).
            if (sonidoRecoger != null)
            {
                Vector3 posicionCamara = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(sonidoRecoger, posicionCamara, volumenSonido);
            }

            // Destruye la gema
            Destroy(gameObject);
        }
    }
}
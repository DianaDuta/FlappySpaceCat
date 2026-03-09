using UnityEngine;
/*
* Controla el movimiento del fondo en el juego.
* El fondo se mueve hacia la izquierda a una velocidad que es una fracción de la velocidad del juego para crear un efecto de parallax (profundidad).
* Cuando el fondo se ha movido completamente hacia la izquierda, se teletransporta a la derecha para crear un efecto de fondo infinito.
* El factor de parallax se puede ajustar desde el inspector para crear diferentes capas de fondo con diferentes velocidades.
* Lee la velocidad progresiva en tiempo real desde el GameManager.
*/
public class ControladorFondo : MonoBehaviour
{
    //--------------------------------
    // CAMPOS
    //--------------------------------
    /* Factor de Parallax: 
    * 0 = no se mueve
    * 1 = se mueve igual que los obstaculos
    * 0.1 = fondo muy lejano (lento)
    * 0.5 = fondo medio
    */
    [Range(0f, 1f)]
    public float efectoParallax = 0.5f;

    public float anchoImagen; // Cuánto mide la imagen de largo

    //--------------------------------
    // MÉTODOS
    //--------------------------------
    /* Método Start, se obtiene el ancho de la imagen del fondo a partir del SpriteRenderer si no se ha definido manualmente.
    * Esto permite que el script funcione con cualquier imagen de fondo sin necesidad de ajustar el ancho en el inspector.
    */
    void Start()
    {
        // Si no define el ancho manualmente, se calcula solo
        if (anchoImagen <= 0)
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                anchoImagen = sprite.bounds.size.x;
            }
        }
    }

    /* Método Update, se obtiene la velocidad del juego desde el GameManager y se calcula la velocidad real del fondo multiplicando la velocidad del juego por el factor de parallax.
    * Luego se mueve el fondo hacia la izquierda utilizando transform.Translate.
    * Si el fondo se ha movido completamente hacia la izquierda (su posición es menor que -ancho), se teletransporta a la derecha sumando 2 veces el ancho a su posición X.
    * Esto crea un efecto de fondo infinito sin necesidad de tener múltiples imágenes.
    */
    void Update()
    {
        /* Obtiene la velocidad base del juego del JSON, si no, usa 3 por defecto.*/
        float velocidadBase = 3f;
        if (GameManager.Instancia != null)
        {
            velocidadBase = GameManager.Instancia.velocidadActual;
        }

        /*Calcula la velocidad real del fondo multiplicando la velocidad del juego por el factor de parallax.
        * Ejemplo: Si el juego va a 3 y parallax es 0.1, el fondo se mueve a 0.3
        */
        float velocidadReal = velocidadBase * efectoParallax;
        transform.Translate(Vector3.left * velocidadReal * Time.deltaTime);

        /* TELETRANSPORTAR (Efecto Infinito)
        * Si el fondo se ha movido completamente hacia la izquierda (su posición es menor que -ancho)
        * Lo movemos 2 veces el ancho hacia la derecha para ponerlo a la cola
        * Esto crea un efecto de fondo infinito sin necesidad de tener múltiples imágenes.
        */
        if (transform.position.x <= -anchoImagen)
        {
            Vector3 nuevaPos = transform.position;
            nuevaPos.x += 2 * anchoImagen; 
            transform.position = nuevaPos;
        }
    }
}
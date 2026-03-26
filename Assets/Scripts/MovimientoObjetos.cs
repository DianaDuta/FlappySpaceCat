using UnityEngine;

/*
* Controla el movimiento de los obstáculos en el juego.
* Los obstáculos se mueven hacia la izquierda a una velocidad progresiva en tiempo real desde el GameManager.
* Cuando un obstáculo se pasa del límite izquierdo de la pantalla, se destruye automáticamente para liberar memoria.
*/
public class MovimientoObjetos : MonoBehaviour
{
    //--------------------------------
    // CAMPOS
    //--------------------------------
    // Límite X donde el objeto se destruye. 
    // Jugador está en -7.35, lo pone en -12  para asegurar que salga totalmente de la pantalla.
    public float limiteIzquierda = -12f; 

    private bool haPuntuado = false;
    private bool esObstaculoPuntuable = false;
    private ControladorJugagor jugador;

    void Start()
    {
        jugador = FindAnyObjectByType<ControladorJugagor>();
        // Solo comprobamos la puntuación si este objeto es un Obstáculo (y no una Gema u otro adorno)
        if (gameObject.name.Contains("Obstaculo"))
        {
            esObstaculoPuntuable = true;
        }
    }

    //--------------------------------
    // MÉTODOS
    //--------------------------------
    /*
    * Método Update, se obtiene la velocidad del juego desde el JSON y se mueve el obstáculo hacia la izquierda.
    * Si el obstáculo se pasa del límite izquierdo, se destruye para liberar memoria.
    */
    void Update()
    {
        // OBTENER VELOCIDAD
        // Lee la velocidad del JSON. Si no existe, usa 3 por defecto.
        float velocidad = 3f;
        if (GameManager.Instancia != null)
        {
            velocidad = GameManager.Instancia.velocidadActual;
        }

        // MUEVE HACIA LA IZQUIERDA: Multiplica por Time.deltaTime para que el movimiento sea suave y constante
        transform.Translate(Vector3.left * velocidad * Time.deltaTime, Space.World);

        // PUNTUACIÓN AUTOMÁTICA POR POSICIÓN (Sin necesidad de colliders)
        if (esObstaculoPuntuable && !haPuntuado && jugador != null)
        {
            // Si la coordenada X del obstáculo ha sobrepasado la del jugador... ¡Punto!
            if (transform.position.x < jugador.transform.position.x)
            {
                haPuntuado = true;
                if (GameManager.Instancia != null) GameManager.Instancia.SumarPunto();
            }
        }

        // AUTODESTRUCCIÓN: Si el objeto se pasa del límite izquierdo, se borra de la memoria
        if (transform.position.x < limiteIzquierda)
        {
            Destroy(gameObject);
        }
    }
}
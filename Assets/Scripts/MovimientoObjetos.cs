using UnityEngine;

/*
* Controla el movimiento de los obstáculos en el juego.
* Los obstáculos se mueven hacia la izquierda a una velocidad que se puede configurar desde un archivo JSON.
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
        if (LectorConfiguracion.Datos != null)
        {
            velocidad = LectorConfiguracion.Datos.velocidadJuego;
        }

        // MUEVE HACIA LA IZQUIERDA: Multiplica por Time.deltaTime para que el movimiento sea suave y constante
        transform.Translate(Vector3.left * velocidad * Time.deltaTime, Space.World);

        // AUTODESTRUCCIÓN: Si el objeto se pasa del límite izquierdo, se borra de la memoria
        if (transform.position.x < limiteIzquierda)
        {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

/*
* Controla el comportamiento del jugador jugador en el juego.
* Permite al jugador saltar al hacer clic, y detecta colisiones con obstáculos o el suelo para finalizar el juego.
* La gravedad del jugador se ajusta dinámicamente según la configuración cargada desde el JSON.
*/
public class ControladorJugagor : MonoBehaviour
{
    //------------------------------------
    // CAMPOS
    //------------------------------------
    private Rigidbody2D rb;
    public float fuerzaSalto = 5f; // Fuerza del impulso hacia arriba
    private bool estaVivo = true;
    private Vector3 posicionOriginal;
    // Límites de pantalla (Ajustables)
    public float limiteArriba = 4.5f; 
    public float limiteAbajo = -4.5f;

    //------------------------------------
    // MÉTODOS
    //------------------------------------
    /*
    * Método Start, se obtiene el componente Rigidbody2D del jugador y se ajusta la gravedad según la configuración cargada desde el JSON.
    * Esto permite que la gravedad del jugador sea configurable sin necesidad de modificar el código.
    */
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        posicionOriginal = transform.position;

        // La gravedad viene del JSON.
        if (LectorConfiguracion.Datos != null)
        {
            rb.gravityScale = LectorConfiguracion.Datos.gravedadJugador;
        }
    }

    /*
    * Método Update, se verifica si el jugador hace clic y si sigue vivo.
    * Si es así, se resetea la velocidad vertical del jugador y se aplica un impulso hacia arriba para simular el salto.
    * Jugador controla el movimiento de manera sencilla y responsiva.
    * Se limita la posición del jugador para que no pueda salir de la pantalla.
    * Se corrige la velocidad si toca el techo para evitar que se quede pegado.
    */
    void Update()
    {
        // Salto si clickean y el jugador sigue vivo
        if (Input.GetMouseButtonDown(0) && estaVivo)
        {
            rb.linearVelocity = Vector2.up * fuerzaSalto;       // NOTA: 'linearVelocity' = 'velocity'.
        }

        // LIMITAR POSICIÓN EN PANTALLA
        // Posición actual del jugador en una variable temporal
        Vector3 posicion = transform.position;  
        // Clamp obliga a la Y a quedarse entre el mínimo y el máximo
        posicion.y = Mathf.Clamp(posicion.y, limiteAbajo, limiteArriba);        
        // Aplicamos la posición corregida al gato
        transform.position = posicion;

        // Si toca el techo, le quita la velocidad hacia arriba para que no se quede pegado
        if (transform.position.y >= limiteArriba && rb.linearVelocity.y > 0)
        {
             rb.linearVelocity = new Vector2(0, 0);
        }
    }

    /*
    * Método OnCollisionEnter2D:
    * @param Collision2D colision: Objeto contra el que chocamos físicamente.
    * Se ejecuta automáticamente cuando el jugador colisiona con otro objeto.
    * Se detienen el tiempo y las físicas del juego y se muestra la pantalla de "Game Over".
    */
    void OnCollisionEnter2D(Collision2D colision)
    {
        if (colision.gameObject.CompareTag("Obstaculo"))
        {
            estaVivo = false;
            //Abre la pantalla de Game Over
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.ActivarGameOver();
            }
        }        
    }

    /*
    * Método Revivir():
    * Restablece al jugador a su posición original, reviviéndolo y deteniendo su inercia.
    */
    public void Revivir()
    {
        estaVivo = true;
        transform.position = posicionOriginal;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
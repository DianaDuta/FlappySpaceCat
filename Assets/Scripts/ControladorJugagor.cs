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
    private Animator animator;
    public float fuerzaSalto = 5f; // Fuerza del impulso hacia arriba
    private bool estaVivo = true;
    private Vector3 posicionOriginal;
    
    [Header("Efectos de Sonido")]
    public AudioClip sonidoSalto; //sonido del maullido
    public AudioClip sonidoMuerte; // sonido de Game Over o muerte
    private AudioSource audioSource;

    [Header("Cara al morir")]
    public SpriteRenderer faceRenderer; // objeto Face 
    public Sprite caraMuerte;           //sprite Face-hurt
    private Sprite caraOriginal;        // Se guarda automáticamente al iniciar

    // Límites de pantalla (Ajustables)
    public float limiteArriba = 4.5f; 
    public float limiteAbajo = -6.5f; // Ajustado para que muera al desaparecer de la pantalla 

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
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        // Hacer que el jugador sea responsive (siempre al 5% de la pantalla desde la izquierda)
        if (Camera.main != null)
        {
            float distanciaZ = transform.position.z - Camera.main.transform.position.z;
            Vector3 posicionViewport = new Vector3(0.05f, 0f, distanciaZ);
            Vector3 posicionMundo = Camera.main.ViewportToWorldPoint(posicionViewport);
            
            // Ajustamos solo la posición X, manteniendo Y y Z originales
            transform.position = new Vector3(posicionMundo.x, transform.position.y, transform.position.z);
        }

        // Guardamos la nueva posición anclada como la original para cuando reviva
        posicionOriginal = transform.position;

        // La gravedad viene del JSON.
        if (LectorConfiguracion.Datos != null)
        {
            rb.gravityScale = LectorConfiguracion.Datos.gravedadJugador;
        }

        // Guardamos la cara normal con la que empieza
        if (faceRenderer != null)
        {
            caraOriginal = faceRenderer.sprite;
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
        // Salto si clickean, el jugador sigue vivo Y el juego no está pausado (menús)
        if (Input.GetMouseButtonDown(0) && estaVivo && Time.timeScale > 0f)
        {
            rb.linearVelocity = Vector2.up * fuerzaSalto;       // NOTA: 'linearVelocity' = 'velocity'.
            
            if (animator != null)
            {
                animator.Play("Player_Salto");
            }

            // Reproducir sonido de salto
            if (audioSource != null && sonidoSalto != null)
            {
                audioSource.PlayOneShot(sonidoSalto);
            }
        }

        // --- SISTEMA DE ANIMACIÓN AL CAER ---
        if (estaVivo && animator != null && rb.linearVelocity.y < 0)
        {
            // Solo reproducimos 'Player_Normal' si no está sonando ya, para no reiniciarla cada frame
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Normal"))
            {
                animator.Play("Player_Normal");
            }
        }

        // LIMITAR POSICIÓN EN PANTALLA Y MUERTE POR CAÍDA
        // Posición actual del jugador en una variable temporal
        Vector3 posicion = transform.position;  
        
        // Si el jugador cae por debajo del límite, muere
        if (posicion.y <= limiteAbajo && estaVivo)
        {
            estaVivo = false;
            
            if (animator != null)
            {
                animator.enabled = false; // Congela la pose en la que estaba (salto o caída)
            }
            if (faceRenderer != null && caraMuerte != null)
            {
                faceRenderer.sprite = caraMuerte; // Le pone la cara triste
            }
            
            // Reproducir sonido al morir por caída
            if (audioSource != null && sonidoMuerte != null)
            {
                audioSource.PlayOneShot(sonidoMuerte);
            }

            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.ActivarGameOver();
            }
        }

        // Clamp obliga a la Y a no superar el techo, pero ahora permitimos que caiga para morir
        posicion.y = Mathf.Min(posicion.y, limiteArriba);        
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
        if (colision.gameObject.CompareTag("Obstaculo") && estaVivo)
        {
            estaVivo = false;

            if (animator != null)
            {
                animator.enabled = false; // Congela la pose en la que estaba (salto o caída)
            }
            if (faceRenderer != null && caraMuerte != null)
            {
                faceRenderer.sprite = caraMuerte; // Le pone la cara triste
            }

            // Reproducir sonido al chocar
            if (audioSource != null && sonidoMuerte != null)
            {
                audioSource.PlayOneShot(sonidoMuerte);
            }

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
        
        // Restablece la posición y la rotación original (recta)
        transform.position = posicionOriginal;
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            // Frena en seco cualquier inercia de movimiento o de giro por el golpe
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.enabled = true; // Vuelve a encender las animaciones
            animator.Play("Player_Normal");
        }
        if (faceRenderer != null && caraOriginal != null)
        {
            faceRenderer.sprite = caraOriginal; // Le devuelve la cara feliz/normal
        }
    }
}
using UnityEngine;

/// <summary>
/// Controla el comportamiento físico y las animaciones del personaje principal (jugador).
/// Gestiona la detección de entradas (saltos) y las colisiones letales con obstáculos o límites del entorno.
/// </summary>
public class ControladorJugagor : MonoBehaviour
{
    //------------------------------------
    // CAMPOS
    //------------------------------------
    private Rigidbody2D rb;
    private Animator animator;
    public float fuerzaSalto = 5f; 
    private bool estaVivo = true;
    private Vector3 posicionOriginal;
    

    [Header("Efectos de Sonido")]
    public AudioClip sonidoSalto; 
    public AudioClip sonidoMuerte; 
    private AudioSource audioSource;

    [Header("Cara al morir")]
    public SpriteRenderer faceRenderer;  
    public Sprite caraMuerte;           
    private Sprite caraOriginal;        

    [Header("Posicionamiento Responsivo")]
    [Tooltip("Nombre del GameObject en la escena que define el punto de generación absoluto.")]
    public string nombrePuntoAparicion = "Punto_Aparicion_Jugador";

    [Header("Límites de Vuelo")]
    public float limiteArriba = 4.5f; 
    public float limiteAbajo = -6.5f; 

    //------------------------------------
    // MÉTODOS
    //------------------------------------
    
    /// <summary>
    /// Se inicializan las referencias de componentes, se reposiciona el jugador de manera responsiva 
    /// en la pantalla y se configura su gravedad dinámica desde el archivo de configuración.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        Vector3 posInicial = transform.position;

        // Búsqueda automática del punto de generación por nombre en la escena activa
        GameObject puntoReferencia = GameObject.Find(nombrePuntoAparicion);

        if (puntoReferencia != null)
        {
            // Se fuerza al punto de referencia a actualizar su posición responsiva
            // para asegurar que las coordenadas estén calculadas en base a la pantalla real de Start().
            PosicionadorPorViewport posicionador = puntoReferencia.GetComponent<PosicionadorPorViewport>();
            if (posicionador != null)
            {
                posicionador.ActualizarPosicion();
            }

            // Asignación de coordenadas estáticas a partir de la referencia de generación encontrada.
            transform.position = new Vector3(puntoReferencia.transform.position.x, puntoReferencia.transform.position.y, transform.position.z);
        }
        else
        {
            Debug.LogWarning("⚠️ [DEBUG_PLAYER] No se encontró el objeto de referencia: " + nombrePuntoAparicion + ". El personaje iniciará en su posición de diseño por defecto.");
        }

        // 2. Calculamos el límite inferior de muerte en base al borde de la cámara
        if (Camera.main != null)
        {
            float distanciaZ = transform.position.z - Camera.main.transform.position.z;
            Vector3 limiteInferiorViewport = new Vector3(0f, 0f, distanciaZ);
            limiteAbajo = Camera.main.ViewportToWorldPoint(limiteInferiorViewport).y - 0.5f;
        }

        posicionOriginal = transform.position;

        Debug.Log("[DEBUG_PLAYER] Player Start. GameObject: " + gameObject.name + 
                  ", Posicion Inicial: " + posInicial + 
                  ", Posicion Final: " + transform.position);

        if (LectorConfiguracion.Datos != null)
        {
            rb.gravityScale = LectorConfiguracion.Datos.gravedadJugador;
        }

        if (faceRenderer != null)
        {
            caraOriginal = faceRenderer.sprite;
        }
    }

    /// <summary>
    /// Se procesan las entradas de usuario en cada fotograma. 
    /// Gestiona el impulso vertical (salto), las transiciones de animación y limita 
    /// la posición del jugador dentro del área de juego permitida.
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && estaVivo && Time.timeScale > 0f)
        {
            rb.linearVelocity = Vector2.up * fuerzaSalto;       
            
            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }

            if (audioSource != null && sonidoSalto != null)
            {
                audioSource.PlayOneShot(sonidoSalto);
            }
        }

        if (estaVivo && animator != null)
        {
            // Se aplica un umbral de tolerancia (-0.5f) para evitar que las fluctuaciones físicas
            // o colisiones con el suelo en el primer fotograma cancelen instantáneamente la animación de salto.
            bool cayendo = rb.linearVelocity.y < -0.5f;
            animator.SetBool("IsFalling", cayendo);

            if (cayendo)
            {
                // Limpia cualquier trigger de salto acumulado (doble clic) para evitar que 
                // se reproduzca un salto fantasma al volver al estado normal.
                animator.ResetTrigger("Jump");
            }
        }

        Vector3 posicion = transform.position;  
        
        if (posicion.y <= limiteAbajo && estaVivo)
        {
            estaVivo = false;
            
            if (animator != null)
            {
                animator.enabled = false; 
            }
            if (faceRenderer != null && caraMuerte != null)
            {
                faceRenderer.sprite = caraMuerte; 
            }
            
            if (audioSource != null && sonidoMuerte != null)
            {
                audioSource.PlayOneShot(sonidoMuerte);
            }

            OpcionesManager.VibrarSiEstaActivado();

            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.ActivarGameOver();
            }
        }

        posicion.y = Mathf.Min(posicion.y, limiteArriba);        
        transform.position = posicion;

        if (transform.position.y >= limiteArriba && rb.linearVelocity.y > 0)
        {
             rb.linearVelocity = new Vector2(0, 0);
        }
    }

    /// <summary>
    /// Detecta las colisiones con objetos etiquetados como obstáculos y desencadena
    /// el final de la partida (Game Over).
    /// </summary>
    /// <param name="colision">Los datos de la colisión capturada por el motor físico.</param>
    void OnCollisionEnter2D(Collision2D colision)
    {
        // En este tipo de juego, tocar cualquier objeto físico (Obstáculo o Suelo) es letal.
        if (estaVivo)
        {
            estaVivo = false;

            if (animator != null)
            {
                animator.enabled = false; 
            }
            if (faceRenderer != null && caraMuerte != null)
            {
                faceRenderer.sprite = caraMuerte; 
            }

            if (audioSource != null && sonidoMuerte != null)
            {
                audioSource.PlayOneShot(sonidoMuerte);
            }

            OpcionesManager.VibrarSiEstaActivado();

            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.ActivarGameOver();
            }
        }        
    }

    /// <summary>
    /// Restablece el estado físico, visual y posicional del jugador a sus valores originales.
    /// Empleado al utilizar mecánicas de resurrección o al reiniciar niveles.
    /// </summary>
    public void Revivir()
    {
        estaVivo = true;
        
        transform.position = posicionOriginal;
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.enabled = true; 
            animator.SetBool("IsFalling", false);
            animator.Rebind(); // Reinicia el Animator a su estado por defecto
        }
        if (faceRenderer != null && caraOriginal != null)
        {
            faceRenderer.sprite = caraOriginal; 
        }
    }
}
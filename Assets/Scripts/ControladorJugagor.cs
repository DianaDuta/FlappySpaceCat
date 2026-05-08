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

        if (Camera.main != null)
        {
            float distanciaZ = transform.position.z - Camera.main.transform.position.z;
            Vector3 posicionViewport = new Vector3(0.05f, 0f, distanciaZ);
            Vector3 posicionMundo = Camera.main.ViewportToWorldPoint(posicionViewport);
            
            transform.position = new Vector3(posicionMundo.x, transform.position.y, transform.position.z);
        }

        posicionOriginal = transform.position;

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
                animator.Play("Player_Salto");
            }

            if (audioSource != null && sonidoSalto != null)
            {
                audioSource.PlayOneShot(sonidoSalto);
            }
        }

        if (estaVivo && animator != null && rb.linearVelocity.y < 0)
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Player_Normal"))
            {
                animator.Play("Player_Normal");
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
        if (colision.gameObject.CompareTag("Obstaculo") && estaVivo)
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
            animator.Play("Player_Normal");
        }
        if (faceRenderer != null && caraOriginal != null)
        {
            faceRenderer.sprite = caraOriginal; 
        }
    }
}
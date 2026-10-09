using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
    private float tiempoBloqueoSalto = 0f;
    

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
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (audioSource.outputAudioMixerGroup == null && SonidosUIManager.Instancia != null && SonidosUIManager.Instancia.grupoEfectos != null)
        {
            audioSource.outputAudioMixerGroup = SonidosUIManager.Instancia.grupoEfectos;
        }
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
            Debug.LogWarning("[ControladorJugador] No se encontro el objeto de referencia: " + nombrePuntoAparicion + ". El personaje iniciara en su posicion de diseno por defecto.");
        }

        // 2. Cálculo del límite inferior de muerte en base al borde de la cámara y al tamaño del personaje
        if (Camera.main != null)
        {
            float distanciaZ = transform.position.z - Camera.main.transform.position.z;
            Vector3 limiteInferiorViewport = new Vector3(0f, 0f, distanciaZ);
            float bordeInferiorCamara = Camera.main.ViewportToWorldPoint(limiteInferiorViewport).y;
            
            // Intento de obtención del tamaño real del SpriteRenderer del personaje para calcular la invisibilidad de forma exacta
            float altoGato = 1.0f; // Valor de seguridad por defecto
            SpriteRenderer rendererGato = GetComponentInChildren<SpriteRenderer>();
            if (rendererGato != null)
            {
                altoGato = rendererGato.bounds.size.y;
            }
            
            // El personaje es declarado muerto cuando su posición física (pivote) cae por debajo del borde inferior de la cámara. 
            // Se aplica un margen de seguridad de 2.0 unidades para asegurar que, al congelarse el flujo de tiempo tras el Game Over 
            // (Time.timeScale = 0), el avatar se encuentre completamente fuera de la pantalla y resulte invisible para el usuario.
            limiteAbajo = bordeInferiorCamara - altoGato - 2.0f;
        }

        posicionOriginal = transform.position;

        if (LectorConfiguracion.Datos != null)
        {
            rb.gravityScale = LectorConfiguracion.Datos.gravedadJugador;
        }

        // Búsqueda del SpriteRenderer de la cara dentro de la jerarquía de la instancia local.
        // Esto evita depender de referencias del Inspector que apunten incorrectamente al archivo de la carpeta de Assets, previniendo alteraciones accidentales del prefab en memoria.
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer r in renderers)
        {
            if (r.gameObject.name == "Face")
            {
                faceRenderer = r;
                break;
            }
        }

        if (faceRenderer != null)
        {
            caraOriginal = faceRenderer.sprite;
            // Se asegura de restablecer la cara al estado original por defecto al inicializarse en escena
            faceRenderer.sprite = caraOriginal;
        }
    }

    /// <summary>
    /// Se procesan las entradas de usuario en cada fotograma. 
    /// Gestiona el impulso vertical (salto), las transiciones de animación y limita 
    /// la posición del jugador dentro del área de juego permitida.
    /// </summary>
    void Update()
    {
        // Se evita cualquier entrada física del jugador si se está esperando la reanudación del juego o si el salto está bloqueado
        if ((GameManager.Instancia != null && GameManager.Instancia.estaEnEsperaDeContinuacion) || Time.unscaledTime < tiempoBloqueoSalto)
        {
            return;
        }

        bool saltoDetectado = false;
#if ENABLE_INPUT_SYSTEM
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            saltoDetectado = true;
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            saltoDetectado = true;
        }
#endif

        // Anula el salto ÚNICAMENTE si el clic/toque se produjo sobre el botón de pausa
        if (saltoDetectado && GameManager.Instancia != null && GameManager.Instancia.botonPausa != null)
        {
            if (EstaSobreObjetoUI(GameManager.Instancia.botonPausa))
            {
                saltoDetectado = false;
            }
        }

        if (saltoDetectado && estaVivo && Time.timeScale > 0f)
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
            rb.bodyType = RigidbodyType2D.Dynamic;
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

    /// <summary>
    /// Congela el movimiento físico y la simulación del jugador durante transiciones o esperas.
    /// </summary>
    public void Congelar()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// Descongela la simulación física del jugador, restableciendo el tipo de cuerpo a dinámico.
    /// </summary>
    public void Descongelar()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// Bloquea temporalmente el salto durante un breve intervalo (útil tras salir de pausas o pulsar botones UI).
    /// </summary>
    /// <param name="duracion">Tiempo en segundos reales a ignorar entradas de salto.</param>
    public void BloquearSaltoTemporalmente(float duracion = 0.05f)
    {
        tiempoBloqueoSalto = Time.unscaledTime + duracion;
    }

    /// <summary>
    /// Cancela cualquier impulso vertical residual acumulado al pausar el juego.
    /// </summary>
    public void CancelarImpulsoVertical()
    {
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0f));
        }
    }

    /// <summary>
    /// Comprueba de forma reutilizable si el puntero o toque actual se encuentra sobre un GameObject de UI específico (o cualquiera de sus hijos).
    /// </summary>
    /// <param name="objetoUI">El GameObject de la UI a comprobar (por ejemplo, el botón de pausa u otro elemento interactivo).</param>
    /// <returns>True si el puntero/toque está sobre el objeto especificado, false en caso contrario.</returns>
    public bool EstaSobreObjetoUI(GameObject objetoUI)
    {
        if (objetoUI == null || EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        List<RaycastResult> resultados = new List<RaycastResult>();

        // 1. Comprobación para ratón / puntero en escritorio
        #if ENABLE_INPUT_SYSTEM
            if (Pointer.current != null)
            {
                eventData.position = Pointer.current.position.ReadValue();
            }
        #else
            eventData.position = Input.mousePosition;
        #endif

        EventSystem.current.RaycastAll(eventData, resultados);
        for (int i = 0; i < resultados.Count; i++)
        {
            if (resultados[i].gameObject == objetoUI || resultados[i].gameObject.transform.IsChildOf(objetoUI.transform))
            {
                return true;
            }
        }

        // 2. Comprobación para pantallas táctiles (móvil legacy)
#if !ENABLE_INPUT_SYSTEM
        for (int i = 0; i < Input.touchCount; i++)
        {
            eventData.position = Input.GetTouch(i).position;
            resultados.Clear();
            EventSystem.current.RaycastAll(eventData, resultados);

            for (int j = 0; j < resultados.Count; j++)
            {
                if (resultados[j].gameObject == objetoUI || resultados[j].gameObject.transform.IsChildOf(objetoUI.transform))
                {
                    return true;
                }
            }
        }
#endif

        #if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null)
            {
                var toques = Touchscreen.current.touches;
                for (int i = 0; i < toques.Count; i++)
                {
                    if (toques[i].isInProgress)
                    {
                        eventData.position = toques[i].position.ReadValue();
                        resultados.Clear();
                        EventSystem.current.RaycastAll(eventData, resultados);

                        for (int j = 0; j < resultados.Count; j++)
                        {
                            if (resultados[j].gameObject == objetoUI || resultados[j].gameObject.transform.IsChildOf(objetoUI.transform))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        #endif

        return false;
    }
}
using UnityEngine;

/// <summary>
/// Controla el desplazamiento vectorial continuo del fondo (Fondo_0).
/// Compensando el pivote central del SpriteRenderer:
/// 1. Fondo Izquierda: Su borde izquierdo (bounds.min.x) se pega al borde izquierdo de la pantalla.
/// 2. Fondo Derecha: Su borde izquierdo (bounds.min.x) se pega al borde derecho del Fondo Izquierda (bounds.max.x).
/// </summary>
public class ControladorFondo : MonoBehaviour
{
    //--------------------------------
    // CAMPOS
    //--------------------------------
    
    [Range(0f, 1f)]
    [Tooltip("Multiplicador de velocidad para simular efecto Parallax.")]
    public float efectoParallax = 0.5f;

    [Tooltip("Referencia al otro objeto de fondo para encadenarlos infinitamente.")]
    public Transform otroFondo;

    [Tooltip("Ancho total de la imagen.")]
    public float anchoImagen; 

    private Vector3 posicionOriginal;
    private SpriteRenderer spriteRenderer;

    //--------------------------------
    // MÉTODOS
    //--------------------------------
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        CalcularAncho();
        AlinearFondosIniciales();
        posicionOriginal = transform.position;
    }

    public void CalcularAncho()
    {
        if (spriteRenderer != null)
        {
            anchoImagen = spriteRenderer.bounds.size.x;
        }
        else if (anchoImagen <= 0f)
        {
            anchoImagen = transform.localScale.x;
        }
    }

    /// <summary>
    /// Alinea los fondos considerando el pivote central del SpriteRenderer:
    /// - Fondo Izquierda: bounds.min.x = bordeIzquierdoPantalla
    /// - Fondo Derecha: bounds.min.x = fondoIzquierda.bounds.max.x
    /// </summary>
    public void AlinearFondosIniciales()
    {
        if (Camera.main == null) return;

        bool esIzquierda = name.ToLower().Contains("izq");
        bool esDerecha = name.ToLower().Contains("drcha");

        float medioAncho = (spriteRenderer != null) ? spriteRenderer.bounds.extents.x : (anchoImagen / 2f);

        if (esIzquierda)
        {
            // Borde izquierdo real de la vista de la cámara
            float bordeIzquierdoPantalla = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
            
            // Posicionamos el centro del sprite de modo que su borde izquierdo caiga exactamente en bordeIzquierdoPantalla
            transform.position = new Vector3(bordeIzquierdoPantalla + medioAncho, transform.position.y, transform.position.z);
        }
        else if (esDerecha && otroFondo != null)
        {
            // Garantiza que el fondo izquierdo esté alineado a la pantalla primero
            ControladorFondo scriptOtro = otroFondo.GetComponent<ControladorFondo>();
            if (scriptOtro != null)
            {
                scriptOtro.CalcularAncho();
                scriptOtro.AlinearFondosIniciales();
            }

            SpriteRenderer srOtro = otroFondo.GetComponent<SpriteRenderer>();
            float bordeDerechoFondoIzquierda = (srOtro != null) 
                ? srOtro.bounds.max.x 
                : (otroFondo.position.x + medioAncho);

            // Posiciona el centro del fondo derecho para que su borde izquierdo caiga en el borde derecho del fondo izquierdo
            transform.position = new Vector3(bordeDerechoFondoIzquierda + medioAncho, otroFondo.position.y, transform.position.z);
        }
    }

    public void Restablecer()
    {
        AlinearFondosIniciales();
        posicionOriginal = transform.position;
    }

    void Update()
    {
        // Detener durante la pausa / espera
        if (GameManager.Instancia != null && GameManager.Instancia.estaEnEsperaDeContinuacion)
        {
            return;
        }

        float velocidadBase = 3f;
        if (GameManager.Instancia != null)
        {
            velocidadBase = GameManager.Instancia.velocidadActual;
        }

        float velocidadReal = velocidadBase * efectoParallax;
        transform.Translate(Vector3.left * velocidadReal * Time.deltaTime, Space.World);

        // 1. Límite izquierdo de la vista de la cámara
        float limiteIzquierdoCamara = (Camera.main != null) 
            ? Camera.main.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x 
            : -anchoImagen;

        // 2. Borde derecho real del gráfico actual
        float limiteBordeDerechoSprite = (spriteRenderer != null) 
            ? spriteRenderer.bounds.max.x 
            : (transform.position.x + (anchoImagen / 2f));

        // 3. Cuando el borde derecho sale totalmente por la izquierda de la cámara:
        if (limiteBordeDerechoSprite <= limiteIzquierdoCamara)
        {
            if (otroFondo != null)
            {
                SpriteRenderer srOtro = otroFondo.GetComponent<SpriteRenderer>();
                float medioAncho = (spriteRenderer != null) ? spriteRenderer.bounds.extents.x : (anchoImagen / 2f);
                float posReubicacion = (srOtro != null) 
                    ? (srOtro.bounds.max.x + medioAncho)
                    : (otroFondo.position.x + anchoImagen);

                transform.position = new Vector3(posReubicacion, otroFondo.position.y, transform.position.z);
            }
            else
            {
                transform.position += new Vector3(2f * anchoImagen, 0f, 0f);
            }
        }
    }
}
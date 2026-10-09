using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Proporciona un efecto de respiración / pulso de luz neón a cualquier componente Image de la UI.
/// Utiliza Time.unscaledTime para que la animación funcione fluidamente incluso con el menú pausado (Time.timeScale = 0).
/// </summary>
[RequireComponent(typeof(Image))]
public class BrilloNeonUI : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS CONFIGURABLES
    // -----------------------------------------------------------------------------
    
    [Header("Configuración del Pulso Neón")]
    [Tooltip("Color base / mínimo de la luz neón.")]
    public Color colorMinimo = new Color(0f, 0.8f, 1f, 0.4f);

    [Tooltip("Color máximo / brillante de la luz neón.")]
    public Color colorMaximo = new Color(0f, 1f, 1f, 1f);

    [Tooltip("Velocidad de la oscilación del pulso (más alto = más rápido).")]
    [Range(0.2f, 10f)]
    public float velocidadPulso = 2.5f;

    [Header("Variación de Escala (Opcional)")]
    [Tooltip("Si se activa, el borde aumentará y disminuirá de tamaño sutilmente junto con el brillo.")]
    public bool animarEscala = false;

    [Tooltip("Factor de escala mínima (1.0 = tamaño original).")]
    public float escalaMinima = 1.0f;

    [Tooltip("Factor de escala máxima.")]
    public float escalaMaxima = 1.03f;

    [Header("Efecto Parpadeo Eléctrico (Opcional)")]
    [Tooltip("Añade micro-parpadeos aleatorios simulando un auténtico tubo de neón espacial.")]
    public bool parpadeoElectrico = false;

    [Tooltip("Probabilidad por frame de que ocurra un micro-parpadeo.")]
    [Range(0f, 0.1f)]
    public float probabilidadParpadeo = 0.02f;

    // -----------------------------------------------------------------------------
    // REFERENCIAS INTERNAS
    // -----------------------------------------------------------------------------
    
    private Image imagen;
    private RectTransform rectTransform;
    private Vector3 escalaOriginal;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    void Awake()
    {
        imagen = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            escalaOriginal = rectTransform.localScale;
        }
    }

    void Update()
    {
        if (imagen == null) return;

        // Onda sinusoidal suave de 0 a 1 usando tiempo no escalado (funciona en menús pausados)
        float factorTiempo = (Mathf.Sin(Time.unscaledTime * velocidadPulso) + 1f) * 0.5f;

        // Micro-parpadeo de tubo neón si está habilitado
        if (parpadeoElectrico && Random.value < probabilidadParpadeo)
        {
            factorTiempo *= Random.Range(0.2f, 0.6f);
        }

        // Interpolar color entre el mínimo y el máximo
        imagen.color = Color.Lerp(colorMinimo, colorMaximo, factorTiempo);

        // Interpolar escala si está activada
        if (animarEscala && rectTransform != null)
        {
            float factorEscala = Mathf.Lerp(escalaMinima, escalaMaxima, factorTiempo);
            rectTransform.localScale = escalaOriginal * factorEscala;
        }
    }
}

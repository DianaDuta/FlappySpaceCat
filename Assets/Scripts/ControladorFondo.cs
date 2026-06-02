using UnityEngine;

/// <summary>
/// Controla el desplazamiento vectorial continuo del fondo de la pantalla,
/// aplicando un factor de escalado sobre la velocidad global para emular profundidad (efecto parallax)
/// y reubicando la geometría para simular un ciclo infinito.
/// </summary>
public class ControladorFondo : MonoBehaviour
{
    //--------------------------------
    // CAMPOS
    //--------------------------------
    
    [Range(0f, 1f)]
    public float efectoParallax = 0.5f;

    public float anchoImagen; 

    private Vector3 posicionOriginal;

    //--------------------------------
    // MÉTODOS
    //--------------------------------
    
    /// <summary>
    /// Calcula automáticamente el límite del componente gráfico si no se ha establecido 
    /// previamente, necesario para el efecto de reinicio visual.
    /// </summary>
    void Start()
    {
        posicionOriginal = transform.position;

        if (anchoImagen <= 0)
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                anchoImagen = sprite.bounds.size.x;
            }
        }
    }

    /// <summary>
    /// Restablece la posición de la imagen de fondo a sus coordenadas de diseño originales.
    /// </summary>
    public void Restablecer()
    {
        transform.position = posicionOriginal;
    }

    /// <summary>
    /// Calcula la translación del marco basándose en la configuración de velocidad global y 
    /// reposiciona la imagen fuera de la cámara cuando sobrepasa sus límites de visión izquierda.
    /// </summary>
    void Update()
    {
        float velocidadBase = 3f;
        if (GameManager.Instancia != null)
        {
            velocidadBase = GameManager.Instancia.velocidadActual;
        }

        float velocidadReal = velocidadBase * efectoParallax;
        transform.Translate(Vector3.left * velocidadReal * Time.deltaTime);

        if (transform.position.x <= -anchoImagen)
        {
            Vector3 nuevaPos = transform.position;
            nuevaPos.x += 2 * anchoImagen; 
            transform.position = nuevaPos;
        }
    }
}
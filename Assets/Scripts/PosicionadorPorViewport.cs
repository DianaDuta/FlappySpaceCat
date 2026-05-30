using UnityEngine;

/// <summary>
/// Componente que posiciona un objeto en el espacio bidimensional del mundo a partir de coordenadas 
/// relativas (porcentajes) del Viewport de la cámara principal.
/// </summary>
[ExecuteAlways]
[DefaultExecutionOrder(-50)] // Asegura la ejecución temprana en el ciclo de vida de Unity
public class PosicionadorPorViewport : MonoBehaviour
{
    [Header("Configuración de Coordenadas")]
    [Tooltip("Habilita el reposicionamiento en el eje horizontal (X).")]
    public bool posicionarEnHorizontal = true;

    [Range(0f, 1f)]
    [Tooltip("Proporción horizontal de la pantalla desde el extremo izquierdo (0.0 a 1.0).")]
    public float xViewport = 0.38f;

    [Tooltip("Habilita el reposicionamiento en el eje vertical (Y).")]
    public bool posicionarEnVertical = false;

    [Range(0f, 1f)]
    [Tooltip("Proporción vertical de la pantalla desde el extremo inferior (0.0 a 1.0).")]
    public float yViewport = 0.5f;

    /// <summary>
    /// Calcula y aplica las coordenadas del mundo durante la fase de inicialización inicial.
    /// </summary>
    private void Awake()
    {
        ActualizarPosicion();
    }

    private void Start()
    {
        ActualizarPosicion();
    }

    private void Update()
    {
        // En el editor de Unity, actualiza constantemente la posición para reflejar cambios en tiempo real
        if (!Application.isPlaying)
        {
            ActualizarPosicion();
        }
    }

    /// <summary>
    /// Traduce las coordenadas de Viewport especificadas a coordenadas físicas del mundo bidimensional.
    /// </summary>
    public void ActualizarPosicion()
    {
        if (Camera.main == null) return;

        float distanciaZ = transform.position.z - Camera.main.transform.position.z;
        Vector3 posicionViewport = new Vector3(xViewport, yViewport, distanciaZ);
        Vector3 posicionMundo = Camera.main.ViewportToWorldPoint(posicionViewport);

        float nuevoX = posicionarEnHorizontal ? posicionMundo.x : transform.position.x;
        float nuevoY = posicionarEnVertical ? posicionMundo.y : transform.position.y;

        transform.position = new Vector3(nuevoX, nuevoY, transform.position.z);
    }
}

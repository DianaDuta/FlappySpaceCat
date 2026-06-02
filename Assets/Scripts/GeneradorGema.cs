using UnityEngine;

/// <summary>
/// Asignado a los obstáculos. Gestiona la probabilidad matemática de generar 
/// un componente coleccionable hijo en las coordenadas locales correspondientes.
/// </summary>
public class GeneradorGemas : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    [Header("Configuración")]
    public GameObject prefabGema;
    
    [Tooltip("Probabilidad de instanciación expresada en porcentaje (0-100).")]
    public float probabilidadAparicion = 100f;

    [Header("Nodos")]
    public Transform[] nodosPosibles;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Evalúa la curva de probabilidad y, si se supera el umbral, 
    /// instancia la gema vinculándola a uno de los puntos ancla predefinidos.
    /// </summary>
    void Start()
    {
        if (Random.Range(0f, 100f) > probabilidadAparicion) return;

        if (nodosPosibles.Length > 0 && prefabGema != null)
        {
            int indiceAleatorio = Random.Range(0, nodosPosibles.Length);
            Transform nodoElegido = nodosPosibles[indiceAleatorio];

            Instantiate(prefabGema, nodoElegido.position, Quaternion.identity, nodoElegido);
        }
    }
}
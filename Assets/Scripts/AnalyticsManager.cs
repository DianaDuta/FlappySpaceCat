using UnityEngine;
using Firebase.Analytics;

/// <summary>
/// Concentra la lógica de despacho de métricas telemétricas hacia los servidores de Firebase Analytics.
/// </summary>
public class AnalyticsManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Provisión estática de la clase para ser consumida de manera ubicua a través del sistema.
    /// </summary>
    public static AnalyticsManager Instancia;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Valida el cumplimiento del diseño Singleton y protege el objeto de descargas prematuras de memoria.
    /// </summary>
    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Interfaz para el registro y subida de una interacción puntual carente de metadatos complejos.
    /// </summary>
    /// <param name="nombreEvento">Cadena identificativa vinculada a un hito en el diseño del sistema analítico.</param>
    public void RegistrarEventoSimple(string nombreEvento)
    {
        FirebaseAnalytics.LogEvent(nombreEvento);
        Debug.Log("Notificación telemétrica despachada con la firma -> " + nombreEvento);
    }

    /// <summary>
    /// Envía un registro multidimensional que asocia un fallo en la sesión con el parámetro de velocidad de los algoritmos de dificultad en dicho lapso.
    /// </summary>
    /// <param name="velocidad">Módulo actual que describe el desplazamiento vectorial del nivel relativo al actor.</param>
    public void RegistrarEventoDificultad(float velocidad)
    {
        FirebaseAnalytics.LogEvent("muerte_por_velocidad", "velocidad_final", velocidad);
    }
}
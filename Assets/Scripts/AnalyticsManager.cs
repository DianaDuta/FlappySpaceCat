using UnityEngine;
using Firebase.Analytics;

/**
* CLASE ANALYTICS MANAGER:
* Centraliza el envío de eventos a Firebase Analytics.
* Utiliza el patrón Singleton para ser accesible desde cualquier script.
*/
public class AnalyticsManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    public static AnalyticsManager Instancia;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /*
    * Método Awake():
    * Garantiza que solo exista una instancia y que no se destruya entre escenas.
    */
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

    /*
    * Método RegistrarEventoSimple():
    * Envía un evento básico sin parámetros adicionales.
    * @param string nombreEvento: El nombre que aparecerá en la consola de Firebase.
    */
    public void RegistrarEventoSimple(string nombreEvento)
    {
        FirebaseAnalytics.LogEvent(nombreEvento);
        Debug.Log("Analytics: Evento registrado -> " + nombreEvento);
    }

    /*
    * Método RegistrarEventoDificultad():
    * Registra la velocidad en la que el jugador ha muerto.
    * @param float velocidad: La velocidad actual del GameManager.
    */
    public void RegistrarEventoDificultad(float velocidad)
    {
        FirebaseAnalytics.LogEvent("muerte_por_velocidad", "velocidad_final", velocidad);
    }
}
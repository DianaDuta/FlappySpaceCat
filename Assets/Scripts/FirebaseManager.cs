using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions; 

/// <summary>
/// Provee la inicialización central de la instancia de la aplicación Firebase,
/// resolviendo dependencias de la plataforma en tiempo de ejecución.
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Instancia estática global que previene la inicialización múltiple del SDK.
    /// </summary>
    public static FirebaseManager Instancia;
    
    [Header("Estado de la Conexión")]
    public bool firebaseListo = false;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Construye el Singleton y marca el objeto para no ser destruido al cambiar de contexto.
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
    /// Llama al proceso de inicialización y verificación de dependencias de Firebase
    /// al montar el componente.
    /// </summary>
    void Start()
    {
        DespertarFirebase();
    }

    /// <summary>
    /// Verifica asíncronamente las dependencias de Google Play Services en el dispositivo,
    /// inicializa la configuración de la aplicación y habilita la recolección analítica si procede.
    /// </summary>
    private void DespertarFirebase()
    {
        Debug.Log("Iniciando validación de dependencias del SDK de Firebase.");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(tarea => 
        {
            if (tarea.Result == DependencyStatus.Available)
            {
                firebaseListo = true;
                Debug.Log("Servicios de Firebase inicializados y operativos.");

                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            }
            else
            {
                Debug.LogError(string.Format("Se interrumpió la inicialización de Firebase con el siguiente código de estado: {0}", tarea.Result));
            }
        });
    }
}
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions; // Para no congelar el juego

/**
* CLASE FIREBASE MANAGER:
* Es único y global. No se destruye al cambiar de escena.
* Se encarga de despertar y conectar Firebase al inicio del juego.
*/
public class FirebaseManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    public static FirebaseManager Instancia;
    
    [Header("Estado de la Conexión")]
    public bool firebaseListo = false;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /*
    * Método Awake():
    * Patrón Singleton: Asegura que solo exista un FirebaseManager en todo el juego
    * y que sobreviva al viajar entre el Menú y el Juego.
    */
    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            // Para que el objeto no se destruya al cargar otra escena
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /*
    * Método Start():
    * Nada más arrancar la aplicación, llama a Firebase.
    */
    void Start()
    {
        DespertarFirebase();
    }

    /*
    * Método DespertarFirebase():
    * Comprueba si el móvil del jugador es compatible y conecta con nuestro google-services.json
    */
    private void DespertarFirebase()
    {
        Debug.Log("Intentando conectar con los servidores de Firebase.");

        // CheckAndFixDependenciesAsync revisa que el móvil tenga todo lo necesario instalado
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(tarea => 
        {
            if (tarea.Result == DependencyStatus.Available)
            {
                // Si la respuesta es positiva, encendemos el sistema
                firebaseListo = true;
                Debug.Log("Firebase conectado y listo.");

                // Analytics: para recopilar datos
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            }
            else
            {
                // Si falla, avisa con el error exacto en la consola
                Debug.LogError("Fallo al iniciar Firebase: " + tarea.Result);
            }
        });
    }
}
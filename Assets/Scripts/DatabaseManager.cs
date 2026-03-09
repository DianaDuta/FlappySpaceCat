using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic; // Para crear "Diccionarios" de datos

/**
* CLASE DATABASE MANAGER:
* Se encarga de enviar y recibir datos de la bbdd Firestore.
* Es un Singleton global para poder llamarlo desde cualquier parte.
*/
public class DatabaseManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    public static DatabaseManager Instancia;
    private FirebaseFirestore db;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    void Awake()
    {
        // Patrón Singleton clásico
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

    void Start()
    {
        // Inicializamos la conexión con el archivador de Firestore
        db = FirebaseFirestore.DefaultInstance;
    }

    /*
    * Método GuardarGemasEnNube():
    * Crea una carpeta para el jugador y guarda su número de gemas.
    * @param string idUsuario: El código secreto del jugador que nos da Firebase Auth
    * @param int cantidadGemas: Las gemas que sacamos del PlayerPrefs
    */
    public void GuardarGemasEnNube(string idUsuario, int cantidadGemas)
    {
        // Apunta a la ruta: Cajón "Jugadores" -> Carpeta "idUsuario"
        DocumentReference docRef = db.Collection("Jugadores").Document(idUsuario);

        // Prepara los datos en formato "Diccionario" (Nombre del dato y su valor)
        Dictionary<string, object> datosUsuario = new Dictionary<string, object>
        {
            { "gemasTotales", cantidadGemas },
            { "ultimaConexion", FieldValue.ServerTimestamp } // Guarda la fecha y hora
        };

        // Envía los datos a la nube
        docRef.SetAsync(datosUsuario).ContinueWithOnMainThread(tarea => 
        {
            if (tarea.IsFaulted)
            {
                Debug.LogError("Error al guardar en la nube: " + tarea.Exception);
            }
            else if (tarea.IsCompleted)
            {
                Debug.Log("¡Caja fuerte actualizada en la nube! Gemas aseguradas.");
            }
        });
    }
}
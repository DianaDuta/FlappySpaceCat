using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic; 

/// <summary>
/// Gestiona la lectura y escritura asíncrona de documentos en la base de datos Firestore,
/// implementando un patrón Singleton para acceso global.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Instancia estática global para el acceso a las funciones de base de datos.
    /// </summary>
    public static DatabaseManager Instancia;
    private FirebaseFirestore db;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Inicializa la instancia Singleton asegurando la persistencia del objeto entre escenas.
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
    /// Establece la referencia principal a la base de datos Firestore.
    /// </summary>
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    /// <summary>
    /// Persiste la cantidad actual de gemas en el documento correspondiente al usuario en la nube,
    /// combinando los datos nuevos con la información preexistente para evitar sobreescritura de otros campos.
    /// </summary>
    /// <param name="idUsuario">Identificador único del usuario provisto por Firebase Auth.</param>
    /// <param name="cantidadGemas">Valor numérico del total de gemas acumuladas.</param>
    public void GuardarGemasEnNube(string idUsuario, int cantidadGemas)
    {
        DocumentReference docRef = db.Collection("Jugadores").Document(idUsuario);

        Dictionary<string, object> datosUsuario = new Dictionary<string, object>
        {
            { "gemasTotales", cantidadGemas },
            { "ultimaConexion", FieldValue.ServerTimestamp } 
        };

        docRef.SetAsync(datosUsuario, SetOptions.MergeAll).ContinueWithOnMainThread(tarea => 
        {
            if (tarea.IsFaulted)
            {
                Debug.LogError("Fallo durante la transacción de escritura: " + tarea.Exception);
            }
            else if (tarea.IsCompleted)
            {
                Debug.Log("Sincronización de progreso (gemas) completada correctamente.");
            }
        });
    }

    /// <summary>
    /// Almacena una nueva marca máxima de puntuación en el documento del usuario en Firestore.
    /// </summary>
    /// <param name="idUsuario">Identificador único del usuario autenticado.</param>
    /// <param name="mejorPuntuacion">El valor numérico del récord alcanzado.</param>
    public void GuardarMejorPuntuacionEnNube(string idUsuario, int mejorPuntuacion)
    {
        DocumentReference docRef = db.Collection("Jugadores").Document(idUsuario);

        Dictionary<string, object> datosUsuario = new Dictionary<string, object>
        {
            { "mejorPuntuacion", mejorPuntuacion }
        };

        docRef.SetAsync(datosUsuario, SetOptions.MergeAll).ContinueWithOnMainThread(tarea => 
        {
            if (tarea.IsFaulted) Debug.LogError("Fallo al persistir la puntuación máxima: " + tarea.Exception);
            else if (tarea.IsCompleted) Debug.Log("Sincronización de registro máximo completada: " + mejorPuntuacion);
        });
    }

    /// <summary>
    /// Almacena la calificación por estrellas otorgada por el jugador en Firestore.
    /// </summary>
    /// <param name="idUsuario">Identificador único del usuario.</param>
    /// <param name="estrellas">Número de estrellas (1 a 5).</param>
    public void GuardarCalificacionEnNube(string idUsuario, int estrellas)
    {
        if (db == null) db = FirebaseFirestore.DefaultInstance;
        if (db == null || string.IsNullOrEmpty(idUsuario)) return;

        DocumentReference docRef = db.Collection("Jugadores").Document(idUsuario);

        Dictionary<string, object> datos = new Dictionary<string, object>
        {
            { "calificacionEstrellas", estrellas },
            { "fechaCalificacion", FieldValue.ServerTimestamp }
        };

        docRef.SetAsync(datos, SetOptions.MergeAll).ContinueWithOnMainThread(tarea =>
        {
            if (tarea.IsFaulted)
            {
                Debug.LogError("Error al guardar la calificación en Firestore: " + tarea.Exception);
            }
            else if (tarea.IsCompleted)
            {
                Debug.Log($"⭐ Calificación de {estrellas} estrellas registrada en Firestore.");
            }
        });
    }

    /// <summary>
    /// Consulta asíncronamente el documento del usuario para extraer la mejor puntuación registrada.
    /// Ejecuta una acción de retorno (callback) al finalizar la operación.
    /// </summary>
    /// <param name="idUsuario">Identificador único del usuario.</param>
    /// <param name="alCompletar">Delegado ejecutado tras procesar la consulta, retornando el valor numérico (0 si no existe).</param>
    public void ObtenerMejorPuntuacion(string idUsuario, System.Action<int> alCompletar)
    {
        DocumentReference docRef = db.Collection("Jugadores").Document(idUsuario);
        
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(tarea =>
        {
            if (tarea.IsCompleted && !tarea.IsFaulted)
            {
                DocumentSnapshot snap = tarea.Result;
                if (snap.Exists && snap.ContainsField("mejorPuntuacion"))
                {
                    int mejor = snap.GetValue<int>("mejorPuntuacion");
                    alCompletar?.Invoke(mejor);
                    return;
                }
            }
            alCompletar?.Invoke(0);
        });
    }

    /// <summary>
    /// Consulta asíncronamente el documento del usuario para extraer el total de gemas registradas en la nube.
    /// Ejecuta una acción de retorno (callback) al finalizar la operación.
    /// </summary>
    /// <param name="idUsuario">Identificador único del usuario.</param>
    /// <param name="alCompletar">Delegado ejecutado tras procesar la consulta, retornando el valor numérico (0 si no existe).</param>
    public void ObtenerGemasTotales(string idUsuario, System.Action<int> alCompletar)
    {
        DocumentReference docRef = db.Collection("Jugadores").Document(idUsuario);
        
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(tarea =>
        {
            if (tarea.IsCompleted && !tarea.IsFaulted)
            {
                DocumentSnapshot snap = tarea.Result;
                if (snap.Exists && snap.ContainsField("gemasTotales"))
                {
                    int gemas = snap.GetValue<int>("gemasTotales");
                    alCompletar?.Invoke(gemas);
                    return;
                }
            }
            alCompletar?.Invoke(0);
        });
    }

    /// <summary>
    /// Guarda el desbloqueo de una skin en el documento del usuario en Firestore.
    /// </summary>
    /// <param name="idUsuario">Identificador único del usuario.</param>
    /// <param name="indiceSkin">Índice de la skin desbloqueada.</param>
    public void GuardarSkinsDesbloqueadas(string idUsuario, int indiceSkin)
    {
        if (db == null) db = FirebaseFirestore.DefaultInstance;
        if (db == null || string.IsNullOrEmpty(idUsuario)) return;

        DocumentReference docRef = db.Collection("Jugadores").Document(idUsuario);

        Dictionary<string, object> datos = new Dictionary<string, object>
        {
            { "SkinDesbloqueada_" + indiceSkin, 1 },
            { "ultimaModificacion", FieldValue.ServerTimestamp }
        };

        docRef.SetAsync(datos, SetOptions.MergeAll).ContinueWithOnMainThread(tarea =>
        {
            if (tarea.IsFaulted)
            {
                Debug.LogError("Error al persistir skin en Firestore: " + tarea.Exception);
            }
            else if (tarea.IsCompleted)
            {
                Debug.Log($"🎨 Skin {indiceSkin} sincronizada en la nube.");
            }
        });
    }
}
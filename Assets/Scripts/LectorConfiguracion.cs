using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections;

/// <summary>
/// Mapea la estructura serializada de las propiedades procedentes de los archivos remotos o locales.
/// </summary>
[System.Serializable]
public class DatosJuego
{
    public float velocidadJuego = 3.0f;
    public float frecuenciaObstaculos = 2.0f;
    public float gravedadJugador = 1.0f;
}

/// <summary>
/// Se encarga de deserializar la configuración JSON en cualquier plataforma (incluyendo Android APK),
/// asegurando que la velocidad y gravedad sean idénticas a las probadas en el Editor.
/// </summary>
public class LectorConfiguracion : MonoBehaviour
{
    public static DatosJuego Datos = new DatosJuego { velocidadJuego = 3.0f, frecuenciaObstaculos = 2.0f, gravedadJugador = 1.0f };

    void Awake()
    {
        // Se fija el refresco a 60 FPS en teléfonos móviles para sincronizar la física exactamente igual al Editor
        Application.targetFrameRate = 60;
        StartCoroutine(CargarConfiguracion());
    }

    private IEnumerator CargarConfiguracion()
    {
        string rutaArchivo = Path.Combine(Application.streamingAssetsPath, "configuracion.json");

#if UNITY_ANDROID && !UNITY_EDITOR
        using (UnityWebRequest request = UnityWebRequest.Get(rutaArchivo))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string contenidoJson = request.downloadHandler.text;
                Datos = JsonUtility.FromJson<DatosJuego>(contenidoJson);
                Debug.Log("✅ [Android] configuracion.json cargado vía WebRequest. Velocidad: " + Datos.velocidadJuego + ", Gravedad: " + Datos.gravedadJugador);
            }
            else
            {
                Debug.LogWarning("⚠️ [Android] No se pudo leer configuracion.json en APK. Usando valores estándar: Velocidad 3.0, Gravedad 1.0");
                Datos = new DatosJuego { velocidadJuego = 3.0f, frecuenciaObstaculos = 2.0f, gravedadJugador = 1.0f };
            }
        }
#else
        if (File.Exists(rutaArchivo))
        {
            string contenidoJson = File.ReadAllText(rutaArchivo);
            Datos = JsonUtility.FromJson<DatosJuego>(contenidoJson);
            Debug.Log("✅ configuracion.json parseado correctamente. Velocidad: " + Datos.velocidadJuego + ", Gravedad: " + Datos.gravedadJugador);
        }
        else
        {
            Datos = new DatosJuego { velocidadJuego = 3.0f, frecuenciaObstaculos = 2.0f, gravedadJugador = 1.0f };
        }
        yield return null;
#endif

        AplicarValores();
    }

    public static void AplicarValores()
    {
        if (Datos == null) return;

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.velocidadInicial = Datos.velocidadJuego;
            GameManager.Instancia.velocidadActual = Datos.velocidadJuego;
        }

        ControladorJugagor jugador = FindFirstObjectByType<ControladorJugagor>();
        if (jugador != null)
        {
            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = Datos.gravedadJugador;
            }
        }
    }
}

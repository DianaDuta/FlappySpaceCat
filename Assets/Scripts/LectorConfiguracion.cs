using UnityEngine;
using System.IO; 

/// <summary>
/// Mapea la estructura serializada de las propiedades procedentes de los archivos remotos o locales.
/// </summary>
[System.Serializable]
public class DatosJuego
{
    public float velocidadJuego;
    public float frecuenciaObstaculos;
    public float gravedadJugador;
}

/// <summary>
/// Se encarga de deserializar la configuración JSON localizada en el entorno del dispositivo, 
/// brindando variables globales preconfiguradas y previniendo errores por archivos faltantes.
/// </summary>
public class LectorConfiguracion : MonoBehaviour
{
    /// <summary>
    /// Repositorio de parámetros globales al que pueden suscribirse los demás módulos.
    /// </summary>
    public static DatosJuego Datos = new DatosJuego { velocidadJuego = 4f, frecuenciaObstaculos = 2f, gravedadJugador = 1.5f };

    /// <summary>
    /// Localiza el archivo JSON alojado en StreamingAssets y parsea la cadena a un objeto C#.
    /// Asigna parámetros de contingencia en caso de presentarse una excepción I/O.
    /// </summary>
    void Awake()
    {
        string rutaArchivo = Path.Combine(Application.streamingAssetsPath, "configuracion.json");

        if (File.Exists(rutaArchivo))
        {
            string contenidoJson = File.ReadAllText(rutaArchivo);
            
            Datos = JsonUtility.FromJson<DatosJuego>(contenidoJson);
            
            Debug.Log("Archivo de inicialización parseado correctamente. Constante de velocidad cargada: " + Datos.velocidadJuego);
        }
        else
        {
            Debug.LogError("Excepción I/O: Archivo de configuración inexistente. Activando perfil de contingencia.");
            Datos = new DatosJuego {
                velocidadJuego = 4f,
                frecuenciaObstaculos = 2f,
                gravedadJugador = 1.5f
            };
        }
    }
}

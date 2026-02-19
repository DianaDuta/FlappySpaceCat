using UnityEngine;
using System.IO; // Para leer archivos

/*
* Clase que representa la estructura de los datos de configuración del juego.
* Los nombres de las variables deben coincidir con los nombres de las propiedades en el archivo JSON para que JsonUtility pueda mapearlos correctamente.
*/
[System.Serializable]
public class DatosJuego
{
    public float velocidadJuego;
    public float frecuenciaObstaculos;
    public float gravedadJugador;
}
/*
* Este script se encarga de leer la configuración del juego desde un archivo JSON.
* El archivo JSON debe estar ubicado en la carpeta "StreamingAssets" del proyecto de Unity.
* La clase DatosJuego define la estructura de los datos que se esperan en el JSON.
* La instancia estática "Datos" permite acceder a estos valores desde cualquier otro script.
*/
public class LectorConfiguracion : MonoBehaviour
{
    // Instancia estática para poder acceder a los datos desde cualquier otro script
    public static DatosJuego Datos;

/*
* Método Awake, se construye la ruta al archivo JSON y se verifica si existe.
* Si el archivo existe, se lee su contenido y se parsea a un objeto de tipo DatosJuego utilizando JsonUtility.
* Si el archivo no se encuentra, se asignan valores por defecto a la instancia de DatosJuego y se muestra un mensaje de error en la consola.
*/
    void Awake()
    {
        // Construye la ruta al archivo JSON simulando que es una respuesta de servidor
        string rutaArchivo = Path.Combine(Application.streamingAssetsPath, "configuracion.json");

        if (File.Exists(rutaArchivo))
        {
            // Lee el texto del archivo
            string contenidoJson = File.ReadAllText(rutaArchivo);
            
            // Parsea el texto a un objeto de Unity
            Datos = JsonUtility.FromJson<DatosJuego>(contenidoJson);
            
            Debug.Log("Configuración cargada. Velocidad: " + Datos.velocidadJuego);
        }
        else
        {
            Debug.LogError("No se encontró el archivo de configuración. Usando valores por defecto.");
            Datos = new DatosJuego {
                velocidadJuego = 3f,
                frecuenciaObstaculos = 2f,
                gravedadJugador = 1.5f
            };
        }
    }
}

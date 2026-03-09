using UnityEngine;
using Firebase.RemoteConfig;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

/**
* CLASE REMOTE CONFIG MANAGER:
* Se conecta a Firebase al iniciar el juego, descarga los ajustes
* y sobrescribe las reglas del GameManager a distancia.
*/
public class RemoteConfigManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    void Start()
    {
        ConfigurarValoresPorDefecto();
    }

    /*
    * Método ConfigurarValoresPorDefecto():
    * Crea un "salvavidas". Si el jugador abre el juego sin internet,
    * Unity usará estos valores para que el juego no se rompa.
    */
    private void ConfigurarValoresPorDefecto()
    {
        Dictionary<string, object> valoresPorDefecto = new Dictionary<string, object>
        {
            // Debe llamarse EXACTAMENTE igual que la clave de la web
            { "velocidad_juego", 4.0 } 
        };

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(valoresPorDefecto).ContinueWithOnMainThread(tarea =>
        {
            // Una vez puestos los salvavidas, se piden los datos reales a la nube
            ObtenerDatosDeLaNube();
        });
    }

    /*
    * Método ObtenerDatosDeLaNube():
    * Llama a los servidores de Google para ver si ha cambiado algo en la web.
    */
    private void ObtenerDatosDeLaNube()
    {
        Debug.Log("Llamando a la web para descargar Remote Config...");

        // TimeSpan.Zero hace que descargue la info al instante.
        // Se suele poner TimeSpan.FromHours(12) para no gastar batería del móvil.
        FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(tareaFetch =>
        {
            if (tareaFetch.IsFaulted)
            {
                Debug.LogError("Error al descargar Remote Config. Jugaremos con los valores por defecto.");
                return;
            }

            // Si la descarga ha ido bien, se "Activan" los datos para poder usarlos
            FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(tareaActivar =>
            {
                Debug.Log("¡Nuevos parámetros de la nube descargados y activados!");
                AplicarAjustesAlJuego();
            });
        });
    }

    /*
    * Método AplicarAjustesAlJuego():
    * Coge el número que ha venido de Google y se lo inyecta al GameManager.
    */
    private void AplicarAjustesAlJuego()
    {
        // Se extrae el valor de la nube. 
        // Firebase devuelve números largos (Double), así que se convierte a un número normal (float)
        float velocidadNube = (float)FirebaseRemoteConfig.DefaultInstance.GetValue("velocidad_juego").DoubleValue;

        Debug.Log("La nube de Firebase dice que la velocidad inicial debe ser: " + velocidadNube);

        // Se pasa al GameManager para que cambie la partida
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.velocidadInicial = velocidadNube;
            
            // Se fuerza a que la velocidad actual se actualice de golpe
            GameManager.Instancia.velocidadActual = velocidadNube;
        }
    }
}
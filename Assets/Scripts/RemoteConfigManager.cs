using UnityEngine;
using Firebase.RemoteConfig;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

/// <summary>
/// Gestiona la recuperación asíncrona de valores paramétricos desde la nube (Firebase Remote Config),
/// inyectando actualizaciones de diseño y métricas al vuelo dentro de los gestores locales.
/// </summary>
public class RemoteConfigManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Llama al proceso de inicialización de salvavidas locales antes de solicitar datos en red.
    /// </summary>
    void Start()
    {
        ConfigurarValoresPorDefecto();
    }

    /// <summary>
    /// Establece variables de contingencia que asegurarán el funcionamiento del sistema 
    /// en caso de fallo en la red o latencia prolongada.
    /// </summary>
    private void ConfigurarValoresPorDefecto()
    {
        Dictionary<string, object> valoresPorDefecto = new Dictionary<string, object>
        {
            { "velocidad_juego", 4.0 } 
        };

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(valoresPorDefecto).ContinueWithOnMainThread(tarea =>
        {
            ObtenerDatosDeLaNube();
        });
    }

    /// <summary>
    /// Envía una solicitud de consulta para descargar la configuración distribuida más reciente
    /// y proceder con su activación en la memoria caché.
    /// </summary>
    private void ObtenerDatosDeLaNube()
    {
        Debug.Log("Iniciando solicitud de metadatos de configuración remota...");

        FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(tareaFetch =>
        {
            if (tareaFetch.IsFaulted)
            {
                Debug.LogError("Error en la descarga de configuración remota. Operando con valores de contingencia.");
                return;
            }

            FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(tareaActivar =>
            {
                Debug.Log("Sincronización de parámetros de nube completada.");
                AplicarAjustesAlJuego();
            });
        });
    }

    /// <summary>
    /// Procesa los tipos de datos obtenidos en la nube y los asigna dinámicamente 
    /// al GameManager central para mutar el comportamiento de la partida.
    /// </summary>
    private void AplicarAjustesAlJuego()
    {
        float velocidadNube = (float)FirebaseRemoteConfig.DefaultInstance.GetValue("velocidad_juego").DoubleValue;

        Debug.Log("Parámetro [velocidad_juego] ajustado desde servidor: " + velocidadNube);

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.velocidadInicial = velocidadNube;
            GameManager.Instancia.velocidadActual = velocidadNube;
        }
    }
}
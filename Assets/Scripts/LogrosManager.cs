using UnityEngine;
using System;
using UnityEngine.SocialPlatforms;

#if GOOGLE_PLAY_GAMES
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

/// <summary>
/// Listado de logros oficiales del videojuego.
/// Define las equivalencias numéricas asociadas a cada tipo de logro.
/// </summary>
public enum TipoLogro
{
    PrimerosPasos = 0,      // Supera 1 obstáculo
    PilotoNovato = 1,       // Alcanza 10 puntos
    AstronautaHabil = 2,    // Alcanza 50 puntos
    CapitanEstelar = 3,     // Alcanza 100 puntos
    LeyendaCosmos = 4,      // Alcanza 250 puntos
    AgujeroNegro = 5,       // Juega 50 partidas en total
    VelocidadLuz = 6,       // Dificultad aumenta 3 veces
    BolsillosLlenos = 7,    // 100 gemas acumuladas en total
    MagnateGalaxia = 8,     // 5000 gemas acumuladas en total
    RachaCodiciosa = 9,     // Recoge 10 gemas en UNA partida
    FiebreCristal = 10,     // Recoge 100 gemas en UNA partida
    AvaroDespistado = 11,   // Muere justo al recoger una gema
    CambioLook = 12,        // Compra tu primera skin
    Coleccionista = 13,     // Desbloquea todas las skins
    GatoEstrellado = 14,    // Muere 100 veces en total
    Patrocinador = 15,      // Ve 1 anuncio recompensado
    Persistencia = 16,      // Revive usando el botón de continuar
    VueloCorto = 17,        // Muere en el primer obstáculo
    MuuuyAlto = 18,         // Juega con la skin de Cowsmo
    OsoOrbital = 19         // Juega con la skin del Bear
}

/// <summary>
/// Estructura de datos empleada para realizar la correspondencia entre los logros
/// definidos en el enumerado local y los identificadores alfanuméricos provistos por Google Play Console.
/// </summary>
[System.Serializable]
public struct MapeoLogroGPGS
{
    public TipoLogro tipo;
    public string idGooglePlay;
}

/// <summary>
/// Controlador principal del sistema de logros. Administra el progreso acumulativo,
/// la persistencia de datos locales y la sincronización con los servicios de Google Play Games.
/// </summary>
public class LogrosManager : MonoBehaviour
{
    // Instancia única (Singleton) para acceso global.
    public static LogrosManager Instancia;

    [Header("Progreso Acumulativo Interno")]
    public int partidasJugadas;
    public int muertesTotales;

    [Header("Google Play Games Services (Logros)")]
    [Tooltip("Habilita o deshabilita la comunicación con la plataforma de Google Play Games Services.")]
    public bool usarGooglePlayGames = true;
    
    [Tooltip("Colección de mapeos entre logros del sistema interno e identificadores alfanuméricos de Google Play Games.")]
    public MapeoLogroGPGS[] mapeoLogrosGPGS;

    private bool autenticadoEnGooglePlay = false;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
            CargarProgresoLocal();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        IniciarGooglePlayGames();
    }

    /// <summary>
    /// Inicializa la colección de mapeo de logros cargando de forma automática todos los elementos
    /// definidos en el enumerado local. Método de soporte exclusivo para el flujo de trabajo en el Editor.
    /// </summary>
    #if UNITY_EDITOR
    private void Reset()
    {
        Array valoresEnum = Enum.GetValues(typeof(TipoLogro));
        mapeoLogrosGPGS = new MapeoLogroGPGS[valoresEnum.Length];
        for (int i = 0; i < valoresEnum.Length; i++)
        {
            mapeoLogrosGPGS[i] = new MapeoLogroGPGS
            {
                tipo = (TipoLogro)valoresEnum.GetValue(i),
                idGooglePlay = ""
            };
        }
    }
    #endif

    /// <summary>
    /// Establece la conexión e inicializa de forma asíncrona la integración con Google Play Games Services.
    /// </summary>
    public void IniciarGooglePlayGames()
    {
        if (!usarGooglePlayGames)
        {
            Debug.Log("🎮 [GPGS] La conexión con Google Play Games está desactivada por configuración local.");
            return;
        }

        Debug.Log("🎮 [GPGS] Iniciando conexión con Google Play Games Services...");

        #if GOOGLE_PLAY_GAMES
        // Configuración e inicialización del plugin de Google Play Games
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .RequestEmail()
            .RequestServerAuthCode(false)
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true; // Habilita la depuración en registros de Android
        PlayGamesPlatform.Activate(); // Establece el proveedor activo para la interfaz Social
        #endif

        // Proceso de autenticación asíncrona no intrusiva (silenciosa)
        Social.localUser.Authenticate((bool exito) =>
        {
            if (exito)
            {
                autenticadoEnGooglePlay = true;
                Debug.Log("🎮 [GPGS] ¡Autenticación exitosa! Jugador: " + Social.localUser.userName + " (ID: " + Social.localUser.id + ")");
                
                // Sincronización de logros almacenados localmente durante sesiones previas sin conexión
                SincronizarLogrosConGooglePlay();
            }
            else
            {
                autenticadoEnGooglePlay = false;
                Debug.LogWarning("🎮 [GPGS] La autenticación silenciosa no se pudo completar. Esto es normal en el Editor de Unity o si el dispositivo no tiene los servicios de Google actualizados.");
            }
        });
    }

    /// <summary>
    /// Carga desde la memoria física los contadores de progreso del personaje.
    /// </summary>
    private void CargarProgresoLocal()
    {
        partidasJugadas = SecurePrefs.GetInt("Progreso_PartidasJugadas", 0);
        muertesTotales = SecurePrefs.GetInt("Progreso_MuertesTotales", 0);
    }

    /// <summary>
    /// Comprueba si un logro específico ya ha sido registrado como desbloqueado.
    /// Devuelve verdadero si el logro está completado (valor igual a 1).
    /// </summary>
    public bool EstaDesbloqueado(TipoLogro logro)
    {
        return SecurePrefs.GetInt("Logro_" + logro.ToString(), 0) == 1;
    }

    /// <summary>
    /// Registra el desbloqueo de un logro de forma persistente, asigna la recompensa correspondiente
    /// de gemas y realiza el reporte hacia la plataforma de servicios en la nube.
    /// </summary>
    public void DesbloquearLogro(TipoLogro logro)
    {
        if (!EstaDesbloqueado(logro))
        {
            SecurePrefs.SetInt("Logro_" + logro.ToString(), 1);
            
            // Recompensa de 100 gemas por logro conseguido
            int gemasGuardadas = SecurePrefs.GetInt("GemasLocales", 0);
            SecurePrefs.SetInt("GemasLocales", gemasGuardadas + 100);
            
            SecurePrefs.Save();
            
            Debug.Log("🏆 ¡NUEVO LOGRO DESBLOQUEADO!: " + logro.ToString() + " - Recompensa: 100 gemas");
            
            // Reporte asíncrono inmediato a la plataforma social vinculada
            ReportarLogroAGooglePlay(logro);
        }
    }

    /// <summary>
    /// Realiza el reporte de finalización de un logro hacia los servidores de Google Play Games.
    /// </summary>
    private void ReportarLogroAGooglePlay(TipoLogro logro)
    {
        if (!usarGooglePlayGames) return;

        string idGPGS = ObtenerIdGooglePlay(logro);
        if (string.IsNullOrEmpty(idGPGS))
        {
            Debug.LogWarning("⚠️ [GPGS] No se ha configurado el ID de Google Play para el logro: " + logro.ToString());
            return;
        }

        // Verificación del estado de autenticación del usuario
        if (Social.localUser.authenticated)
        {
            // Reporte de completitud del logro (100% de progreso)
            Social.ReportProgress(idGPGS, 100.0, (bool exito) =>
            {
                if (exito)
                {
                    Debug.Log("🎯 [GPGS] Logro reportado con éxito a Google Play: " + logro.ToString() + " (" + idGPGS + ")");
                }
                else
                {
                    Debug.LogError("❌ [GPGS] Error al reportar el logro a Google Play: " + logro.ToString() + " (" + idGPGS + ")");
                }
            });
        }
        else
        {
            Debug.LogWarning("⚠️ [GPGS] No autenticado en Google Play. El logro " + logro.ToString() + " se sincronizará automáticamente la próxima vez que te conectes online.");
        }
    }

    /// <summary>
    /// Sincroniza de forma asíncrona con los servidores de Google Play Games la totalidad de los logros
    /// que hayan sido desbloqueados localmente durante partidas en modo sin conexión.
    /// </summary>
    public void SincronizarLogrosConGooglePlay()
    {
        if (!usarGooglePlayGames || !Social.localUser.authenticated) return;

        Debug.Log("🔄 [GPGS] Sincronizando logros locales desbloqueados en segundo plano con Google Play...");

        Array valoresEnum = Enum.GetValues(typeof(TipoLogro));
        int totalReportados = 0;

        foreach (TipoLogro logro in valoresEnum)
        {
            if (EstaDesbloqueado(logro))
            {
                string idGPGS = ObtenerIdGooglePlay(logro);
                if (!string.IsNullOrEmpty(idGPGS))
                {
                    totalReportados++;
                    ReportarLogroAGooglePlay(logro);
                }
            }
        }

        if (totalReportados > 0)
        {
            Debug.Log("🔄 [GPGS] Sincronización finalizada. Enviados: " + totalReportados + " logros.");
        }
        else
        {
            Debug.Log("🔄 [GPGS] Sincronización finalizada. Todos los logros están al día.");
        }
    }

    /// <summary>
    /// Recupera la correspondencia del identificador único de Google Play asignado a un TipoLogro.
    /// </summary>
    public string ObtenerIdGooglePlay(TipoLogro logro)
    {
        if (mapeoLogrosGPGS != null)
        {
            foreach (MapeoLogroGPGS mapeo in mapeoLogrosGPGS)
            {
                if (mapeo.tipo == logro)
                {
                    return mapeo.idGooglePlay;
                }
            }
        }
        return "";
    }

    // -------------------------------------------------------------------------
    // MÉTODOS AUXILIARES PARA LOGROS ACUMULATIVOS
    // -------------------------------------------------------------------------

    public void SumarPartidaJugada()
    {
        partidasJugadas++;
        SecurePrefs.SetInt("Progreso_PartidasJugadas", partidasJugadas);
        SecurePrefs.Save();

        if (partidasJugadas >= 50)
        {
            DesbloquearLogro(TipoLogro.AgujeroNegro);
        }
    }

    public void SumarMuerte()
    {
        muertesTotales++;
        SecurePrefs.SetInt("Progreso_MuertesTotales", muertesTotales);
        SecurePrefs.Save();

        if (muertesTotales >= 100)
        {
            DesbloquearLogro(TipoLogro.GatoEstrellado);
        }
    }
}

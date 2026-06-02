using UnityEngine;
using GoogleMobileAds.Api;
using System;

/// <summary>
/// Gestiona la solicitud, precarga y visualización de recursos publicitarios de AdMob,
/// incluyendo bloques intersticiales y recompensados, además de coordinar la lógica de retorno.
/// </summary>
public class PublicidadManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Instancia estática única para facilitar la invocación global de los servicios de anuncios.
    /// </summary>
    public static PublicidadManager Instancia;

    /// <summary>
    /// Evento estático que se dispara al visualizar completamente un anuncio recompensado y recibir la recompensa.
    /// </summary>
    public static event Action OnAnuncioRecompensadoCompletado;

    [Header("IDs de Prueba de AdMob")]
    private string idBanner = "ca-app-pub-3940256099942544/6300978111";
    private string idIntersticial = "ca-app-pub-3940256099942544/1033173712";
    private string idRecompensado = "ca-app-pub-3940256099942544/5224354917";

    private BannerView anuncioBanner;
    private InterstitialAd anuncioIntersticial;
    private RewardedAd anuncioRecompensado;
    private bool reanudarJuego = false;
    private bool volverAlMenu = false;

    // -----------------------------------------------------------------------------
    // MÉTODOS DE INICIALIZACIÓN
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Establece la instancia Singleton persistente a través de la ejecución del programa.
    /// </summary>
    void Awake()
    {
        if (Instancia == null) { Instancia = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// Realiza la inicialización de la API de Google Mobile Ads y despacha de manera asíncrona 
    /// las primeras solicitudes de carga en segundo plano.
    /// </summary>
    void Start()
    {
        MobileAds.Initialize((InitializationStatus estado) => {
            Debug.Log("SDK de Google Mobile Ads inicializado exitosamente.");
            
            CargarBanner();
            CargarIntersticial();
            CargarAnuncioRecompensado();
        });
    }

    /// <summary>
    /// Monitorea el indicador de reanudación del ciclo de la aplicación debido 
    /// al cierre asíncrono de un contenedor publicitario.
    /// </summary>
    void Update()
    {
        if (reanudarJuego)
        {
            reanudarJuego = false; 
            
            if (GameManager.Instancia != null) 
            {
                GameManager.Instancia.ContinuarPartida();
            }
        }

        if (volverAlMenu)
        {
            volverAlMenu = false;
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.MostrarMenuPrincipal();
            }
        }
    }

    // -----------------------------------------------------------------------------
    // MÉTODOS DEL ANUNCIO INTERSTICIAL (GAME OVER)
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Construye y envía una petición para la obtención de un recurso de publicidad a pantalla completa.
    /// Suscribe el evento de cierre a la recarga y a la reactivación de la partida.
    /// </summary>
    private void CargarIntersticial()
    {
        if (anuncioIntersticial != null) { anuncioIntersticial.Destroy(); anuncioIntersticial = null; }

        AdRequest peticion = new AdRequest();
        InterstitialAd.Load(idIntersticial, peticion, (InterstitialAd anuncio, LoadAdError error) =>
        {
            if (error != null || anuncio == null) { Debug.LogError("Anomalía al obtener bloque intersticial: " + error); return; }
            
            anuncioIntersticial = anuncio;
            Debug.Log("Bloque intersticial obtenido y cacheado en memoria.");
            
            anuncioIntersticial.OnAdFullScreenContentClosed += () => {
                CargarIntersticial();
                reanudarJuego = true;
            };
        });
    }

    /// <summary>
    /// Verifica la disponibilidad del recurso precargado y lo despliega en la capa principal.
    /// Informa a la plataforma analítica si el despliegue es exitoso.
    /// </summary>
    public void MostrarIntersticial()
    {
        if (anuncioIntersticial != null && anuncioIntersticial.CanShowAd())
        {
            anuncioIntersticial.Show();
            if(AnalyticsManager.Instancia != null) AnalyticsManager.Instancia.RegistrarEventoSimple("vio_anuncio_gameover");
        }
        else
        {
            Debug.LogWarning("El bloque intersticial no se encontró disponible en caché.");
        }
    }

    // -----------------------------------------------------------------------------
    // MÉTODOS DEL ANUNCIO RECOMPENSADO (20 GEMAS)
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Gestiona la petición asíncrona de un material audiovisual bonificado
    /// preparándolo para su eventual reproducción a demanda.
    /// </summary>
    private void CargarAnuncioRecompensado()
    {
        if (anuncioRecompensado != null) { anuncioRecompensado.Destroy(); anuncioRecompensado = null; }

        AdRequest peticion = new AdRequest();
        RewardedAd.Load(idRecompensado, peticion, (RewardedAd anuncio, LoadAdError error) =>
        {
            if (error != null || anuncio == null) { Debug.LogError("Anomalía al obtener bloque bonificado: " + error); return; }
            
            anuncioRecompensado = anuncio;
            Debug.Log("Bloque de video bonificado cacheado en memoria.");
            
            anuncioRecompensado.OnAdFullScreenContentClosed += () => { 
                CargarAnuncioRecompensado(); 
                volverAlMenu = true; // Se procesará en el hilo principal (Update)
            };
        });
    }

    /// <summary>
    /// Ejecuta la reproducción del material en formato recompensa y define
    /// el delegado anónimo a ser activado tras la finalización completa de la visualización.
    /// </summary>
    public void MostrarAnuncioRecompensado()
    {
        if (anuncioRecompensado != null && anuncioRecompensado.CanShowAd())
        {
            anuncioRecompensado.Show((Reward recompensa) => 
            {
                DarRecompensaGemas();
            });
        }
        else
        {
            Debug.LogWarning("El material audiovisual bonificado no se encontraba preparado.");
        }
    }

    /// <summary>
    /// Incrementa las reservas de divisa virtual del cliente en respuesta a
    /// la confirmación de retención proveída por la API publicitaria, y sincroniza la transacción en línea.
    /// </summary>
    private void DarRecompensaGemas()
    {
        int gemasActuales = SecurePrefs.GetInt("GemasLocales", 0);
        
        gemasActuales += 20;
        SecurePrefs.SetInt("GemasLocales", gemasActuales);
        SecurePrefs.Save();

        if (DatabaseManager.Instancia != null && Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            string idUsuario = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            DatabaseManager.Instancia.GuardarGemasEnNube(idUsuario, gemasActuales);
        }

        if (AnalyticsManager.Instancia != null) AnalyticsManager.Instancia.RegistrarEventoSimple("recompensa_20gemas_completada");

        if (LogrosManager.Instancia != null)
        {
            LogrosManager.Instancia.DesbloquearLogro(TipoLogro.Patrocinador);
        }

        Debug.Log("Balance de activos virtuales modificado. Crédito vigente: " + gemasActuales);

        // Notifica a los scripts interesados (como el temporizador del botón)
        OnAnuncioRecompensadoCompletado?.Invoke();
    }

    // -----------------------------------------------------------------------------
    // MÉTODOS DEL ANUNCIO BANNER (ESTÁTICO)
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Crea y carga un banner publicitario en la parte superior central de la pantalla.
    /// Se configura por defecto para mostrarse de inmediato tras la carga inicial.
    /// </summary>
    private void CargarBanner()
    {
        if (anuncioBanner != null) { anuncioBanner.Destroy(); anuncioBanner = null; }

        // Creamos el banner en la posición Top (superior centro) que es ideal para landscape
        anuncioBanner = new BannerView(idBanner, AdSize.Banner, AdPosition.Top);

        AdRequest peticion = new AdRequest();
        anuncioBanner.LoadAd(peticion);
        
        Debug.Log("Banner de AdMob solicitado y cargando...");
    }

    /// <summary>
    /// Hace visible el banner en la pantalla.
    /// </summary>
    public void MostrarBanner()
    {
        if (anuncioBanner != null)
        {
            anuncioBanner.Show();
            Debug.Log("Banner publicitario desplegado en pantalla.");
        }
    }

    /// <summary>
    /// Oculta el banner de la pantalla para evitar colisiones visuales o interrupciones.
    /// </summary>
    public void OcultarBanner()
    {
        if (anuncioBanner != null)
        {
            anuncioBanner.Hide();
            Debug.Log("Banner publicitario ocultado de la pantalla.");
        }
    }

    /// <summary>
    /// Destruye el banner para liberar los recursos de memoria.
    /// </summary>
    public void DestruirBanner()
    {
        if (anuncioBanner != null)
        {
            anuncioBanner.Destroy();
            anuncioBanner = null;
            Debug.Log("Banner publicitario destruido de memoria.");
        }
    }

    private void OnDestroy()
    {
        DestruirBanner();
    }
}
using UnityEngine;
using GoogleMobileAds.Api;
using System;

/// <summary>
/// Gestiona la publicidad en el juego (AdMob):
/// 1. Anuncio Largo (Recompensado - 50 Gemas) -> MostrarAnuncio50Gemas()
/// 2. Anuncio Corto (Intersticial - Game Over) -> MostrarAnuncioGameOver()
/// 3. Banners de publicidad -> MostrarBanner() / OcultarBanner()
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

    [Header("IDs de AdMob (IDs de Prueba)")]
    public string idBanner = "ca-app-pub-3940256099942544/6300978111";
    public string idIntersticial = "ca-app-pub-3940256099942544/1033173712";
    public string idRecompensado = "ca-app-pub-3940256099942544/5224354917";

    private BannerView anuncioBanner;
    private InterstitialAd anuncioIntersticial;
    private RewardedAd anuncioRecompensado;
    private bool reanudarJuego = false;
    private bool mantenerMenuPausado = false;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------
    ///<sumary>
    /// Awake: Crea una instancia única del objeto y evita que se destruya al cambiar de escena.
    ///</sumary>
    private void Awake()
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
    /// Realiza la inicialización de la API de Google Mobile Ads y despacha de manera asíncrona 
    /// las primeras solicitudes de carga en segundo plano.
    /// </summary>
    private void Start()
    {
        MobileAds.Initialize((InitializationStatus estado) => {
            Debug.Log("SDK de Google Mobile Ads inicializado.");
            CargarBanner();
            CargarIntersticial();
            CargarAnuncioRecompensado();
        });
    }

    /// <summary>
    /// Monitorea los indicadores de eventos asíncronos de publicidad para
    /// ejecutarlos de forma segura en el hilo principal de Unity (Update).
    /// </summary>
    private void Update()
    {
        if (reanudarJuego)
        {
            reanudarJuego = false; 
            if (GameManager.Instancia != null) 
            {
                // Solo continuar la partida si se encuentra estrictamente en la pantalla de Game Over
                if (GameManager.Instancia.panelGameOver != null && GameManager.Instancia.panelGameOver.activeSelf)
                {
                    GameManager.Instancia.PrepararContinuacionPartida();
                }
                else
                {
                    Debug.LogWarning("[PublicidadManager] Intento de reanudación ignorado porque no nos encontramos en la pantalla de Game Over.");
                }
            }
        }

        if (mantenerMenuPausado)
        {
            mantenerMenuPausado = false;
            // Garantiza que tras el anuncio de 50 gemas el juego permanezca pausado y en el Menú Principal
            if (GameManager.Instancia != null && GameManager.Instancia.panelMenuPrincipal != null && GameManager.Instancia.panelMenuPrincipal.activeSelf)
            {
                Time.timeScale = 0f;
            }
        }
    }

    // =========================================================================
    // 1. ANUNCIO CORTO (INTERSTICIAL - GAME OVER)
    // =========================================================================

    private void CargarIntersticial()
    {
        if (anuncioIntersticial != null) { anuncioIntersticial.Destroy(); anuncioIntersticial = null; }

        string idFinal = string.IsNullOrEmpty(idIntersticial) ? "" : idIntersticial.Trim();
        AdRequest peticion = new AdRequest();
        InterstitialAd.Load(idFinal, peticion, (InterstitialAd anuncio, LoadAdError error) =>
        {
            if (error != null || anuncio == null)
            {
                Debug.LogError("Error al cargar anuncio corto intersticial: " + error);
                return;
            }
            
            anuncioIntersticial = anuncio;
            
            anuncioIntersticial.OnAdFullScreenContentClosed += () => {
                CargarIntersticial();
                // Solo activa reanudarJuego si esta en el panel de Game Over
                if (GameManager.Instancia != null && GameManager.Instancia.panelGameOver != null && GameManager.Instancia.panelGameOver.activeSelf)
                {
                    reanudarJuego = true;
                }
            };
        });
    }

    /// <summary>
    /// Usar este método en el botón de Game Over / Continuar (Anuncio Corto ~5s)
    /// </summary>
    public void MostrarAnuncioGameOver()
    {
        MostrarIntersticial();
    }

    /// <summary>
    /// Método principal para mostrar el anuncio corto de Game Over.
    /// </summary>
    public void MostrarIntersticial()
    {
        if (anuncioIntersticial != null && anuncioIntersticial.CanShowAd())
        {
            anuncioIntersticial.Show();
            if (AnalyticsManager.Instancia != null) 
                AnalyticsManager.Instancia.RegistrarEventoSimple("vio_anuncio_gameover");
        }
        else
        {
            Debug.LogWarning("Anuncio corto no listo.");
            // Solo continuar partida directamente si el panel de Game Over está activo
            if (GameManager.Instancia != null && GameManager.Instancia.panelGameOver != null && GameManager.Instancia.panelGameOver.activeSelf) 
            {
                Debug.LogWarning("Continuando la partida directamente desde Game Over...");
                GameManager.Instancia.PrepararContinuacionPartida();
            }
        }
    }

    // =========================================================================
    // 2. ANUNCIO LARGO (RECOMPENSADO - 50 GEMAS)
    // =========================================================================

    /// <summary>
    /// Carga un nuevo anuncio recompensado.
    /// </summary>
    private void CargarAnuncioRecompensado()
    {
        if (anuncioRecompensado != null) { anuncioRecompensado.Destroy(); anuncioRecompensado = null; }

        string idFinal = string.IsNullOrEmpty(idRecompensado) ? "" : idRecompensado.Trim();
        AdRequest peticion = new AdRequest();
        RewardedAd.Load(idFinal, peticion, (RewardedAd anuncio, LoadAdError error) =>
        {
            if (error != null || anuncio == null)
            {
                Debug.LogError("Error al cargar anuncio recompensado largo: " + error);
                return;
            }
            
            anuncioRecompensado = anuncio;
            
            anuncioRecompensado.OnAdFullScreenContentClosed += () => { 
                CargarAnuncioRecompensado(); 
                mantenerMenuPausado = true; // Se procesará de forma segura en el hilo principal (Update)
            };
        });
    }

    /// <summary>
    /// Método para mostrar el anuncio largo con recompensa de 50 gemas en el Menú Principal.
    /// </summary>
    public void MostrarAnuncio50Gemas()
    {
        MostrarAnuncioRecompensado();
    }

    /// <summary>
    /// Mantiene compatibilidad hacia atrás con referencias previas.
    /// </summary>
    [System.Obsolete("Usar MostrarAnuncio50Gemas en su lugar.")]
    public void MostrarAnuncio20Gemas()
    {
        MostrarAnuncio50Gemas();
    }

    /// <summary>
    /// Método principal para mostrar el anuncio largo con recompensa de 50 gemas: MostrarAnuncio50Gemas().
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
            Debug.LogWarning("El anuncio recompensado de gemas no se encuentra listo.");
        }
    }

    private void DarRecompensaGemas()
    {
        int gemasActuales = SecurePrefs.GetInt("GemasLocales", 0);
        gemasActuales += 50;
        SecurePrefs.SetInt("GemasLocales", gemasActuales);
        SecurePrefs.Save();

        if (DatabaseManager.Instancia != null && Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            string idUsuario = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            DatabaseManager.Instancia.GuardarGemasEnNube(idUsuario, gemasActuales);
        }

        if (AnalyticsManager.Instancia != null) 
            AnalyticsManager.Instancia.RegistrarEventoSimple("recompensa_50gemas_completada");

        if (LogrosManager.Instancia != null) 
            LogrosManager.Instancia.DesbloquearLogro(TipoLogro.Patrocinador);

        if (RetosDiariosManager.Instancia != null)
            RetosDiariosManager.Instancia.RegistrarAnuncioVisto();

        Debug.Log("🎉 Recompensa otorgada: +50 Gemas. Total actual: " + gemasActuales);

        mantenerMenuPausado = true;

        // Dispara el evento para que el Temporizador del botón inicie su cooldown en el menú
        OnAnuncioRecompensadoCompletado?.Invoke();
    }

    // =========================================================================
    // 3. BANNER PUBLICITARIO
    // =========================================================================

    private void CargarBanner()
    {
        if (anuncioBanner != null) { anuncioBanner.Destroy(); anuncioBanner = null; }

        string idFinal = string.IsNullOrEmpty(idBanner) ? "" : idBanner.Trim();
        anuncioBanner = new BannerView(idFinal, AdSize.Banner, AdPosition.Top);
        AdRequest peticion = new AdRequest();
        anuncioBanner.LoadAd(peticion);
    }

    public void MostrarBanner()
    {
        if (anuncioBanner != null) anuncioBanner.Show();
    }

    public void OcultarBanner()
    {
        if (anuncioBanner != null) anuncioBanner.Hide();
    }

    public void DestruirBanner()
    {
        if (anuncioBanner != null)
        {
            anuncioBanner.Destroy();
            anuncioBanner = null;
        }
    }

    private void OnDestroy()
    {
        DestruirBanner();
    }
}
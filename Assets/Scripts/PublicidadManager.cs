using UnityEngine;
using GoogleMobileAds.Api;
using System;

/**
* CLASE PUBLICIDAD MANAGER:
* Gestiona la publicidad de AdMob.
* Controla el anuncio Intersticial del Game Over y el Vídeo Recompensado del menú.
*/
public class PublicidadManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    public static PublicidadManager Instancia;

    [Header("IDs de Prueba de AdMob")]
    // IMPORTANTE: Estos son IDs de prueba de Google. CAMBIAR POR LOS MÍOS al publicar.
    private string idIntersticial = "ca-app-pub-3940256099942544/1033173712";
    private string idRecompensado = "ca-app-pub-3940256099942544/5224354917";

    private InterstitialAd anuncioIntersticial;
    private RewardedAd anuncioRecompensado;
    private bool reanudarJuego = false;

    // -----------------------------------------------------------------------------
    // MÉTODOS DE INICIALIZACIÓN
    // -----------------------------------------------------------------------------
    void Awake()
    {
        if (Instancia == null) { Instancia = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // 'InitializationStatus' explícito para evitar fallos de compilación
        MobileAds.Initialize((InitializationStatus estado) => {
            Debug.Log("AdMob: Motor inicializado correctamente.");
            
            // Pre-carga los anuncios en la sombra para que no haya tiempos de carga al jugar
            CargarIntersticial();
            CargarAnuncioRecompensado();
        });
    }

    /*
    * Método Update():
    * Vigila la variable booleana reanudarJuego para reanudar el juego después de ver el anuncio.
    */
    void Update()
    {
        // Unity vigila este interruptor en cada fotograma. 
        if (reanudarJuego)
        {
            reanudarJuego = false; // Se apaga al instante
            
            if (GameManager.Instancia != null) 
            {
                GameManager.Instancia.ContinuarPartida();
            }
        }
    }

    // -----------------------------------------------------------------------------
    // MÉTODOS DEL ANUNCIO INTERSTICIAL (GAME OVER)
    // -----------------------------------------------------------------------------
    
    /*
    * Método CargarIntersticial():
    * Pide a Google que descargue un anuncio de pantalla completa.
    */
    private void CargarIntersticial()
    {
        if (anuncioIntersticial != null) { anuncioIntersticial.Destroy(); anuncioIntersticial = null; }

        AdRequest peticion = new AdRequest();
        InterstitialAd.Load(idIntersticial, peticion, (InterstitialAd anuncio, LoadAdError error) =>
        {
            if (error != null || anuncio == null) { Debug.LogError("Error al cargar intersticial: " + error); return; }
            
            anuncioIntersticial = anuncio;
            Debug.Log("AdMob: Anuncio Intersticial cargado y listo.");
            
            // Cuando se cierre el anuncio, se carga otro nuevo para la siguiente partida y se reanuda el juego
            anuncioIntersticial.OnAdFullScreenContentClosed += () => {
                CargarIntersticial();
                reanudarJuego = true;
            };
        });
    }

    /*
    * Método MostrarIntersticial():
    * Este es el método que llamarás desde el Botón "Continuar" del Game Over.
    */
    public void MostrarIntersticial()
    {
        if (anuncioIntersticial != null && anuncioIntersticial.CanShowAd())
        {
            anuncioIntersticial.Show();
            // FUNNEL MONETIZACIÓN: Registra que ha visto un anuncio
            if(AnalyticsManager.Instancia != null) AnalyticsManager.Instancia.RegistrarEventoSimple("vio_anuncio_gameover");
        }
        else
        {
            Debug.LogWarning("AdMob: El anuncio intersticial no estaba listo.");
        }
    }

    // -----------------------------------------------------------------------------
    // MÉTODOS DEL ANUNCIO RECOMPENSADO (20 GEMAS)
    // -----------------------------------------------------------------------------
    
    /*
    * Método CargarAnuncioRecompensado():
    * Pide a Google que descargue un vídeo con recompensa.
    */
    private void CargarAnuncioRecompensado()
    {
        if (anuncioRecompensado != null) { anuncioRecompensado.Destroy(); anuncioRecompensado = null; }

        AdRequest peticion = new AdRequest();
        RewardedAd.Load(idRecompensado, peticion, (RewardedAd anuncio, LoadAdError error) =>
        {
            if (error != null || anuncio == null) { Debug.LogError("Error al cargar recompensado: " + error); return; }
            
            anuncioRecompensado = anuncio;
            Debug.Log("AdMob: Vídeo recompensado cargado y listo.");
            
            anuncioRecompensado.OnAdFullScreenContentClosed += () => { CargarAnuncioRecompensado(); };
        });
    }

    /*
    * Método MostrarAnuncioRecompensado():
    * Este es el método que llamará desde el botón de ver anuncio del Menú Principal.
    */
    public void MostrarAnuncioRecompensado()
    {
        if (anuncioRecompensado != null && anuncioRecompensado.CanShowAd())
        {
            // Muestra el anuncio y le dice qué método ejecutar si el jugador lo ve entero
            anuncioRecompensado.Show((Reward recompensa) => 
            {
                DarRecompensaGemas();
            });
        }
        else
        {
            Debug.LogWarning("AdMob: El vídeo de recompensa no estaba listo.");
        }
    }

    /*
    * Método DarRecompensaGemas():
    * Se ejecuta automáticamente cuando el jugador termina de ver el vídeo.
    */
    private void DarRecompensaGemas()
    {
        Debug.Log("¡Vídeo completado! Entregando 20 gemas...");

        // Lee las gemas actuales
        int gemasActuales = PlayerPrefs.GetInt("GemasLocales", 0);
        
        // CORRECCIÓN: Sumamos 20 gemas para coincidir con tu configuración en AdMob
        gemasActuales += 20;
        PlayerPrefs.SetInt("GemasLocales", gemasActuales);
        PlayerPrefs.Save();

        // Sube las nuevas gemas a Firebase Firestore
        if (DatabaseManager.Instancia != null && Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            string idUsuario = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            DatabaseManager.Instancia.GuardarGemasEnNube(idUsuario, gemasActuales);
        }

        // FUNNEL MONETIZACIÓN: Registra que vio el anuncio
        if(AnalyticsManager.Instancia != null) AnalyticsManager.Instancia.RegistrarEventoSimple("recompensa_20gemas_completada");

        Debug.Log("Gemas actualizadas. Total actual: " + gemasActuales);
    }
}
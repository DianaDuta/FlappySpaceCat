using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Representa la estructura de un idioma disponible en el sistema de localización.
/// </summary>
[System.Serializable]
public class InfoIdioma
{
    public string codigo;
    public string nombre;
}

/// <summary>
/// Gestor central de localización e internacionalización.
/// Carga las traducciones locales desde Assets/Resources/LocalizationData.json,
/// detecta automáticamente el idioma del dispositivo y permite cambiar de idioma en tiempo real sin recargar la escena.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager instancia;
    public static LocalizationManager Instancia
    {
        get
        {
            if (instancia == null)
            {
                instancia = FindAnyObjectByType<LocalizationManager>();
                if (instancia == null)
                {
                    GameObject go = new GameObject("LocalizationManager");
                    instancia = go.AddComponent<LocalizationManager>();
                    DontDestroyOnLoad(go);
                    instancia.Inicializar();
                }
            }
            return instancia;
        }
    }

    /// <summary>
    /// Evento estático que notifica a todos los componentes de UI cuando el jugador cambia de idioma.
    /// </summary>
    public static event Action OnLanguageChanged;

    [Header("Idioma Actual")]
    [SerializeField] private string idiomaActual = "es";

    public string IdiomaActual => idiomaActual;

    private Dictionary<string, Dictionary<string, string>> diccionarioTraducciones = new Dictionary<string, Dictionary<string, string>>();
    private List<InfoIdioma> listaIdiomas = new List<InfoIdioma>();

    public List<InfoIdioma> IdiomasDisponibles
    {
        get
        {
            if (listaIdiomas.Count == 0) Inicializar();
            return listaIdiomas;
        }
    }

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
            Inicializar();
        }
        else if (instancia != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Inicializar();
        TraducirTodaLaEscena();
    }

    /// <summary>
    /// Asegura que la base de datos de traducciones y el idioma inicial estén cargados.
    /// </summary>
    public void Inicializar()
    {
        if (diccionarioTraducciones.Count == 0)
        {
            CargarBaseDatosTraducciones();
            InicializarIdioma();
        }
    }

    /// <summary>
    /// Carga y parsea el archivo JSON de traducciones desde Resources.
    /// </summary>
    private void CargarBaseDatosTraducciones()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("LocalizationData");
        if (jsonAsset == null)
        {
            Debug.LogError("❌ [LocalizationManager] No se encontró el archivo LocalizationData.json en Assets/Resources.");
            return;
        }

        try
        {
            // Parseo manual ligero y robusto compatible con cualquier versión de Unity
            diccionarioTraducciones.Clear();
            listaIdiomas.Clear();

            // Idiomas soportados
            listaIdiomas.Add(new InfoIdioma { codigo = "es", nombre = "Castellano" });
            listaIdiomas.Add(new InfoIdioma { codigo = "en", nombre = "English" });
            listaIdiomas.Add(new InfoIdioma { codigo = "fr", nombre = "Français" });
            listaIdiomas.Add(new InfoIdioma { codigo = "de", nombre = "Deutsch" });
            listaIdiomas.Add(new InfoIdioma { codigo = "it", nombre = "Italiano" });
            listaIdiomas.Add(new InfoIdioma { codigo = "pt", nombre = "Português" });
            listaIdiomas.Add(new InfoIdioma { codigo = "ro", nombre = "Română" });

            // Parsear el JSON
            string json = jsonAsset.text;
            ParsearJsonTraducciones(json);
            
            Debug.Log($"🌐 [LocalizationManager] Base de datos de idiomas cargada con {diccionarioTraducciones.Count} claves.");
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ [LocalizationManager] Error al parsear LocalizationData.json: " + ex.Message);
        }
    }

    /// <summary>
    /// Extrae las claves y traducciones del JSON a memoria.
    /// </summary>
    private void ParsearJsonTraducciones(string json)
    {
        // Enfoque simplificado para diccionarios anidados
        string seccionTraducciones = json;
        int idxTraducciones = json.IndexOf("\"traducciones\"");
        if (idxTraducciones >= 0)
        {
            seccionTraducciones = json.Substring(idxTraducciones);
        }

        string[] lineas = seccionTraducciones.Split('\n');
        string claveActual = "";

        string[] codigosIdiomas = new string[] { "es", "en", "fr", "de", "it", "pt", "ro" };

        foreach (string linea in lineas)
        {
            string l = linea.Trim();
            if (string.IsNullOrEmpty(l) || l == "{" || l == "}" || l == "}," || l == "],") continue;

            // Detectar inicio de clave: "menu_jugar": {
            if (l.EndsWith("{"))
            {
                int firstQuote = l.IndexOf('"');
                int secondQuote = l.IndexOf('"', firstQuote + 1);
                if (firstQuote >= 0 && secondQuote > firstQuote)
                {
                    claveActual = l.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                    if (!diccionarioTraducciones.ContainsKey(claveActual))
                    {
                        diccionarioTraducciones[claveActual] = new Dictionary<string, string>();
                    }
                }
            }
            else if (!string.IsNullOrEmpty(claveActual))
            {
                // Detectar traducción: "es": "JUGAR",
                foreach (string cod in codigosIdiomas)
                {
                    string prefijo = $"\"{cod}\":";
                    if (l.StartsWith(prefijo))
                    {
                        int inicioValor = l.IndexOf('"', prefijo.Length);
                        int finValor = l.LastIndexOf('"');
                        if (inicioValor >= 0 && finValor > inicioValor)
                        {
                            string valor = l.Substring(inicioValor + 1, finValor - inicioValor - 1);
                            // Desescapar comillas
                            valor = valor.Replace("\\\"", "\"");
                            diccionarioTraducciones[claveActual][cod] = valor;
                        }
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Determina el idioma inicial: busca la preferencia guardada o detecta el idioma del móvil.
    /// </summary>
    private void InicializarIdioma()
    {
        string guardado = SecurePrefs.GetString("IdiomaSeleccionado", "");
        if (!string.IsNullOrEmpty(guardado) && EsIdiomaValido(guardado))
        {
            idiomaActual = guardado;
        }
        else
        {
            // Auto-detección del idioma del sistema operativo
            idiomaActual = DetectarIdiomaSistema();
            SecurePrefs.SetString("IdiomaSeleccionado", idiomaActual);
            SecurePrefs.Save();
        }

        Debug.Log($"🌐 [LocalizationManager] Idioma activo establecido: {idiomaActual.ToUpper()}");
    }

    /// <summary>
    /// Mapea el SystemLanguage de Unity al código ISO de 2 letras.
    /// </summary>
    private string DetectarIdiomaSistema()
    {
        SystemLanguage lang = Application.systemLanguage;
        switch (lang)
        {
            case SystemLanguage.English: return "en";
            case SystemLanguage.French: return "fr";
            case SystemLanguage.German: return "de";
            case SystemLanguage.Italian: return "it";
            case SystemLanguage.Portuguese: return "pt";
            case SystemLanguage.Romanian: return "ro";
            case SystemLanguage.Spanish: return "es";
            default: return "es"; // Idioma por defecto
        }
    }

    private bool EsIdiomaValido(string codigo)
    {
        return listaIdiomas.Exists(i => i.codigo == codigo);
    }

    /// <summary>
    /// Cambia el idioma activo en tiempo real, lo guarda en persistencia y notifica a toda la UI.
    /// </summary>
    /// <param name="nuevoCodigoIdioma">Código de idioma ('es', 'en', 'fr', 'de', 'it', 'pt', 'ro')</param>
    public void CambiarIdioma(string nuevoCodigoIdioma)
    {
        if (!EsIdiomaValido(nuevoCodigoIdioma))
        {
            Debug.LogWarning($"⚠️ [LocalizationManager] Idioma '{nuevoCodigoIdioma}' no soportado.");
            return;
        }

        idiomaActual = nuevoCodigoIdioma;
        SecurePrefs.SetString("IdiomaSeleccionado", idiomaActual);
        SecurePrefs.Save();

        Debug.Log($"🌐 [LocalizationManager] Idioma cambiado exitosamente a: {idiomaActual.ToUpper()}");

        // 1. Disparar evento para scripts suscritos
        OnLanguageChanged?.Invoke();

        // 2. Escanear y traducir automáticamente toda la interfaz de la escena
        TraducirTodaLaEscena();
    }

    /// <summary>
    /// Escanea automáticamente todos los textos de la escena y los traduce según su nombre o su clave.
    /// Funciona en tiempo real tanto para objetos con TextoTraducible como para cualquier texto estándar.
    /// </summary>
    public void TraducirTodaLaEscena()
    {
        // 1. Componentes TextoTraducible explícitos (activos e inactivos)
        TextoTraducible[] componentes = Resources.FindObjectsOfTypeAll<TextoTraducible>();
        foreach (var comp in componentes)
        {
            if (comp != null && comp.gameObject.scene.name != null)
            {
                comp.ActualizarTexto();
            }
        }

        // 2. Auto-detección para todos los TextMeshProUGUI de la escena
        TextMeshProUGUI[] todosTMP = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var tmp in todosTMP)
        {
            if (tmp == null || tmp.gameObject.scene.name == null) continue; // Ignorar prefabs en disco

            // No sobrescribir si ya tiene su propio componente TextoTraducible gestionándolo
            if (tmp.GetComponent<TextoTraducible>() != null) continue;

            string goName = tmp.gameObject.name;
            string parentName = tmp.transform.parent != null ? tmp.transform.parent.name : "";

            string clave = ObtenerClavePorNombre(goName, parentName, tmp.text);
            if (!string.IsNullOrEmpty(clave))
            {
                string traducido = ObtenerTexto(clave);
                if (!string.IsNullOrEmpty(traducido))
                {
                    tmp.text = traducido;
                }
            }
        }
    }

    /// <summary>
    /// Infiere inteligentemente la clave de traducción basada en el nombre del GameObject o el texto.
    /// </summary>
    public string ObtenerClavePorNombre(string goName, string parentName, string currentText)
    {
        string txtLimpio = currentText != null ? currentText.Trim() : "";

        // Menú Opciones
        if (goName == "Txt_Ajustes" || goName == "Titulo_Opciones" || parentName == "Ajustes" || txtLimpio == "Ajustes" || txtLimpio == "Settings" || txtLimpio == "Paramètres" || txtLimpio == "Einstellungen" || txtLimpio == "Impostazioni" || txtLimpio == "Setări") return "opciones_titulo";
        if (goName == "Txt_Musica" || parentName == "Musica" || txtLimpio == "Musica" || txtLimpio == "Music" || txtLimpio == "Musique" || txtLimpio == "Música" || txtLimpio == "Muzică") return "opciones_musica";
        if (goName == "Txt_Efectos" || parentName == "Efectos" || txtLimpio == "Efectos" || txtLimpio == "Sound FX" || txtLimpio == "Effets Sonores" || txtLimpio == "Soundeffekte" || txtLimpio == "Effetti" || txtLimpio == "Efeitos" || txtLimpio == "Efecte") return "opciones_efectos";
        if (goName == "Txt_Idiomas" || parentName == "Idiomas" || txtLimpio == "Idioma" || txtLimpio == "Language" || txtLimpio == "Langue" || txtLimpio == "Sprache" || txtLimpio == "Lingua" || txtLimpio == "Limbă") return "opciones_idioma";
        if (goName == "Toggle_Vibracion" || parentName == "Toggle_Vibracion" || txtLimpio == "Vibracion" || txtLimpio == "Vibration" || txtLimpio == "Vibrazione" || txtLimpio == "Vibração" || txtLimpio == "Vibrație") return "opciones_vibracion";

        // Botones de Opciones (Privacidad, Términos, Feedback, Calificar)
        string gLow = goName.ToLower();
        string pLow = parentName.ToLower();
        string tLow = txtLimpio.ToLower();

        if (gLow.Contains("privacidad") || pLow.Contains("privacidad") || gLow.Contains("privacy") || pLow.Contains("privacy") || tLow.Contains("privacidad") || tLow.Contains("privacy") || tLow.Contains("confidentialit") || tLow.Contains("datenschutz")) 
            return "opciones_privacidad";

        if (gLow.Contains("termino") || pLow.Contains("termino") || gLow.Contains("term") || pLow.Contains("term") || tLow.Contains("términos") || tLow.Contains("terminos") || tLow.Contains("terms") || tLow.Contains("condition") || tLow.Contains("agb") || tLow.Contains("nutzungsbed")) 
            return "opciones_terminos";

        if (gLow.Contains("feedback") || pLow.Contains("feedback") || gLow.Contains("soporte") || pLow.Contains("soporte") || gLow.Contains("comentario") || pLow.Contains("comentario") || tLow.Contains("comentario") || tLow.Contains("feedback") || tLow.Contains("soporte") || tLow.Contains("support")) 
            return "opciones_feedback";

        if (gLow.Contains("calificar") || pLow.Contains("calificar") || gLow.Contains("rate") || pLow.Contains("rate") || tLow == "calificar" || tLow == "rate us" || tLow == "noter" || tLow == "bewerten" || tLow == "valuta" || tLow == "avaliar" || tLow == "evaluează") 
            return "opciones_calificar";

        // Menú Principal
        if (goName == "Btn_Jugar" || parentName == "Btn_Jugar" || txtLimpio == "Jugar" || txtLimpio == "Play" || txtLimpio == "Jouer" || txtLimpio == "Spielen" || txtLimpio == "Gioca" || txtLimpio == "Jogar" || txtLimpio == "Joacă") return "menu_jugar";
        if (goName == "Btn_Tienda" || parentName == "Btn_Tienda" || txtLimpio == "Tienda" || txtLimpio == "Shop" || txtLimpio == "Boutique" || txtLimpio == "Negozio" || txtLimpio == "Loja" || txtLimpio == "Magazin") return "menu_tienda";
        if (goName == "Btn_Perfil" || parentName == "Btn_Perfil" || txtLimpio == "Perfil" || txtLimpio == "Profile" || txtLimpio == "Profil" || txtLimpio == "Profilo") return "menu_perfil";
        if (goName == "Btn_Logros" || parentName == "Btn_Logros" || txtLimpio == "Logros" || txtLimpio == "Achievements" || txtLimpio == "Succès" || txtLimpio == "Erfolge" || txtLimpio == "Obiettivi" || txtLimpio == "Conquistas" || txtLimpio == "Realizări") return "menu_logros";
        if (goName == "Btn_Salir_App" || parentName == "Btn_Salir_App" || txtLimpio == "Salir del juego" || txtLimpio == "Exit game" || txtLimpio == "Quitter le jeu" || txtLimpio == "Spiel beenden" || txtLimpio == "Esci dal gioco" || txtLimpio == "Sair do jogo" || txtLimpio == "Ieșire din joc") return "menu_salir_app";

        // Inicio de Sesión
        if (goName == "Txt_Titulo_Inicio") return "login_titulo";
        if (goName == "Txt_Subtitulo_InicioSesion") return "login_subtitulo";
        if (goName == "Txt_Cowsmo") return "login_cowsmo";
        if (goName == "Btn_Google" || goName == "Txt_Google" || parentName == "Btn_Google") return "login_btn_google";
        if (goName == "Btn_ID" || goName == "Txt_ID" || parentName == "Btn_ID" || txtLimpio == "Crear ID" || txtLimpio == "Create ID") return "login_btn_if";
        if (goName == "Btn_Saltar" || goName == "Txt_Saltar" || parentName == "Btn_Saltar" || txtLimpio == "SALTAR" || txtLimpio == "SKIP") return "login_btn_saltar";

        // Botones de Anuncio y Recompensas (+50 y +20)
        if (goName == "Btn_Anuncio" || goName == "Txt_Anuncio" || parentName == "Btn_Anuncio" || txtLimpio == "+50") return "menu_ver_anuncio";
        if (goName == "Btn_Reclamar" || goName == "Txt_Reclamar" || parentName == "Btn_Reclamar" || txtLimpio == "+20") return "menu_reclamar";

        // Game Over
        if (goName == "Txt_GameOver" || goName == "Titulo_GameOver" || txtLimpio.ToUpper() == "GAME OVER" || txtLimpio == "JOC TERMINAT") return "gameover_titulo";
        if (goName == "Btn_Volver_Al_Menu" || parentName == "Btn_Volver_Al_Menu") return "gameover_volver_menu";

        // Pausa
        if (goName == "Txt_Pausa" || txtLimpio.ToUpper() == "PAUSA" || txtLimpio.ToUpper() == "PAUSE" || txtLimpio.ToUpper() == "PAUZĂ") return "pausa_titulo";

        // Botones comunes
        if (goName == "Btn_Continuar" || goName == "Txt_Continuar" || parentName == "Btn_Continuar" || txtLimpio.StartsWith("Continuar") || txtLimpio.StartsWith("Continue") || txtLimpio.StartsWith("Continuer") || txtLimpio.StartsWith("Weiterspielen") || txtLimpio.StartsWith("Continua") || txtLimpio.StartsWith("Continuă")) return "gameover_continuar";
        if (goName == "Btn_Salir" || parentName == "Btn_Salir" || txtLimpio == "Salir" || txtLimpio == "Exit" || txtLimpio == "Quitter" || txtLimpio == "Beenden" || txtLimpio == "Esci" || txtLimpio == "Sair" || txtLimpio == "Ieșire") return "pausa_salir";
        if (goName == "Btn_Si" || parentName == "Btn_Si" || txtLimpio == "Si" || txtLimpio == "Sí" || txtLimpio == "Yes" || txtLimpio == "Oui" || txtLimpio == "Ja" || txtLimpio == "Sì" || txtLimpio == "Sim" || txtLimpio == "Da") return "salir_si";
        if (goName == "Btn_No" || parentName == "Btn_No" || txtLimpio == "No" || txtLimpio == "Non" || txtLimpio == "Nein" || txtLimpio == "Não" || txtLimpio == "Nu") return "salir_no";

        // Tienda
        if (goName == "Txt_Titulo_Tienda" || goName == "Titulo_Tienda") return "tienda_titulo";

        // Notificaciones de Logro
        if (goName == "Txt_Titulo_Notificacion" || goName == "Txt_Titulo_Logro_Notif") return "notif_logro_titulo";
        if (goName == "Btn_Aceptar_Notificacion" || parentName == "Btn_Aceptar_Notificacion" || txtLimpio == "¡GENIAL!" || txtLimpio == "GENIAL!" || txtLimpio == "AWESOME!") return "notif_btn_aceptar";

        // Perfil
        if (goName == "Btn_InicioSesion" || parentName == "Btn_InicioSesion") return "perfil_iniciar_sesion";
        if (goName == "Btn_Fondo_gemas" || parentName == "Btn_Fondo_gemas" || txtLimpio == "Gemas" || txtLimpio == "Gems") return "perfil_gemas";
        if (goName == "Btn_Fondo_puntuacion" || parentName == "Btn_Fondo_puntuacion") return "perfil_record";

        return "";
    }

    /// <summary>
    /// Avanza al siguiente idioma disponible en la lista (ideal para botones de cambio rápido de idioma).
    /// </summary>
    public void SiguienteIdioma()
    {
        int index = listaIdiomas.FindIndex(i => i.codigo == idiomaActual);
        int siguienteIndex = (index + 1) % listaIdiomas.Count;
        CambiarIdioma(listaIdiomas[siguienteIndex].codigo);
    }

    /// <summary>
    /// Obtiene el texto traducido para una clave en el idioma activo actual.
    /// Si la clave o el idioma no existen, devuelve el texto de respaldo (fallback) o la propia clave.
    /// </summary>
    public string ObtenerTexto(string clave, string fallback = "")
    {
        if (string.IsNullOrEmpty(clave)) return fallback;

        string c = clave.Trim();

        if (diccionarioTraducciones.TryGetValue(c, out Dictionary<string, string> traducciones))
        {
            if (traducciones.TryGetValue(idiomaActual, out string texto) && !string.IsNullOrEmpty(texto)) return texto;
            if (traducciones.TryGetValue("es", out string textoEs)) return textoEs;
            if (traducciones.TryGetValue("en", out string textoEn)) return textoEn;
        }

        return !string.IsNullOrEmpty(fallback) ? fallback : clave;
    }

    /// <summary>
    /// Obtiene el nombre legible del idioma activo actual (ej: "Español", "English", "Română").
    /// </summary>
    public string ObtenerNombreIdiomaActual()
    {
        InfoIdioma info = listaIdiomas.Find(i => i.codigo == idiomaActual);
        return info != null ? info.nombre : "Español";
    }
}

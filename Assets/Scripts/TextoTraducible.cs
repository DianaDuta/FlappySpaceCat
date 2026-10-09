using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Componente ligero para adjuntar a cualquier TextMeshProUGUI o Text de la UI.
/// Actualiza automáticamente su contenido al idioma activo y reacciona en tiempo real
/// cuando el jugador cambia de idioma desde las opciones del juego.
/// </summary>
public class TextoTraducible : MonoBehaviour
{
    [Header("Identificador de Traducción")]
    [Tooltip("Clave exacta correspondiente en LocalizationData.json (ej: 'menu_jugar', 'opciones_musica')")]
    public string claveTraduccion;

    [Header("Formato Opcional")]
    [Tooltip("Texto prefijo o sufijo si es necesario (ej: mayúsculas forzadas).")]
    public bool forzarMayusculas = false;

    private TextMeshProUGUI textoTMP;
    private Text textoLegacy;

    private void Awake()
    {
        textoTMP = GetComponent<TextMeshProUGUI>();
        if (textoTMP == null) textoTMP = GetComponentInChildren<TextMeshProUGUI>();

        textoLegacy = GetComponent<Text>();
        if (textoLegacy == null) textoLegacy = GetComponentInChildren<Text>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += ActualizarTexto;
        ActualizarTexto();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= ActualizarTexto;
    }

    private void Start()
    {
        ActualizarTexto();
    }

    /// <summary>
    /// Consulta el LocalizationManager y aplica la traducción al componente de texto.
    /// </summary>
    public void ActualizarTexto()
    {
        // Los textos de puntuación dinámica del Game Over son gestionados directamente por el GameManager con sus números
        if (claveTraduccion == "gameover_puntuacion" || claveTraduccion == "gameover_record" || 
            gameObject.name == "Txt_Puntuación_Actual" || gameObject.name == "Txt_Puntuación_Record" ||
            gameObject.name == "Txt_Puntuacion_actual" || gameObject.name == "Txt_Puntuacion_GameOver" || 
            gameObject.name == "Txt_MejorPuntuacion_GameOver")
        {
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.ActualizarTextosGameOverPorIdioma();
            }
            return;
        }

        if (textoTMP == null) textoTMP = GetComponent<TextMeshProUGUI>();
        if (textoTMP == null) textoTMP = GetComponentInChildren<TextMeshProUGUI>();

        if (textoLegacy == null) textoLegacy = GetComponent<Text>();
        if (textoLegacy == null) textoLegacy = GetComponentInChildren<Text>();

        if (LocalizationManager.Instancia != null)
        {
            string currentTxt = textoTMP != null ? textoTMP.text : (textoLegacy != null ? textoLegacy.text : "");
            string parentName = transform.parent != null ? transform.parent.name : "";
            
            // Si la clave es opciones_calificar pero el objeto fue duplicado para Privacidad, Términos o Feedback, corregirla
            if (claveTraduccion == "opciones_calificar" || string.IsNullOrEmpty(claveTraduccion))
            {
                string inferida = LocalizationManager.Instancia.ObtenerClavePorNombre(gameObject.name, parentName, currentTxt);
                if (!string.IsNullOrEmpty(inferida) && inferida != "opciones_calificar")
                {
                    claveTraduccion = inferida;
                }
                else if (string.IsNullOrEmpty(claveTraduccion))
                {
                    claveTraduccion = inferida;
                }
            }
        }

        if (string.IsNullOrEmpty(claveTraduccion)) return;

        string traduccion = "";
        if (LocalizationManager.Instancia != null)
        {
            traduccion = LocalizationManager.Instancia.ObtenerTexto(claveTraduccion);
        }

        if (!string.IsNullOrEmpty(traduccion))
        {
            if (forzarMayusculas) traduccion = traduccion.ToUpper();

            if (textoTMP != null)
            {
                textoTMP.text = traduccion;
            }
            else if (textoLegacy != null)
            {
                textoLegacy.text = traduccion;
            }
        }
    }

    /// <summary>
    /// Permite asignar una nueva clave de traducción por código y actualizar el texto al instante.
    /// </summary>
    public void AsignarClave(string nuevaClave)
    {
        claveTraduccion = nuevaClave;
        ActualizarTexto();
    }
}

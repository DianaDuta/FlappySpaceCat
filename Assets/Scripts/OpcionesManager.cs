using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Administra la configuración preferida del usuario, incluyendo volumen de audio, 
/// ajustes de vibración háptica, selección de idioma mediante desplegable y enlaces externos.
/// </summary>
public class OpcionesManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelOpciones;

    [Header("UI Audio")]
    public Slider sliderMusica;
    public Slider sliderEfectos;
    public AudioMixer audioMixer;

    [Header("UI Extras")]
    public Toggle toggleVibracion;

    [Header("Botón Calificar")]
    [Tooltip("Botón para calificar el juego. Se ocultará automáticamente si el usuario ya ha calificado.")]
    public GameObject botonCalificar;

    [Header("UI Idioma")]
    [Tooltip("Desplegable TextMeshPro (TMP_Dropdown) con las opciones de idiomas disponibles.")]
    public TMP_Dropdown dropdownIdiomasTMP;

    [Tooltip("Desplegable estándar de Unity (Dropdown) con las opciones de idiomas.")]
    public Dropdown dropdownIdiomasLegacy;

    [Tooltip("Texto opcional para mostrar el nombre del idioma actual.")]
    public TextMeshProUGUI txtIdiomaActual;
    
    [Header("Enlaces y Soporte")]
    [Tooltip("URL pública de la política de privacidad.")]
    public string urlPrivacidad = "https://dianaduta.github.io/FlappySpaceCat/Legal/privacy.html"; 

    [Tooltip("URL pública de los términos de servicio.")]
    public string urlTerminos = "https://dianaduta.github.io/FlappySpaceCat/Legal/terms.html";

    [Tooltip("Correo de contacto para soporte y feedback de jugadores.")]
    public string emailSoporte = "flappyspacecat@gmail.com";

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += SincronizarDropdownIdioma;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= SincronizarDropdownIdioma;
    }

    private void Awake()
    {
        AsegurarReferenciasSliders();
    }

    /// <summary>
    /// Oculta el panel por defecto y carga las preferencias almacenadas en el sistema.
    /// </summary>
    private void Start()
    {
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }
        AsegurarReferenciasSliders();
        CargarAjustes();
        ActualizarBotonCalificar();
        ConfigurarDropdownIdiomas();
        ActualizarTextoIdioma();
    }

    /// <summary>
    /// Garantiza que los Sliders de música y efectos estén correctamente encontrados y enlazados
    /// a sus respectivos controladores dinámicos mediante código directo.
    /// </summary>
    public void AsegurarReferenciasSliders()
    {
        if (panelOpciones == null)
        {
            GameObject buscado = GameObject.Find("Panel_Opciones");
            if (buscado != null) panelOpciones = buscado;
        }

        if (panelOpciones != null)
        {
            if (sliderMusica == null)
            {
                Transform tMusica = BuscarTransformRecursivo(panelOpciones.transform, "Slider_Musica");
                if (tMusica != null) sliderMusica = tMusica.GetComponent<Slider>();
            }

            if (sliderEfectos == null)
            {
                Transform tEfectos = BuscarTransformRecursivo(panelOpciones.transform, "Slider_Efectos");
                if (tEfectos != null) sliderEfectos = tEfectos.GetComponent<Slider>();
            }

            if (toggleVibracion == null)
            {
                Transform tVib = BuscarTransformRecursivo(panelOpciones.transform, "Toggle_Vibracion");
                if (tVib != null) toggleVibracion = tVib.GetComponent<Toggle>();
            }
        }

        if (sliderMusica != null)
        {
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.onValueChanged.AddListener(SetMusica);
        }

        if (sliderEfectos != null)
        {
            sliderEfectos.onValueChanged.RemoveAllListeners();
            sliderEfectos.onValueChanged.AddListener(SetEfectos);
        }

        if (toggleVibracion != null)
        {
            toggleVibracion.onValueChanged.RemoveAllListeners();
            toggleVibracion.onValueChanged.AddListener(ToggleVibracion);
        }

        // Vinculación y traducción robusta de todos los botones en el panel de opciones
        ConfigurarYTraducirBotonesOpciones();
    }

    /// <summary>
    /// Rellena automáticamente el desplegable con todos los idiomas soportados
    /// y selecciona el idioma actualmente activo.
    /// </summary>
    public void ConfigurarDropdownIdiomas()
    {
        if (LocalizationManager.Instancia == null) return;

        List<InfoIdioma> idiomas = LocalizationManager.Instancia.IdiomasDisponibles;
        if (idiomas == null || idiomas.Count == 0) return;

        // 1. Configurar TMP_Dropdown (TextMeshPro)
        if (dropdownIdiomasTMP != null)
        {
            dropdownIdiomasTMP.ClearOptions();
            List<TMP_Dropdown.OptionData> opcionesTMP = new List<TMP_Dropdown.OptionData>();
            foreach (var idm in idiomas)
            {
                opcionesTMP.Add(new TMP_Dropdown.OptionData(idm.nombre));
            }
            dropdownIdiomasTMP.AddOptions(opcionesTMP);

            int indiceActual = idiomas.FindIndex(i => i.codigo == LocalizationManager.Instancia.IdiomaActual);
            if (indiceActual >= 0) dropdownIdiomasTMP.SetValueWithoutNotify(indiceActual);

            dropdownIdiomasTMP.onValueChanged.RemoveListener(AlCambiarDropdownIdioma);
            dropdownIdiomasTMP.onValueChanged.AddListener(AlCambiarDropdownIdioma);

            // Ajustar el Viewport del Template para darle margen superior cómodo (25px)
            if (dropdownIdiomasTMP.template != null)
            {
                Transform viewport = dropdownIdiomasTMP.template.Find("Viewport");
                if (viewport != null)
                {
                    RectTransform rtViewport = viewport.GetComponent<RectTransform>();
                    if (rtViewport != null)
                    {
                        rtViewport.offsetMax = new Vector2(rtViewport.offsetMax.x, -25f);
                        rtViewport.offsetMin = new Vector2(rtViewport.offsetMin.x, 10f);
                    }

                    Transform content = viewport.Find("Content");
                    if (content != null)
                    {
                        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
                        if (vlg != null)
                        {
                            vlg.padding.top = 20;
                            vlg.padding.bottom = 15;
                        }
                    }
                }
            }
        }

        // 2. Configurar Dropdown Legacy (uGUI)
        if (dropdownIdiomasLegacy != null)
        {
            dropdownIdiomasLegacy.ClearOptions();
            List<Dropdown.OptionData> opcionesLegacy = new List<Dropdown.OptionData>();
            foreach (var idm in idiomas)
            {
                opcionesLegacy.Add(new Dropdown.OptionData(idm.nombre));
            }
            dropdownIdiomasLegacy.AddOptions(opcionesLegacy);

            int indiceActual = idiomas.FindIndex(i => i.codigo == LocalizationManager.Instancia.IdiomaActual);
            if (indiceActual >= 0) dropdownIdiomasLegacy.SetValueWithoutNotify(indiceActual);

            dropdownIdiomasLegacy.onValueChanged.RemoveListener(AlCambiarDropdownIdioma);
            dropdownIdiomasLegacy.onValueChanged.AddListener(AlCambiarDropdownIdioma);
        }
    }

    /// <summary>
    /// Método invocado al seleccionar una opción en el desplegable de idiomas.
    /// </summary>
    public void AlCambiarDropdownIdioma(int indiceSeleccionado)
    {
        if (LocalizationManager.Instancia == null) return;

        List<InfoIdioma> idiomas = LocalizationManager.Instancia.IdiomasDisponibles;
        if (indiceSeleccionado >= 0 && indiceSeleccionado < idiomas.Count)
        {
            string nuevoCodigo = idiomas[indiceSeleccionado].codigo;
            LocalizationManager.Instancia.CambiarIdioma(nuevoCodigo);

            if (SonidosUIManager.Instancia != null)
            {
                SonidosUIManager.Instancia.ReproducirSonidoBoton();
            }
        }
    }

    /// <summary>
    /// Mantiene el desplegable sincronizado con el idioma activo.
    /// </summary>
    public void SincronizarDropdownIdioma()
    {
        if (LocalizationManager.Instancia == null) return;

        List<InfoIdioma> idiomas = LocalizationManager.Instancia.IdiomasDisponibles;
        if (idiomas != null)
        {
            int indiceActual = idiomas.FindIndex(i => i.codigo == LocalizationManager.Instancia.IdiomaActual);
            if (indiceActual >= 0)
            {
                if (dropdownIdiomasTMP != null) dropdownIdiomasTMP.SetValueWithoutNotify(indiceActual);
                if (dropdownIdiomasLegacy != null) dropdownIdiomasLegacy.SetValueWithoutNotify(indiceActual);
            }
        }
        ActualizarTextoIdioma();
        ActualizarTextosOpciones();
    }

    /// <summary>
    /// Traduce en tiempo real todos los textos y etiquetas dentro del panel de opciones.
    /// </summary>
    public void ActualizarTextosOpciones()
    {
        if (panelOpciones == null || LocalizationManager.Instancia == null) return;

        TraducirTextoEnHijo(panelOpciones.transform, "Titulo_Opciones", "opciones_titulo");
        TraducirTextoEnHijo(panelOpciones.transform, "Txt_Ajustes", "opciones_titulo");
        TraducirTextoEnHijo(panelOpciones.transform, "Txt_Musica", "opciones_musica");
        TraducirTextoEnHijo(panelOpciones.transform, "Txt_Efectos", "opciones_efectos");
        TraducirTextoEnHijo(panelOpciones.transform, "Txt_Idiomas", "opciones_idioma");
        TraducirTextoEnHijo(panelOpciones.transform, "Toggle_Vibracion", "opciones_vibracion");

        ConfigurarYTraducirBotonesOpciones();
    }

    /// <summary>
    /// Identifica dinámicamente cada botón en el panel de opciones por su nombre o contenido,
    /// asignándole su listener de clic y su traducción correcta en el idioma activo.
    /// </summary>
    public void ConfigurarYTraducirBotonesOpciones()
    {
        if (panelOpciones == null || LocalizationManager.Instancia == null) return;

        Button[] botones = panelOpciones.GetComponentsInChildren<Button>(true);
        foreach (Button btn in botones)
        {
            string bName = btn.gameObject.name.ToLower();
            if (bName.Contains("cerrar") || bName.Contains("close") || bName == "btn_x" || bName.Contains("back")) continue;

            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            TextoTraducible tt = btn.GetComponentInChildren<TextoTraducible>(true);
            string txtActual = tmp != null ? tmp.text.ToLower().Trim() : "";

            // 1. Botón de Privacidad
            if (bName.Contains("privacidad") || bName.Contains("privacy") || bName.Contains("politica") || 
                txtActual.Contains("privacidad") || txtActual.Contains("privacy") || txtActual.Contains("confidentialit") || txtActual.Contains("datenschutz"))
            {
                btn.onClick.RemoveListener(VerPrivacidad);
                btn.onClick.AddListener(VerPrivacidad);
                if (tt != null) tt.claveTraduccion = "opciones_privacidad";
                if (tmp != null) tmp.text = LocalizationManager.Instancia.ObtenerTexto("opciones_privacidad", "Privacidad");
            }
            // 2. Botón de Términos
            else if (bName.Contains("termino") || bName.Contains("term") || bName.Contains("condicion") || 
                     txtActual.Contains("términos") || txtActual.Contains("terminos") || txtActual.Contains("terms") || txtActual.Contains("condition") || txtActual.Contains("agb") || txtActual.Contains("nutzungsbed"))
            {
                btn.onClick.RemoveListener(VerTerminosServicio);
                btn.onClick.AddListener(VerTerminosServicio);
                if (tt != null) tt.claveTraduccion = "opciones_terminos";
                if (tmp != null) tmp.text = LocalizationManager.Instancia.ObtenerTexto("opciones_terminos", "Términos");
            }
            // 3. Botón de Feedback / Soporte
            else if (bName.Contains("feedback") || bName.Contains("soporte") || bName.Contains("comentario") || bName.Contains("support") || 
                     txtActual.Contains("comentarios") || txtActual.Contains("feedback") || txtActual.Contains("soporte") || txtActual.Contains("support"))
            {
                btn.onClick.RemoveListener(EnviarFeedback);
                btn.onClick.AddListener(EnviarFeedback);
                if (tt != null) tt.claveTraduccion = "opciones_feedback";
                if (tmp != null) tmp.text = LocalizationManager.Instancia.ObtenerTexto("opciones_feedback", "Feedback");
            }
            // 4. Botón de Calificar
            else if (bName.Contains("calificar") || bName.Contains("rate") || 
                     txtActual.Contains("calificar") || txtActual.Contains("rate us") || txtActual.Contains("noter") || txtActual.Contains("bewerten") || txtActual.Contains("valuta") || txtActual.Contains("avaliar") || txtActual.Contains("evaluează"))
            {
                btn.onClick.RemoveListener(CalificarJuego);
                btn.onClick.AddListener(CalificarJuego);
                if (tt != null) tt.claveTraduccion = "opciones_calificar";
                if (tmp != null) tmp.text = LocalizationManager.Instancia.ObtenerTexto("opciones_calificar", "Calificar");
            }
        }
    }

    private void TraducirTextoEnHijo(Transform raiz, string nombreObjeto, string clave)
    {
        Transform t = BuscarTransformRecursivo(raiz, nombreObjeto);
        if (t != null)
        {
            TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = t.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.text = LocalizationManager.Instancia.ObtenerTexto(clave, tmp.text);
            }
        }
    }

    private Transform BuscarTransformRecursivo(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = BuscarTransformRecursivo(parent.GetChild(i), name);
            if (result != null) return result;
        }
        return null;
    }

    /// <summary>
    /// Cambia el idioma del juego a un código específico ('es', 'en', 'fr', 'de', 'it', 'pt', 'ro').
    /// </summary>
    public void CambiarIdioma(string codigoIdioma)
    {
        if (LocalizationManager.Instancia != null)
        {
            LocalizationManager.Instancia.CambiarIdioma(codigoIdioma);
            ActualizarTextoIdioma();
            ActualizarTextosOpciones();
        }
    }

    /// <summary>
    /// Cambia cíclicamente al siguiente idioma disponible en la lista.
    /// </summary>
    public void SiguienteIdioma()
    {
        if (LocalizationManager.Instancia != null)
        {
            LocalizationManager.Instancia.SiguienteIdioma();
            ActualizarTextoIdioma();
            ActualizarTextosOpciones();
        }
    }

    /// <summary>
    /// Actualiza la visualización del nombre del idioma activo en la UI.
    /// </summary>
    public void ActualizarTextoIdioma()
    {
        if (txtIdiomaActual != null && LocalizationManager.Instancia != null)
        {
            txtIdiomaActual.text = LocalizationManager.Instancia.ObtenerNombreIdiomaActual();
        }
    }

    /// <summary>
    /// Comprueba si el usuario ya ha calificado el juego y oculta el botón si ya lo hizo.
    /// </summary>
    public void ActualizarBotonCalificar()
    {
        if (botonCalificar == null && panelOpciones != null)
        {
            Transform encontrado = panelOpciones.transform.Find("Btn_Calificar");
            if (encontrado == null) encontrado = panelOpciones.transform.Find("Btn_Rate");
            if (encontrado != null) botonCalificar = encontrado.gameObject;
        }

        if (botonCalificar != null)
        {
            int calificacion = SecurePrefs.GetInt("CalificacionEstrellas", 0);
            bool yaCalificado = (calificacion > 0);
            botonCalificar.SetActive(!yaCalificado);
        }
    }

    /// <summary>
    /// Recupera de la memoria local los volúmenes del sistema de audio y el estado
    /// de la vibración, aplicándolos a los componentes de la interfaz.
    /// </summary>
    private void CargarAjustes()
    {
        float volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 1f);
        float volumenEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);

        if (sliderMusica != null)
        {
            sliderMusica.SetValueWithoutNotify(volumenMusica);
        }
        if (sliderEfectos != null)
        {
            sliderEfectos.SetValueWithoutNotify(volumenEfectos);
        }

        SetMusica(volumenMusica);
        SetEfectos(volumenEfectos);

        int vibracionActiva = SecurePrefs.GetInt("VibracionActiva", 1);
        if (toggleVibracion != null)
        {
            toggleVibracion.SetIsOnWithoutNotify(vibracionActiva == 1);
        }
    }

    /// <summary>
    /// Verifica que la referencia del AudioMixer esté disponible, buscándola si es necesario.
    /// </summary>
    private void VerificarAudioMixer()
    {
        if (audioMixer == null)
        {
            if (GameManager.Instancia != null && GameManager.Instancia.grupoMusica != null)
            {
                audioMixer = GameManager.Instancia.grupoMusica.audioMixer;
            }
            else if (SonidosUIManager.Instancia != null && SonidosUIManager.Instancia.grupoEfectos != null)
            {
                audioMixer = SonidosUIManager.Instancia.grupoEfectos.audioMixer;
            }
            else
            {
                AudioMixer[] mixers = Resources.FindObjectsOfTypeAll<AudioMixer>();
                if (mixers != null && mixers.Length > 0)
                {
                    audioMixer = mixers[0];
                }
            }
        }
    }

    // --- NAVEGACIÓN ---

    /// <summary>
    /// Muestra la interfaz de configuraciones y actualiza la visibilidad del botón de calificar.
    /// </summary>
    public void AbrirOpciones()
    {
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }
        AsegurarReferenciasSliders();
        CargarAjustes();
        ActualizarBotonCalificar();
        ConfigurarDropdownIdiomas();
        SincronizarDropdownIdioma();
    }

    /// <summary>
    /// Oculta la interfaz de configuraciones.
    /// </summary>
    public void CerrarOpciones()
    {
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }
    }

    // --- CONTROL DE AUDIO ---

    /// <summary>
    /// Ajusta logarítmicamente el canal principal de música en el AudioMixer y guarda la preferencia.
    /// </summary>
    /// <param name="sliderValue">El valor lineal proveniente de la interfaz gráfica (0.0 a 1.0).</param>
    public void SetMusica(float sliderValue)
    {
        VerificarAudioMixer();
        float valorMixer = (sliderValue <= 0.0001f) ? -80f : Mathf.Log10(Mathf.Clamp01(sliderValue)) * 20f;
        if (audioMixer != null)
        {
            audioMixer.SetFloat("VolMusica", valorMixer);
        }
        PlayerPrefs.SetFloat("VolumenMusica", sliderValue);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Ajusta logarítmicamente el canal principal de efectos en el AudioMixer y guarda la preferencia.
    /// </summary>
    /// <param name="sliderValue">El valor lineal proveniente de la interfaz gráfica (0.0 a 1.0).</param>
    public void SetEfectos(float sliderValue)
    {
        VerificarAudioMixer();
        float valorMixer = (sliderValue <= 0.0001f) ? -80f : Mathf.Log10(Mathf.Clamp01(sliderValue)) * 20f;
        if (audioMixer != null)
        {
            audioMixer.SetFloat("VolEfectos", valorMixer);
        }
        PlayerPrefs.SetFloat("VolumenEfectos", sliderValue);
        PlayerPrefs.Save();
    }

    // --- VIBRACIÓN ---

    /// <summary>
    /// Modifica y almacena el estado global de la vibración del dispositivo.
    /// </summary>
    /// <param name="activado">Booleano que determina si el dispositivo debe vibrar.</param>
    public void ToggleVibracion(bool activado)
    {
        SecurePrefs.SetInt("VibracionActiva", activado ? 1 : 0);
        
        if (activado) Handheld.Vibrate(); 
    }

    /// <summary>
    /// Consulta la preferencia almacenada y activa la respuesta háptica si se encuentra habilitada.
    /// Expuesto de manera global (estática) para uso transversal en todo el juego.
    /// </summary>
    public static void VibrarSiEstaActivado()
    {
        if (SecurePrefs.GetInt("VibracionActiva", 1) == 1)
        {
            Handheld.Vibrate();
        }
    }

    // --- LINKS EXTERNOS ---

    /// <summary>
    /// Abre el panel de calificación a través del CalificacionManager.
    /// </summary>
    public void CalificarJuego()
    {
        if (CalificacionManager.Instancia != null)
        {
            CalificacionManager.Instancia.AbrirCalificacion();
        }
        else
        {
            string idPaquete = !string.IsNullOrEmpty(Application.identifier) ? Application.identifier : "com.Dianarcado.FlappySpaceCat";
            Application.OpenURL("market://details?id=" + idPaquete);
        }
    }

    /// <summary>
    /// Redirige al usuario al documento de políticas de privacidad correspondiente.
    /// </summary>
    public void VerPrivacidad()
    {
        if (SonidosUIManager.Instancia != null) SonidosUIManager.Instancia.ReproducirSonidoBoton();
        string url = urlPrivacidad;
        if (string.IsNullOrEmpty(url) || url.Contains("tupagina.com") || url.Contains("dianarcado.github.io"))
        {
            url = "https://dianaduta.github.io/FlappySpaceCat/Legal/privacy.html";
            urlPrivacidad = url;
        }
        Application.OpenURL(url);
    }

    /// <summary>
    /// Redirige al usuario al documento de términos de servicio.
    /// </summary>
    public void VerTerminosServicio()
    {
        if (SonidosUIManager.Instancia != null) SonidosUIManager.Instancia.ReproducirSonidoBoton();
        string url = urlTerminos;
        if (string.IsNullOrEmpty(url) || url.Contains("tupagina.com") || url.Contains("dianarcado.github.io"))
        {
            url = "https://dianaduta.github.io/FlappySpaceCat/Legal/terms.html";
            urlTerminos = url;
        }
        Application.OpenURL(url);
    }

    /// <summary>
    /// Abre el cliente de correo del dispositivo con plantilla lista para enviar sugerencias o reportes.
    /// </summary>
    public void EnviarFeedback()
    {
        if (SonidosUIManager.Instancia != null) SonidosUIManager.Instancia.ReproducirSonidoBoton();

        string correo = string.IsNullOrEmpty(emailSoporte) || emailSoporte.Contains("diana.soporte.dev") ? "flappyspacecat@gmail.com" : emailSoporte;
        string asunto = System.Uri.EscapeDataString("Flappy Space Cat - Feedback & Soporte");
        string cuerpo = System.Uri.EscapeDataString(
            "\n\n--- Información del Dispositivo ---\n" +
            $"Versión del Juego: {Application.version}\n" +
            $"Dispositivo: {SystemInfo.deviceModel}\n" +
            $"Sistema Operativo: {SystemInfo.operatingSystem}\n"
        );
        string mailtoUri = $"mailto:{correo}?subject={asunto}&body={cuerpo}";
        Application.OpenURL(mailtoUri);
    }
}

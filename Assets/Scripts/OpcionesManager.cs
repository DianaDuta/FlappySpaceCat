using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

/// <summary>
/// Administra la configuración preferida del usuario, incluyendo volumen de audio, 
/// ajustes de vibración háptica y enlaces externos.
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
    
    private string urlGooglePlay = "market://details?id=com.tuempresa.flappyspacecat"; 
    private string urlPrivacidad = "https://tupagina.com/privacidad"; 

    /// <summary>
    /// Oculta el panel por defecto y carga las preferencias almacenadas en el sistema.
    /// </summary>
    private void Start()
    {
        panelOpciones.SetActive(false);
        CargarAjustes();
    }

    /// <summary>
    /// Recupera de la memoria local los volúmenes del sistema de audio y el estado
    /// de la vibración, aplicándolos a los componentes de la interfaz.
    /// </summary>
    private void CargarAjustes()
    {
        float volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 1f);
        float volumenEfectos = PlayerPrefs.GetFloat("VolumenEfectos", 1f);

        if (sliderMusica != null) sliderMusica.value = volumenMusica;
        if (sliderEfectos != null) sliderEfectos.value = volumenEfectos;

        SetMusica(volumenMusica);
        SetEfectos(volumenEfectos);

        int vibracionActiva = SecurePrefs.GetInt("VibracionActiva", 1);
        if (toggleVibracion != null) toggleVibracion.isOn = (vibracionActiva == 1);
    }

    // --- NAVEGACIÓN ---

    /// <summary>
    /// Muestra la interfaz de configuraciones.
    /// </summary>
    public void AbrirOpciones() => panelOpciones.SetActive(true);

    /// <summary>
    /// Oculta la interfaz de configuraciones.
    /// </summary>
    public void CerrarOpciones() => panelOpciones.SetActive(false);

    // --- CONTROL DE AUDIO ---

    /// <summary>
    /// Ajusta logarítmicamente el canal principal de música en el AudioMixer y guarda la preferencia.
    /// </summary>
    /// <param name="sliderValue">El valor lineal proveniente de la interfaz gráfica.</param>
    public void SetMusica(float sliderValue)
    {
        float valorMixer = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        audioMixer.SetFloat("VolMusica", valorMixer);
        PlayerPrefs.SetFloat("VolumenMusica", sliderValue);
    }

    /// <summary>
    /// Ajusta logarítmicamente el canal principal de efectos en el AudioMixer y guarda la preferencia.
    /// </summary>
    /// <param name="sliderValue">El valor lineal proveniente de la interfaz gráfica.</param>
    public void SetEfectos(float sliderValue)
    {
        float valorMixer = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        audioMixer.SetFloat("VolEfectos", valorMixer);
        PlayerPrefs.SetFloat("VolumenEfectos", sliderValue);
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
    /// Redirige al usuario a la ficha de la tienda de aplicaciones.
    /// </summary>
    public void CalificarJuego()
    {
        Application.OpenURL(urlGooglePlay);
    }

    /// <summary>
    /// Redirige al usuario al documento de políticas de privacidad correspondiente.
    /// </summary>
    public void VerPrivacidad()
    {
        Application.OpenURL(urlPrivacidad);
    }
}

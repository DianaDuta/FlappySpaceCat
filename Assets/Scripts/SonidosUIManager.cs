using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Gestiona la reproducción centralizada de efectos de sonido de la interfaz de usuario.
/// Utiliza el AudioSource del emisor de efectos físico en la escena.
/// </summary>
public class SonidosUIManager : MonoBehaviour
{
    public static SonidosUIManager Instancia;

    [Header("AudioSource y Mezclador")]
    [Tooltip("AudioSource para reproducir los efectos de la UI (Emisor_Efectos).")]
    public AudioSource audioSource;

    [Tooltip("Grupo de audio 'Efectos' del MixerPrincipal.")]
    public AudioMixerGroup grupoEfectos;

    [Header("Sonidos de la UI")]
    [Tooltip("Sonido que sonará por defecto en los botones estándar.")]
    public AudioClip sonidoBotonPorDefecto;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            ConfigurarAudio();
        }
        else if (Instancia != this)
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Localiza el AudioSource del emisor de efectos en la escena si no se asignó en el Inspector.
    /// </summary>
    private void ConfigurarAudio()
    {
        if (audioSource == null)
        {
            GameObject emisor = GameObject.Find("Emisor_Efectos");
            if (emisor != null)
            {
                audioSource = emisor.GetComponent<AudioSource>();
            }
        }
    }

    /// <summary>
    /// Reproduce un clip de sonido específico. Si se pasa nulo, reproduce el sonido por defecto.
    /// </summary>
    public void ReproducirSonidoBoton(AudioClip clipPersonalizado = null)
    {
        AudioClip clipFinal = clipPersonalizado != null ? clipPersonalizado : sonidoBotonPorDefecto;

        if (clipFinal != null && audioSource != null)
        {
            audioSource.PlayOneShot(clipFinal);
        }
    }

    /// <summary>
    /// Reproduce el sonido de salto específico del personaje (skin) que el usuario tiene equipado actualmente.
    /// </summary>
    public void ReproducirSonidoSaltoSkinActual()
    {
        AudioClip clipSalto = ObtenerSonidoSaltoSkinEquipada();
        ReproducirSonidoBoton(clipSalto != null ? clipSalto : sonidoBotonPorDefecto);
    }

    /// <summary>
    /// Consulta el prefab de la skin activa en el GameManager y extrae su AudioClip de salto.
    /// </summary>
    public AudioClip ObtenerSonidoSaltoSkinEquipada()
    {
        int skinActiva = SecurePrefs.GetInt("SkinEquipada", 0);
        return ObtenerSonidoSaltoSkin(skinActiva);
    }

    /// <summary>
    /// Reproduce el sonido característico de una skin específica a partir de su índice.
    /// </summary>
    public void ReproducirSonidoSkin(int indiceSkin)
    {
        AudioClip clipSalto = ObtenerSonidoSaltoSkin(indiceSkin);
        ReproducirSonidoBoton(clipSalto != null ? clipSalto : sonidoBotonPorDefecto);
    }

    /// <summary>
    /// Obtiene el AudioClip de salto de una skin dada por su índice.
    /// </summary>
    public AudioClip ObtenerSonidoSaltoSkin(int indiceSkin)
    {
        if (GameManager.Instancia != null && GameManager.Instancia.prefabsSkinsJugador != null)
        {
            if (indiceSkin >= 0 && indiceSkin < GameManager.Instancia.prefabsSkinsJugador.Length)
            {
                GameObject prefabPersonaje = GameManager.Instancia.prefabsSkinsJugador[indiceSkin];
                if (prefabPersonaje != null)
                {
                    ControladorJugagor controlador = prefabPersonaje.GetComponent<ControladorJugagor>();
                    if (controlador != null && controlador.sonidoSalto != null)
                    {
                        return controlador.sonidoSalto;
                    }
                }
            }
        }

        return null;
    }
}

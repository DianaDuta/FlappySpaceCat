using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tipos de sonido configurables para un botón de la interfaz.
/// </summary>
public enum TipoSonidoBoton
{
    PorDefecto,
    Personalizado,
    SaltoPersonajeActual
}

/// <summary>
/// Se añade a cualquier botón de la UI para reproducir automáticamente un sonido al hacer clic.
/// Soporta sonido por defecto, sonido personalizado o el sonido de salto del personaje equipado.
/// </summary>
[RequireComponent(typeof(Button))]
public class BotonSonido : MonoBehaviour
{
    [Header("Configuración de Sonido")]
    [Tooltip("Elige si este botón usa el sonido estándar, uno personalizado, o el salto del personaje actual.")]
    public TipoSonidoBoton tipoSonido = TipoSonidoBoton.PorDefecto;

    [Header("Sonido Personalizado")]
    [Tooltip("AudioClip específico a reproducir si 'Tipo de Sonido' está en 'Personalizado'.")]
    public AudioClip sonidoPersonalizado;

    private Button boton;

    private void Awake()
    {
        boton = GetComponent<Button>();
        if (boton != null)
        {
            boton.onClick.AddListener(AlPulsarBoton);
        }
    }

    private void AlPulsarBoton()
    {
        if (SonidosUIManager.Instancia != null)
        {
            switch (tipoSonido)
            {
                case TipoSonidoBoton.SaltoPersonajeActual:
                    SonidosUIManager.Instancia.ReproducirSonidoSaltoSkinActual();
                    break;

                case TipoSonidoBoton.Personalizado:
                    SonidosUIManager.Instancia.ReproducirSonidoBoton(sonidoPersonalizado);
                    break;

                case TipoSonidoBoton.PorDefecto:
                default:
                    SonidosUIManager.Instancia.ReproducirSonidoBoton();
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        if (boton != null)
        {
            boton.onClick.RemoveListener(AlPulsarBoton);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contenedor de referencias visuales para el prefab de la tarjeta de logro.
/// </summary>
public class TarjetaLogroUI : MonoBehaviour
{
    public TextMeshProUGUI txtTitulo;
    public TextMeshProUGUI txtDescripcion;
    public Image iconoLogro;
    public Image fondoTarjeta;
    
    [Tooltip("Imagen genérica que mostrará el Candado si está bloqueado, o el Trofeo si está conseguido")]
    public Image iconoEstado; 
}

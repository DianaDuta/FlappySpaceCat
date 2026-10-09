using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Contenedor de referencias visuales para el prefab de interfaz de las tarjetas 
/// individuales mostradas dentro del grid de la tienda de skins.
/// Soporta tanto imágenes estáticas como prefabs de animales animados en tiempo real.
/// </summary>
public class TarjetaSkinUI : MonoBehaviour
{
    [Header("Iconos y Visualización")]
    public Image iconoGato;
    [Tooltip("Contenedor opcional donde se instancia el prefab animado del animal (si está disponible).")]
    public RectTransform contenedorAnimacion;

    [Header("Textos")]
    public TextMeshProUGUI txtNombre;
    public TextMeshProUGUI txtPrecio;

    [Header("Elementos Interactivos")]
    public Image iconoGema; // Imagen pequeña de la gema junto al precio
    public Button botonAccion;
    [Tooltip("Botón opcional sobre toda la tarjeta para reproducir el sonido o seleccionar.")]
    public Button botonTarjeta;
}

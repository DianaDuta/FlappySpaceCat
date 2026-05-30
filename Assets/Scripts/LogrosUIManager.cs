using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contiene la información de textos e iconos que se mostrarán al jugador.
/// </summary>
[System.Serializable]
public class DatosVisualesLogro
{
    public TipoLogro tipo;
    public string titulo;
    [TextArea]
    public string descripcion;
    public Sprite icono;
}

/// <summary>
/// Gestiona la ventana gráfica de los logros, generando la lista
/// y cambiando su aspecto si están bloqueados o desbloqueados.
/// </summary>
public class LogrosUIManager : MonoBehaviour
{
    [Header("Referencias a la Interfaz")]
    public GameObject panelLogros;
    public Transform contenidoScroll;
    public GameObject prefabTarjetaLogro;

    [Header("Textos e Iconos de los Logros")]
    public DatosVisualesLogro[] listaLogrosVisuales;

    [Header("Iconos Generales")]
    public Sprite spriteCandado; // Se mostrará cuando esté bloqueado
    public Sprite spriteTrofeo;  // Se mostrará cuando esté conseguido

    void Start()
    {
        // Se asegura de que el panel empiece cerrado
        if (panelLogros != null) panelLogros.SetActive(false);
    }

    /// <summary>
    /// Abre el panel y genera/actualiza todas las tarjetas en el momento.
    /// </summary>
    public void AbrirPanelLogros()
    {
        if (panelLogros != null) panelLogros.SetActive(true);
        GenerarTarjetas();
    }

    /// <summary>
    /// Cierra el panel de logros.
    /// </summary>
    public void CerrarPanelLogros()
    {
        if (panelLogros != null) panelLogros.SetActive(false);
    }

    /// <summary>
    /// Crea cada tarjeta de logro dinámicamente y la pinta según su estado.
    /// </summary>
    private void GenerarTarjetas()
    {
        if (prefabTarjetaLogro == null || contenidoScroll == null) return;

        // Limpia las tarjetas antiguas para no duplicarlas
        foreach (Transform hijo in contenidoScroll)
        {
            Destroy(hijo.gameObject);
        }

        // Genera la nueva lista
        foreach (DatosVisualesLogro datos in listaLogrosVisuales)
        {
            GameObject nuevaTarjeta = Instantiate(prefabTarjetaLogro, contenidoScroll, false);
            nuevaTarjeta.transform.localScale = Vector3.one; 

            TarjetaLogroUI scriptUI = nuevaTarjeta.GetComponent<TarjetaLogroUI>();

            if (scriptUI != null)
            {
                // Asignar textos
                if (scriptUI.txtTitulo != null) scriptUI.txtTitulo.text = datos.titulo;
                if (scriptUI.txtDescripcion != null) scriptUI.txtDescripcion.text = datos.descripcion;
                
                // Asignar icono si lo hay
                if (datos.icono != null && scriptUI.iconoLogro != null)
                {
                    scriptUI.iconoLogro.sprite = datos.icono;
                }

                // Comprobar si el jugador ha conseguido este logro
                bool estaConseguido = false;
                if (LogrosManager.Instancia != null)
                {
                    estaConseguido = LogrosManager.Instancia.EstaDesbloqueado(datos.tipo);
                }

                // Aplicar estilos según el estado
                if (estaConseguido)
                {
                    // Al estar desbloqueado
                    if (scriptUI.iconoEstado != null)
                    {
                        // Se requiere que el candado desaparezca para ver la imagen de abajo
                        // Se puede usar color transparente o desactivarlo
                        scriptUI.iconoEstado.color = Color.clear; 
                    }
                }
                else
                {
                    // Al estar bloqueado
                    if (scriptUI.iconoEstado != null)
                    {
                        scriptUI.iconoEstado.color = Color.white; 
                        
                        if (spriteCandado != null)
                        {
                            scriptUI.iconoEstado.sprite = spriteCandado;
                        }
                    }
                }
            }
        }
    }
}

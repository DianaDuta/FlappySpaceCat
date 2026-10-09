using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona la visualización y encolamiento de las notificaciones emergentes de logros conseguidos.
/// Si un logro se completa durante una partida, se guarda en cola para mostrarse automáticamente al volver al Menú Principal.
/// </summary>
public class NotificacionLogrosManager : MonoBehaviour
{
    public static NotificacionLogrosManager Instancia;

    [Header("Referencias del Panel de Notificación")]
    [Tooltip("Objeto raíz del panel de notificación de logros.")]
    public GameObject panelNotificacion;

    [Tooltip("Componente de la tarjeta visual del logro dentro de la notificación.")]
    public TarjetaLogroUI tarjetaLogroUI;

    [Tooltip("Texto que muestra la cantidad de gemas obtenidas (por defecto '100').")]
    public TextMeshProUGUI txtGemasRecompensa;

    [Tooltip("Botón para aceptar/cerrar la notificación y pasar a la siguiente.")]
    public Button btnAceptar;

    [Header("Efectos y Animación")]
    [Tooltip("Clip de sonido que se reproducirá al desplegar la notificación.")]
    public AudioClip sonidoAparicion;

    [Tooltip("Activa una animación de escala (pop/bounce) al abrir el panel.")]
    public bool animarAparicion = true;

    [Tooltip("Elemento que se escalará (si se deja vacío, se escala el panelNotificacion completo).")]
    public Transform contenedorDialogo;

    [Tooltip("Duración de la animación de aparición en segundos.")]
    [Range(0.1f, 1f)]
    public float duracionAnimacion = 0.35f;

    private Coroutine corrutinaAnimacion;

    // Cola FIFO de logros pendientes de notificación
    private Queue<TipoLogro> colaLogrosPendientes = new Queue<TipoLogro>();

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else if (Instancia != this)
        {
            Destroy(this);
            return;
        }

        if (btnAceptar != null)
        {
            btnAceptar.onClick.RemoveListener(CerrarNotificacionActual);
            btnAceptar.onClick.AddListener(CerrarNotificacionActual);
        }
    }

    private void Start()
    {
        if (panelNotificacion != null)
        {
            panelNotificacion.SetActive(false);
        }
    }

    /// <summary>
    /// Recibe la señal de un logro desbloqueado. Si el Menú Principal está actualmente visible
    /// y no está abierto el inicio de sesión u otro panel bloqueante, muestra la notificación.
    /// De lo contrario (en partida, inicio de sesión, etc.), lo guarda en cola para mostrarlo al volver al Menú Principal.
    /// </summary>
    public void NotificarLogroDesbloqueado(TipoLogro logro)
    {
        bool enMenuPrincipal = EsMenuPrincipalActivo();

        if (enMenuPrincipal)
        {
            // Si ya hay una notificación abierta en pantalla, encolamos para mostrarla en secuencia
            if (panelNotificacion != null && panelNotificacion.activeSelf)
            {
                colaLogrosPendientes.Enqueue(logro);
            }
            else
            {
                MostrarNotificacion(logro);
            }
        }
        else
        {
            colaLogrosPendientes.Enqueue(logro);
            Debug.Log("🔔 [NotificacionLogros] Logro encolado para mostrarse al abrir el Menú Principal: " + logro);
        }
    }

    /// <summary>
    /// Comprueba si hay logros pendientes en cola y los muestra únicamente cuando el Menú Principal está visible.
    /// </summary>
    public void VerificarYMostrarPendientes()
    {
        if (!EsMenuPrincipalActivo()) return;

        if (colaLogrosPendientes.Count > 0)
        {
            if (panelNotificacion == null || !panelNotificacion.activeSelf)
            {
                TipoLogro siguienteLogro = colaLogrosPendientes.Dequeue();
                MostrarNotificacion(siguienteLogro);
            }
        }
    }

    /// <summary>
    /// Valida que el panel del Menú Principal esté activo y que el panel de inicio de sesión NO esté en pantalla.
    /// </summary>
    private bool EsMenuPrincipalActivo()
    {
        if (GameManager.Instancia == null) return false;

        // Si está en plena partida o está abierto el inicio de sesión, NO se debe mostrar
        if (GameManager.Instancia.juegoIniciado) return false;
        if (GameManager.Instancia.panelInicioSesion != null && GameManager.Instancia.panelInicioSesion.activeSelf) return false;

        // Debe estar activo el Menú Principal
        if (GameManager.Instancia.panelMenuPrincipal != null && GameManager.Instancia.panelMenuPrincipal.activeSelf)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Despliega el panel de notificación configurando la tarjeta del logro y la recompensa.
    /// </summary>
    public void MostrarNotificacion(TipoLogro logro)
    {
        DatosVisualesLogro datos = ObtenerDatosVisuales(logro);

        if (tarjetaLogroUI != null)
        {
            if (datos != null)
            {
                string claveTitulo = $"logro_{(int)logro}_tit";
                string claveDesc = $"logro_{(int)logro}_desc";

                string tituloFinal = LocalizationManager.Instancia != null 
                    ? LocalizationManager.Instancia.ObtenerTexto(claveTitulo, datos.titulo) 
                    : datos.titulo;

                string descFinal = LocalizationManager.Instancia != null 
                    ? LocalizationManager.Instancia.ObtenerTexto(claveDesc, datos.descripcion) 
                    : datos.descripcion;

                if (tarjetaLogroUI.txtTitulo != null) tarjetaLogroUI.txtTitulo.text = tituloFinal;
                if (tarjetaLogroUI.txtDescripcion != null) tarjetaLogroUI.txtDescripcion.text = descFinal;
                if (tarjetaLogroUI.iconoLogro != null && datos.icono != null) tarjetaLogroUI.iconoLogro.sprite = datos.icono;
            }

            // Ocultar candado (el logro está conseguido)
            if (tarjetaLogroUI.iconoEstado != null)
            {
                tarjetaLogroUI.iconoEstado.gameObject.SetActive(false);
                tarjetaLogroUI.iconoEstado.enabled = false;
            }
        }

        if (txtGemasRecompensa != null)
        {
            txtGemasRecompensa.text = "100";
        }

        if (panelNotificacion != null)
        {
            panelNotificacion.SetActive(true);
            panelNotificacion.transform.SetAsLastSibling(); // Poner al frente en el Canvas
        }

        // Reproducir sonido de aparición (o el sonido por defecto de la UI)
        if (SonidosUIManager.Instancia != null)
        {
            if (sonidoAparicion != null)
            {
                SonidosUIManager.Instancia.ReproducirSonidoBoton(sonidoAparicion);
            }
            else
            {
                SonidosUIManager.Instancia.ReproducirSonidoBoton();
            }
        }

        // Animación suave de escala emergente (Pop / Bounce)
        if (animarAparicion && panelNotificacion != null)
        {
            if (corrutinaAnimacion != null) StopCoroutine(corrutinaAnimacion);
            corrutinaAnimacion = StartCoroutine(AnimarAparicionCoroutine());
        }

        Debug.Log("🎉 [NotificacionLogros] Mostrando notificación del logro: " + logro);
    }

    /// <summary>
    /// Anima suavemente la escala desde 0 hasta el tamaño normal con un ligero rebote elástico.
    /// Utiliza unscaledDeltaTime para funcionar perfectamente incluso con el juego pausado.
    /// </summary>
    private System.Collections.IEnumerator AnimarAparicionCoroutine()
    {
        Transform target = contenedorDialogo != null ? contenedorDialogo : panelNotificacion.transform;
        target.localScale = Vector3.zero;

        float tiempo = 0f;
        while (tiempo < duracionAnimacion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionAnimacion);

            // Efecto pop elástico: sube suave hasta 1.08 y se asienta en 1.0
            float factor;
            if (t < 0.7f)
            {
                factor = Mathf.Lerp(0f, 1.08f, t / 0.7f);
            }
            else
            {
                factor = Mathf.Lerp(1.08f, 1.0f, (t - 0.7f) / 0.3f);
            }

            target.localScale = new Vector3(factor, factor, 1f);
            yield return null;
        }

        target.localScale = Vector3.one;
        corrutinaAnimacion = null;
    }

    /// <summary>
    /// Cierra la notificación actual. Si quedan más logros en cola, abre el siguiente de inmediato.
    /// </summary>
    public void CerrarNotificacionActual()
    {
        if (colaLogrosPendientes.Count > 0)
        {
            TipoLogro siguienteLogro = colaLogrosPendientes.Dequeue();
            MostrarNotificacion(siguienteLogro);
        }
        else
        {
            if (panelNotificacion != null)
            {
                panelNotificacion.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Busca la información visual (título, descripción, icono) del logro solicitado directamente desde LogrosUIManager.
    /// </summary>
    private DatosVisualesLogro ObtenerDatosVisuales(TipoLogro logro)
    {
        LogrosUIManager logrosUI = LogrosUIManager.Instancia != null 
            ? LogrosUIManager.Instancia 
            : FindAnyObjectByType<LogrosUIManager>();

        if (logrosUI != null && logrosUI.listaLogrosVisuales != null)
        {
            foreach (DatosVisualesLogro d in logrosUI.listaLogrosVisuales)
            {
                if (d.tipo == logro) return d;
            }
        }

        return null;
    }
}

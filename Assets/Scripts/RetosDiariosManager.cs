using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tipos de retos diarios soportados por el sistema.
/// </summary>
public enum TipoRetoDiario
{
    ObstaculosUnaPartida,
    GemasAcumuladas,
    GemasUnaPartida,
    PartidasJugadas,
    PuntosUnaPartida,
    PuntosAcumulados,
    UsarContinuar,
    CambiarSkin,
    VerAnuncio
}

/// <summary>
/// Representa un reto diario con su configuración.
/// </summary>
[System.Serializable]
public class RetoDiario
{
    public int id;
    public string descripcion;
    public TipoRetoDiario tipo;
    public int objetivo;
    public int recompensaGemas = 20;

    public RetoDiario(int id, string descripcion, TipoRetoDiario tipo, int objetivo, int recompensaGemas = 20)
    {
        this.id = id;
        this.descripcion = descripcion;
        this.tipo = tipo;
        this.objetivo = objetivo;
        this.recompensaGemas = recompensaGemas;
    }
}

/// <summary>
/// Gestor central de los Retos Diarios.
/// Elige aleatoriamente 1 reto cada 24 horas (UTC), guarda el progreso y permite reclamar la recompensa de 20 gemas.
/// </summary>
public class RetosDiariosManager : MonoBehaviour
{
    public static RetosDiariosManager Instancia;

    [Header("Referencias de la Interfaz (UI)")]
    [Tooltip("Texto del botón en el menú principal donde se muestra el reto diario.")]
    public TextMeshProUGUI txtDiario;

    [Tooltip("Barra de progreso visual (Slider) para el reto diario.")]
    public Slider sliderProgreso;

    [Tooltip("Barra de progreso alternativa mediante imagen con Image Type = Filled.")]
    public Image barraProgresoFill;

    [Tooltip("Texto opcional para mostrar los números de progreso (ej. '3/5').")]
    public TextMeshProUGUI txtProgreso;

    [Tooltip("Botón para reclamar la recompensa de 20 gemas.")]
    public Button btnReclamarRecompensa;

    [Header("Sonidos (Opcional)")]
    public AudioClip sonidoReclamar;

    // Base de datos de los 20 retos fáciles
    private List<RetoDiario> baseDatosRetos = new List<RetoDiario>();

    // Reto activo hoy y su estado
    private RetoDiario retoActual;
    private int progresoActual = 0;
    private bool completado = false;
    private bool reclamado = false;

    // Tracking de partida en curso
    private int obstaculosPartidaActual = 0;
    private int gemasPartidaActual = 0;

    // ---------------------------------------------------------------------------------
    // MÉTODOS
    // ---------------------------------------------------------------------------------
    
    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += ActualizarInterfaz;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= ActualizarInterfaz;
    }

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

        InicializarBaseDatosRetos();
        CargarOActualizarRetoDiario();

        if (btnReclamarRecompensa != null)
        {
            btnReclamarRecompensa.onClick.RemoveListener(ReclamarRecompensa);
            btnReclamarRecompensa.onClick.AddListener(ReclamarRecompensa);
        }
    }

    private void Start()
    {
        ActualizarInterfaz();
    }

    /// <summary>
    /// Define los 20 retos diarios fáciles del juego.
    /// </summary>
    private void InicializarBaseDatosRetos()
    {
        baseDatosRetos.Clear();
        baseDatosRetos.Add(new RetoDiario(1, "Supera 3 obstáculos en una partida", TipoRetoDiario.ObstaculosUnaPartida, 3));
        baseDatosRetos.Add(new RetoDiario(2, "Recoge 10 gemas en total", TipoRetoDiario.GemasAcumuladas, 10));
        baseDatosRetos.Add(new RetoDiario(3, "Juega 2 partidas", TipoRetoDiario.PartidasJugadas, 2));
        baseDatosRetos.Add(new RetoDiario(4, "Supera 5 obstáculos en una partida", TipoRetoDiario.ObstaculosUnaPartida, 5));
        baseDatosRetos.Add(new RetoDiario(5, "Recoge 5 gemas en una partida", TipoRetoDiario.GemasUnaPartida, 5));
        baseDatosRetos.Add(new RetoDiario(6, "Usa la opción Continuar 1 vez", TipoRetoDiario.UsarContinuar, 1));
        baseDatosRetos.Add(new RetoDiario(7, "Equipa o cambia de skin en la tienda", TipoRetoDiario.CambiarSkin, 1));
        baseDatosRetos.Add(new RetoDiario(8, "Supera 7 obstáculos en una partida", TipoRetoDiario.ObstaculosUnaPartida, 7));
        baseDatosRetos.Add(new RetoDiario(9, "Recoge 15 gemas en total", TipoRetoDiario.GemasAcumuladas, 15));
        baseDatosRetos.Add(new RetoDiario(10, "Alcanza 10 puntos en una partida", TipoRetoDiario.PuntosUnaPartida, 10));
        baseDatosRetos.Add(new RetoDiario(11, "Juega 3 partidas", TipoRetoDiario.PartidasJugadas, 3));
        baseDatosRetos.Add(new RetoDiario(12, "Supera 4 obstáculos en una partida", TipoRetoDiario.ObstaculosUnaPartida, 4));
        baseDatosRetos.Add(new RetoDiario(13, "Recoge 8 gemas en una partida", TipoRetoDiario.GemasUnaPartida, 8));
        baseDatosRetos.Add(new RetoDiario(14, "Acumula 15 puntos en total", TipoRetoDiario.PuntosAcumulados, 15));
        baseDatosRetos.Add(new RetoDiario(15, "Mira 1 anuncio para obtener recompensa", TipoRetoDiario.VerAnuncio, 1));
        baseDatosRetos.Add(new RetoDiario(16, "Supera 6 obstáculos en una partida", TipoRetoDiario.ObstaculosUnaPartida, 6));
        baseDatosRetos.Add(new RetoDiario(17, "Recoge 20 gemas en total", TipoRetoDiario.GemasAcumuladas, 20));
        baseDatosRetos.Add(new RetoDiario(18, "Juega 1 partida", TipoRetoDiario.PartidasJugadas, 1));
        baseDatosRetos.Add(new RetoDiario(19, "Supera 8 obstáculos en una partida", TipoRetoDiario.ObstaculosUnaPartida, 8));
        baseDatosRetos.Add(new RetoDiario(20, "Acumula 25 puntos en total", TipoRetoDiario.PuntosAcumulados, 25));
    }

    /// <summary>
    /// Verifica la fecha guardada y rota el reto automáticamente cada 24 horas.
    /// </summary>
    public void CargarOActualizarRetoDiario()
    {
        string hoy = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string fechaGuardada = SecurePrefs.GetString("RetoDiario_Fecha", "");

        if (fechaGuardada != hoy)
        {
            // Nuevo día: elegir un reto aleatorio diferente al anterior
            int idAnterior = SecurePrefs.GetInt("RetoDiario_ID", -1);
            List<RetoDiario> candidatos = baseDatosRetos.FindAll(r => r.id != idAnterior);
            if (candidatos.Count == 0) candidatos = baseDatosRetos;

            int indexAleatorio = UnityEngine.Random.Range(0, candidatos.Count);
            retoActual = candidatos[indexAleatorio];

            progresoActual = 0;
            completado = false;
            reclamado = false;

            SecurePrefs.SetString("RetoDiario_Fecha", hoy);
            SecurePrefs.SetInt("RetoDiario_ID", retoActual.id);
            SecurePrefs.SetInt("RetoDiario_Progreso", 0);
            SecurePrefs.SetInt("RetoDiario_Completado", 0);
            SecurePrefs.SetInt("RetoDiario_Reclamado", 0);
            SecurePrefs.Save();

            Debug.Log($"📅 [RetosDiarios] Nuevo reto diario asignado para {hoy}: {retoActual.descripcion}");
        }
        else
        {
            // Mismo día: cargar el reto activo y su progreso
            int idGuardado = SecurePrefs.GetInt("RetoDiario_ID", 1);
            retoActual = baseDatosRetos.Find(r => r.id == idGuardado);
            if (retoActual == null) retoActual = baseDatosRetos[0];

            progresoActual = SecurePrefs.GetInt("RetoDiario_Progreso", 0);
            completado = SecurePrefs.GetInt("RetoDiario_Completado", 0) == 1;
            reclamado = SecurePrefs.GetInt("RetoDiario_Reclamado", 0) == 1;
        }
    }

    /// <summary>
    /// Inicia el seguimiento de una nueva partida (reinicia contadores locales de partida).
    /// </summary>
    public void IniciarPartida()
    {
        obstaculosPartidaActual = 0;
        gemasPartidaActual = 0;
    }

    /// <summary>
    /// Registra la superación de un obstáculo / punto.
    /// </summary>
    public void RegistrarPunto()
    {
        if (completado || retoActual == null) return;

        obstaculosPartidaActual++;

        if (retoActual.tipo == TipoRetoDiario.ObstaculosUnaPartida || retoActual.tipo == TipoRetoDiario.PuntosUnaPartida)
        {
            if (obstaculosPartidaActual > progresoActual)
            {
                progresoActual = obstaculosPartidaActual;
                VerificarCompletado();
            }
        }
        else if (retoActual.tipo == TipoRetoDiario.PuntosAcumulados)
        {
            progresoActual++;
            VerificarCompletado();
        }
    }

    /// <summary>
    /// Registra la recolección de una gema.
    /// </summary>
    public void RegistrarGema()
    {
        if (completado || retoActual == null) return;

        gemasPartidaActual++;

        if (retoActual.tipo == TipoRetoDiario.GemasUnaPartida)
        {
            if (gemasPartidaActual > progresoActual)
            {
                progresoActual = gemasPartidaActual;
                VerificarCompletado();
            }
        }
        else if (retoActual.tipo == TipoRetoDiario.GemasAcumuladas)
        {
            progresoActual++;
            VerificarCompletado();
        }
    }

    /// <summary>
    /// Registra la finalización de una partida jugada.
    /// </summary>
    public void RegistrarFinPartida()
    {
        if (completado || retoActual == null) return;

        if (retoActual.tipo == TipoRetoDiario.PartidasJugadas)
        {
            progresoActual++;
            VerificarCompletado();
        }

        GuardarProgreso();
    }

    /// <summary>
    /// Registra cuando el jugador utiliza la opción de continuar una partida.
    /// </summary>
    public void RegistrarContinuar()
    {
        if (completado || retoActual == null) return;

        if (retoActual.tipo == TipoRetoDiario.UsarContinuar)
        {
            progresoActual++;
            VerificarCompletado();
        }
    }

    /// <summary>
    /// Registra cuando el jugador equipa o cambia de skin en la tienda.
    /// </summary>
    public void RegistrarCambioSkin()
    {
        if (completado || retoActual == null) return;

        if (retoActual.tipo == TipoRetoDiario.CambiarSkin)
        {
            progresoActual++;
            VerificarCompletado();
        }
    }

    /// <summary>
    /// Registra cuando el jugador visualiza un anuncio publicitario con recompensa.
    /// </summary>
    public void RegistrarAnuncioVisto()
    {
        if (completado || retoActual == null) return;

        if (retoActual.tipo == TipoRetoDiario.VerAnuncio)
        {
            progresoActual++;
            VerificarCompletado();
        }
    }

    /// <summary>
    /// Comprueba si el reto actual ha alcanzado su objetivo.
    /// </summary>
    private void VerificarCompletado()
    {
        if (retoActual != null && progresoActual >= retoActual.objetivo)
        {
            progresoActual = retoActual.objetivo;
            completado = true;
            Debug.Log($"🎉 [RetosDiarios] ¡Reto diario completado!: {retoActual.descripcion}");
        }

        GuardarProgreso();
        ActualizarInterfaz();
    }

    /// <summary>
    /// Guarda el progreso actual en almacenamiento seguro persistente.
    /// </summary>
    private void GuardarProgreso()
    {
        SecurePrefs.SetInt("RetoDiario_Progreso", progresoActual);
        SecurePrefs.SetInt("RetoDiario_Completado", completado ? 1 : 0);
        SecurePrefs.SetInt("RetoDiario_Reclamado", reclamado ? 1 : 0);
        SecurePrefs.Save();
    }

    /// <summary>
    /// Reclama las 20 gemas de recompensa si el reto fue completado y aún no ha sido reclamado.
    /// </summary>
    public void ReclamarRecompensa()
    {
        if (!completado)
        {
            Debug.Log("⚠️ [RetosDiarios] El reto aún no ha sido completado.");
            return;
        }

        if (reclamado)
        {
            Debug.Log("⚠️ [RetosDiarios] La recompensa de hoy ya fue reclamada.");
            return;
        }

        reclamado = true;
        GuardarProgreso();

        // Otorgar 20 gemas
        int gemasGuardadas = SecurePrefs.GetInt("GemasLocales", 0);
        int recompensa = retoActual != null ? retoActual.recompensaGemas : 20;
        SecurePrefs.SetInt("GemasLocales", gemasGuardadas + recompensa);
        SecurePrefs.Save();

        // Sonido de feedback
        if (SonidosUIManager.Instancia != null)
        {
            if (sonidoReclamar != null)
            {
                SonidosUIManager.Instancia.ReproducirSonidoBoton(sonidoReclamar);
            }
            else
            {
                SonidosUIManager.Instancia.ReproducirSonidoBoton();
            }
        }

        ActualizarInterfaz();
        Debug.Log($"💎 [RetosDiarios] ¡Recompensa de {recompensa} gemas reclamada exitosamente!");
    }

    /// <summary>
    /// Actualiza la visualización de los textos y botones del reto diario en el Menú Principal.
    /// </summary>
    public void ActualizarInterfaz()
    {
        CargarOActualizarRetoDiario();

        if (retoActual != null)
        {
            float progresoNormalizado = retoActual.objetivo > 0 
                ? Mathf.Clamp01((float)progresoActual / retoActual.objetivo) 
                : 0f;

            // Actualizar Slider de Unity si está asignado
            if (sliderProgreso != null)
            {
                sliderProgreso.minValue = 0f;
                sliderProgreso.maxValue = retoActual.objetivo;
                sliderProgreso.value = Mathf.Clamp(progresoActual, 0, retoActual.objetivo);
            }

            // Actualizar Imagen Filled si está asignada
            if (barraProgresoFill != null)
            {
                barraProgresoFill.fillAmount = progresoNormalizado;
            }

            // Actualizar Texto de progreso numérico si está asignado (ej. "3/5")
            if (txtProgreso != null)
            {
                if (reclamado)
                {
                    string completadoStr = LocalizationManager.Instancia != null 
                        ? LocalizationManager.Instancia.ObtenerTexto("reto_completado", "¡Completado!") 
                        : "¡Completado!";
                    txtProgreso.text = completadoStr;
                }
                else
                {
                    txtProgreso.text = $"{progresoActual}/{retoActual.objetivo}";
                }
            }

            // Actualizar Texto de descripción general (solo el texto limpio del reto)
            if (txtDiario != null)
            {
                // Obtener descripción traducida
                string descripcionTraducida = LocalizationManager.Instancia != null 
                    ? LocalizationManager.Instancia.ObtenerTexto($"reto_{retoActual.id}_desc", retoActual.descripcion) 
                    : retoActual.descripcion;

                txtDiario.text = descripcionTraducida;
            }
        }

        if (btnReclamarRecompensa != null)
        {
            if (reclamado)
            {
                // Una vez recogida la recompensa, el botón se oculta por completo
                btnReclamarRecompensa.gameObject.SetActive(false);
            }
            else
            {
                // Mientras no se haya reclamado, el botón está visible pero solo es interactuable si el reto está completado
                btnReclamarRecompensa.gameObject.SetActive(true);
                btnReclamarRecompensa.interactable = completado;
            }
        }
    }
}

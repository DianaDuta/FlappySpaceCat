using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controla una cuenta atrás visual en la pantalla de GameOver.
/// Si el temporizador llega a cero, el botón de continuar se desactiva y el juego regresa automáticamente al menú principal.
/// Funciona de forma desescalada (realtime) ya que el juego se encuentra pausado (Time.timeScale = 0).
/// </summary>
public class CuentaAtrasGameOver : MonoBehaviour
{
    [Header("Configuración del Temporizador")]
    [Tooltip("Tiempo en segundos que tiene el jugador para decidir si ver el anuncio para continuar.")]
    public float duracionTotal = 10f;

    [Header("Componentes de UI")]
    [Tooltip("El botón para continuar/ver anuncio en el panel de GameOver.")]
    public Button botonContinuar;

    [Tooltip("El texto del propio botón continuar.")]
    public TextMeshProUGUI textoBotonContinuar;

    [Tooltip("Texto por defecto para restaurar el botón si es necesario.")]
    public string textoBotonOriginal = "Continuar";

    private Coroutine coroutineCuentaAtras;
    private float segundosRestantes;
    private bool haRespondido = false;

    private void Awake()
    {
        if (textoBotonContinuar == null)
        {
            textoBotonContinuar = GetComponentInChildren<TextMeshProUGUI>();
        }
        if (botonContinuar == null)
        {
            botonContinuar = GetComponent<Button>();
            if (botonContinuar == null) botonContinuar = GetComponentInChildren<Button>();
        }
    }

    /// <summary>
    /// Obtiene el texto base traducido según el idioma activo actual.
    /// </summary>
    private string ObtenerTextoBaseTraducido()
    {
        if (LocalizationManager.Instancia != null)
        {
            // Si el texto configurado contiene "jugando" o es más largo, intentar la clave específica
            if (!string.IsNullOrEmpty(textoBotonOriginal) && textoBotonOriginal.ToLower().Contains("jugando"))
            {
                string txtLargo = LocalizationManager.Instancia.ObtenerTexto("gameover_continuar_jugando");
                if (!string.IsNullOrEmpty(txtLargo) && txtLargo != "gameover_continuar_jugando")
                {
                    return txtLargo;
                }
            }

            string traducido = LocalizationManager.Instancia.ObtenerTexto("gameover_continuar");
            if (!string.IsNullOrEmpty(traducido) && traducido != "gameover_continuar")
            {
                return traducido;
            }
        }
        return !string.IsNullOrEmpty(textoBotonOriginal) ? textoBotonOriginal : "Continuar";
    }

    /// <summary>
    /// Obtiene el mensaje de tiempo expirado traducido según el idioma activo actual.
    /// </summary>
    private string ObtenerTextoExpiradoTraducido()
    {
        if (LocalizationManager.Instancia != null)
        {
            string expirado = LocalizationManager.Instancia.ObtenerTexto("gameover_tiempo_expirado");
            if (!string.IsNullOrEmpty(expirado) && expirado != "gameover_tiempo_expirado")
            {
                return expirado;
            }
        }
        return "Tiempo Expirado";
    }

    /// <summary>
    /// Restablece el estado de respuesta, la cuenta atrás y la interactividad 
    /// y visibilidad del botón continuar a sus valores iniciales predeterminados.
    /// </summary>
    public void RestablecerBoton()
    {
        haRespondido = false;
        segundosRestantes = duracionTotal;

        if (botonContinuar != null)
        {
            botonContinuar.gameObject.SetActive(true);
            botonContinuar.interactable = true;
        }

        if (textoBotonContinuar != null)
        {
            textoBotonContinuar.text = string.Format("{0} ({1})", ObtenerTextoBaseTraducido(), duracionTotal);
        }
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += ActualizarUI;

        // Si ya se usó la opción de continuar en la partida en curso, se desactiva y oculta por completo
        if (GameManager.Instancia != null && GameManager.Instancia.haContinuadoEnPartida)
        {
            if (botonContinuar != null)
            {
                botonContinuar.interactable = false;
                botonContinuar.gameObject.SetActive(false);
            }
            return;
        }

        RestablecerBoton();

        if (botonContinuar != null)
        {
            // Asegura que el botón escuche cuando se pulsa para detener la cuenta regresiva
            botonContinuar.onClick.RemoveListener(AlPulsarContinuar);
            botonContinuar.onClick.AddListener(AlPulsarContinuar);
        }

        if (coroutineCuentaAtras != null)
        {
            StopCoroutine(coroutineCuentaAtras);
        }
        coroutineCuentaAtras = StartCoroutine(IniciarCuentaAtrasRealtime());
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= ActualizarUI;
        DetenerTemporizador();
    }

    /// <summary>
    /// Se invoca de inmediato cuando el jugador pulsa el botón "Continuar" para evitar que el tiempo expire durante la carga del anuncio.
    /// </summary>
    public void AlPulsarContinuar()
    {
        haRespondido = true;
        DetenerTemporizador();
    }

    /// <summary>
    /// Detiene de manera segura la coroutine de cuenta atrás.
    /// </summary>
    private void DetenerTemporizador()
    {
        if (coroutineCuentaAtras != null)
        {
            StopCoroutine(coroutineCuentaAtras);
            coroutineCuentaAtras = null;
        }
    }

    /// <summary>
    /// Coroutine que gestiona el decrecimiento del tiempo segundo a segundo sin usar Time.deltaTime (ya que timeScale es 0).
    /// </summary>
    private IEnumerator IniciarCuentaAtrasRealtime()
    {
        bool panelEstabaAbierto = false;

        while (segundosRestantes > 0)
        {
            // Si el panel de inicio de sesión de Firebase está activo, se pausa temporalmente la cuenta atrás
            if (GameManager.Instancia != null && GameManager.Instancia.panelInicioSesion != null && GameManager.Instancia.panelInicioSesion.activeSelf)
            {
                panelEstabaAbierto = true;
                yield return new WaitForSecondsRealtime(0.2f);
                continue;
            }

            // Si el panel de inicio de sesión estaba abierto y se acaba de cerrar (por saltar o registro exitoso),
            // se reinicia el tiempo para otorgar al jugador la cuenta atrás completa
            if (panelEstabaAbierto)
            {
                panelEstabaAbierto = false;
                segundosRestantes = duracionTotal;
            }

            ActualizarUI();
            
            // Se espera exactamente 1 segundo de tiempo real desescalado
            yield return new WaitForSecondsRealtime(1f);
            segundosRestantes--;
        }

        segundosRestantes = 0;
        ActualizarUI();

        // El tiempo ha expirado sin respuesta
        if (!haRespondido)
        {
            AlExpirarTiempo();
        }
    }

    /// <summary>
    /// Actualiza los textos asociados con los segundos restantes en el idioma activo.
    /// </summary>
    private void ActualizarUI()
    {
        // Se cambia el texto del botón de continuar mostrando los segundos restantes
        if (textoBotonContinuar != null)
        {
            if (segundosRestantes > 0)
            {
                textoBotonContinuar.text = string.Format("{0} ({1})", ObtenerTextoBaseTraducido(), segundosRestantes);
            }
            else
            {
                textoBotonContinuar.text = ObtenerTextoExpiradoTraducido();
            }
        }
    }

    /// <summary>
    /// Acción ejecutada cuando el temporizador llega a cero.
    /// Desactiva y oculta el botón de continuar para impedir interacciones futuras.
    /// </summary>
    private void AlExpirarTiempo()
    {
        Debug.Log("Cuenta atrás de GameOver finalizada. Desactivando opción de continuar.");

        if (botonContinuar != null)
        {
            botonContinuar.interactable = false;
            botonContinuar.gameObject.SetActive(false); // Oculta y desactiva el botón del panel
        }
    }
}

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

    private void OnEnable()
    {
        // Reiniciar banderas e iniciar cuenta atrás cuando se muestra el panel
        haRespondido = false;
        segundosRestantes = duracionTotal;

        if (botonContinuar != null)
        {
            botonContinuar.interactable = true;
            // Aseguramos que el botón escuche cuando se pulsa para detener la cuenta regresiva
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
    /// Actualiza los textos asociados con los segundos restantes.
    /// </summary>
    private void ActualizarUI()
    {
        // Se cambia el texto del botón de continuar mostrando los segundos restantes
        if (textoBotonContinuar != null)
        {
            if (segundosRestantes > 0)
            {
                textoBotonContinuar.text = string.Format("{0} ({1})", textoBotonOriginal, segundosRestantes);
            }
            else
            {
                textoBotonContinuar.text = "Tiempo Expirado";
            }
        }
    }

    /// <summary>
    /// Acción ejecutada cuando el temporizador llega a cero.
    /// </summary>
    private void AlExpirarTiempo()
    {
        Debug.Log("Cuenta atrás de GameOver finalizada. Retornando al menú principal...");

        if (botonContinuar != null)
        {
            botonContinuar.interactable = false;
        }

        // Se devuelve al jugador de forma automática al menú principal
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.VolverMenuPrincipal();
        }
        else
        {
            Debug.LogWarning("No se encontró la instancia de GameManager para volver al menú principal.");
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Gestiona un temporizador de enfriamiento (cooldown) persistente en el botón de anuncios del menú.
/// Utiliza SecurePrefs para garantizar que el cooldown sobreviva al cierre o reinicio del juego.
/// Desactiva automáticamente el Animator mientras esté bloqueado y lo reactiva únicamente cuando esté disponible.
/// </summary>
[RequireComponent(typeof(Button))]
public class TemporizadorBotonAnuncio : MonoBehaviour
{
    [Header("Configuración del Cooldown")]
    [Tooltip("Duración en segundos del tiempo de espera antes de poder ver otro anuncio.")]
    public float duracionCooldownSegundos = 300f; // 5 minutos por defecto
    
    [Tooltip("Clave con la que se guardará la fecha de finalización en SecurePrefs.")]
    public string clavePersistencia = "CooldownAnuncioMenu";

    [Header("Componentes de UI")]
    [Tooltip("Texto del botón que se actualizará con la cuenta atrás.")]
    public TextMeshProUGUI textoBoton;
    
    [Tooltip("Texto que se mostrará en el botón cuando el temporizador no esté activo.")]
    public string textoListo = "Ver Anuncio";

    [Header("Animación")]
    [Tooltip("Animator del botón (se desactivará mientras esté en cooldown).")]
    public Animator animadorBoton;

    private Button boton;
    private Coroutine coroutineTimer;
    private DateTime fechaFinCooldown;
    private bool estaEnCooldown = false;

    /// <summary>
    /// Indica si el botón se encuentra actualmente en tiempo de espera (cooldown).
    /// </summary>
    public bool EstaEnCooldown => estaEnCooldown;

    private void Awake()
    {
        boton = GetComponent<Button>();
        if (animadorBoton == null) animadorBoton = GetComponent<Animator>();
        if (animadorBoton == null) animadorBoton = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        // Suscribirse al evento de finalización del anuncio bonificado
        PublicidadManager.OnAnuncioRecompensadoCompletado += AlCompletarAnuncio;
        
        // Comprobar si hay un cooldown activo guardado al activarse
        ComprobarYRestaurarCooldown();
    }

    private void OnDisable()
    {
        // Cancelar suscripción para evitar pérdidas de memoria
        PublicidadManager.OnAnuncioRecompensadoCompletado -= AlCompletarAnuncio;
        
        if (coroutineTimer != null)
        {
            StopCoroutine(coroutineTimer);
            coroutineTimer = null;
        }
    }

    /// <summary>
    /// Comprueba si existe un cooldown almacenado en SecurePrefs y calcula si aún sigue activo.
    /// </summary>
    private void ComprobarYRestaurarCooldown()
    {
        string fechaGuardada = SecurePrefs.GetString(clavePersistencia, "");
        if (!string.IsNullOrEmpty(fechaGuardada))
        {
            if (DateTime.TryParse(fechaGuardada, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime finGuardado))
            {
                if (DateTime.Now < finGuardado)
                {
                    fechaFinCooldown = finGuardado;
                    ActivarCooldown(finGuardado);
                    return;
                }
            }
        }
        
        // Si no hay cooldown activo, dejar el botón en su estado normal
        DesactivarCooldown();
    }

    /// <summary>
    /// Se invoca cuando el PublicidadManager despacha el evento de anuncio visto correctamente.
    /// </summary>
    private void AlCompletarAnuncio()
    {
        // Calcular la fecha y hora de finalización del nuevo cooldown
        DateTime finNuevo = DateTime.Now.AddSeconds(duracionCooldownSegundos);
        
        // Guardar en persistencia local
        SecurePrefs.SetString(clavePersistencia, finNuevo.ToString("O"));
        SecurePrefs.Save();

        fechaFinCooldown = finNuevo;
        ActivarCooldown(finNuevo);
    }

    /// <summary>
    /// Bloquea el botón, desactiva la animación y arranca la coroutine para mostrar la cuenta atrás.
    /// </summary>
    private void ActivarCooldown(DateTime finCooldown)
    {
        estaEnCooldown = true;
        if (boton != null) boton.interactable = false;

        // Desactivar Animator y restablecer la escala del botón
        if (animadorBoton != null)
        {
            animadorBoton.enabled = false;
            animadorBoton.transform.localScale = Vector3.one;
        }

        if (coroutineTimer != null) StopCoroutine(coroutineTimer);
        coroutineTimer = StartCoroutine(ActualizarTemporizadorCooldown());
    }

    /// <summary>
    /// Habilita de nuevo el botón, reactiva la animación y borra el registro del cooldown.
    /// </summary>
    private void DesactivarCooldown()
    {
        estaEnCooldown = false;
        if (boton != null) boton.interactable = true;
        if (textoBoton != null) textoBoton.text = textoListo;

        // Reactivar Animator para que vuelva a animarse el botón listo
        if (animadorBoton != null)
        {
            animadorBoton.enabled = true;
        }

        // Limpiar de SecurePrefs para indicar que está listo
        SecurePrefs.SetString(clavePersistencia, "");
        SecurePrefs.Save();

        if (coroutineTimer != null)
        {
            StopCoroutine(coroutineTimer);
            coroutineTimer = null;
        }
    }

    /// <summary>
    /// Coroutine que actualiza segundo a segundo el texto del botón mostrando el tiempo restante.
    /// </summary>
    private IEnumerator ActualizarTemporizadorCooldown()
    {
        while (DateTime.Now < fechaFinCooldown)
        {
            TimeSpan restante = fechaFinCooldown - DateTime.Now;
            
            if (textoBoton != null)
            {
                // Formato MM:SS
                textoBoton.text = string.Format("{0:D2}:{1:D2}", restante.Minutes, restante.Seconds);
            }

            yield return new WaitForSecondsRealtime(1f);
        }

        // El tiempo se ha cumplido
        DesactivarCooldown();
    }
}

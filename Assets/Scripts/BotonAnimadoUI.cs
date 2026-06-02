using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Permite que la animación de un botón se reproduzca visualmente
/// antes de ejecutar la acción real asociada a él.
/// </summary>
[RequireComponent(typeof(Button))]
public class BotonAnimadoUI : MonoBehaviour
{
    [Header("Configuración del Retraso")]
    [Tooltip("Tiempo en segundos que se esperará antes de ejecutar la acción (entre 0.15 y 0.2 segundos es recomendable para una transición fluida).")]
    public float tiempoEspera = 0.15f;

    [Header("Acciones aquí en lugar de en OnClick")]
    public UnityEvent AccionReal;

    private Button boton;

    void Start()
    {
        boton = GetComponent<Button>();
        
        // Hace que el botón original inicie el retraso cuando se pulse
        boton.onClick.AddListener(IniciarRetraso);
    }

    private void IniciarRetraso()
    {
        // Desactiva el botón temporalmente para evitar interacciones múltiples durante la reproducción de la animación
        boton.interactable = false;
        
        StartCoroutine(EsperarYEjecutar());
    }

    private IEnumerator EsperarYEjecutar()
    {
        // WaitForSecondsRealtime asegura que funcione aunque el juego esté pausado (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(tiempoEspera);
        
        // Ejecuta la acción configurada en el componente del Inspector
        AccionReal.Invoke();
        
        // Restablece la interactividad del botón
        boton.interactable = true;
    }
}

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
    [Tooltip("Tiempo en segundos que esperará antes de ejecutar la acción (0.15 a 0.2 suele verse muy bien).")]
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
        // Desactiva el botón temporalmente para que no hagan doble clic mientras hace la animación
        boton.interactable = false;
        
        StartCoroutine(EsperarYEjecutar());
    }

    private IEnumerator EsperarYEjecutar()
    {
        // WaitForSecondsRealtime asegura que funcione aunque el juego esté pausado (Time.timeScale = 0)
        yield return new WaitForSecondsRealtime(tiempoEspera);
        
        // Ejecuta la acción que has puesto en el Inspector
        AccionReal.Invoke();
        
        // Vuelve a activar el botón
        boton.interactable = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona la pantalla de carga inicial:
/// Llena el slider de progreso y mueve a una fila de personajes (ej. 3 gatos)
/// en tren de izquierda a derecha atravesando la pantalla al ritmo de la carga.
/// </summary>
public class PantallaDeCargaManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Panel contenedor de toda la pantalla de carga.")]
    public GameObject panelCarga;

    [Tooltip("CanvasGroup del panel para el desvanecimiento (Fade Out).")]
    public CanvasGroup canvasGroupCarga;

    [Tooltip("Slider que representa el progreso de la carga.")]
    public Slider sliderProgreso;

    [Tooltip("Lista de personajes que caminarán en fila (arrastra los 3 gatos aquí).")]
    public RectTransform[] personajesEnFila;

    [Tooltip("Texto opcional para porcentaje (déjalo vacío si no lo quieres).")]
    public TextMeshProUGUI textoProgreso;

    [Header("Recorrido de la Fila")]
    [Tooltip("Si está activo, la fila atraviesa de lado a lado de la pantalla completa.")]
    public bool atravesarTodaLaPantalla = true;

    [Tooltip("Posición horizontal de inicio para el líder (izquierda).")]
    public float posicionXInicio = -650f;

    [Tooltip("Posición horizontal final para el líder (derecha).")]
    public float posicionXFin = 650f;

    [Tooltip("Altura (Y) de la fila.")]
    public float posicionY = -230f;

    [Header("Configuración de Tiempos")]
    [Tooltip("Tiempo en segundos que tardará la fila en cruzar la pantalla.")]
    public float tiempoMinimoCarga = 3.0f;

    [Tooltip("Duración de la animación de desvanecimiento (Fade Out) al terminar.")]
    public float duracionFadeOut = 0.5f;

    [Header("Herramientas de Prueba")]
    [Tooltip("Tecla para relanzar la pantalla de carga en cualquier momento en Play Mode.")]
    public KeyCode teclaParaProbar = KeyCode.L;

    private float[] offsetsRelativosX;
    private Coroutine coroutineCarga;

    private void Awake()
    {
        if (panelCarga == null) panelCarga = this.gameObject;
        if (canvasGroupCarga == null) canvasGroupCarga = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        CalcularOffsetsFila();
        IniciarCarga();
    }

    private void Update()
    {
        // Pulsa 'L' en Play Mode para probar la animación y el recorrido cuantas veces quieras
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame)
        {
            IniciarCarga();
        }
#else
        if (Input.GetKeyDown(teclaParaProbar))
        {
            IniciarCarga();
        }
#endif
    }

    /// <summary>
    /// Guarda la distancia y separación exacta que tienen los gatos en la escena
    /// para que la mantengan perfectamente durante toda la caminata.
    /// </summary>
    private void CalcularOffsetsFila()
    {
        if (personajesEnFila != null && personajesEnFila.Length > 0)
        {
            offsetsRelativosX = new float[personajesEnFila.Length];
            float posXPrimero = personajesEnFila[0].anchoredPosition.x;

            for (int i = 0; i < personajesEnFila.Length; i++)
            {
                if (personajesEnFila[i] != null)
                {
                    // Distancia relativa respecto al primer gato de la fila
                    offsetsRelativosX[i] = personajesEnFila[i].anchoredPosition.x - posXPrimero;
                }
            }

            // Si no se asignó una Y personalizada, usar la altura que ya tienen en la escena
            if (personajesEnFila[0] != null)
            {
                posicionY = personajesEnFila[0].anchoredPosition.y;
            }
        }
    }

    [ContextMenu("Probar Pantalla de Carga")]
    public void IniciarCarga()
    {
        if (panelCarga != null)
        {
            panelCarga.SetActive(true);
            panelCarga.transform.SetAsLastSibling(); // Traer al frente
        }

        if (canvasGroupCarga != null)
        {
            canvasGroupCarga.alpha = 1f;
            canvasGroupCarga.blocksRaycasts = true;
        }

        if (offsetsRelativosX == null || offsetsRelativosX.Length == 0)
        {
            CalcularOffsetsFila();
        }

        ActualizarPosicionFila(0f);

        if (coroutineCarga != null) StopCoroutine(coroutineCarga);
        coroutineCarga = StartCoroutine(RutinaCargaInicial());
    }

    private IEnumerator RutinaCargaInicial()
    {
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoMinimoCarga)
        {
            tiempoTranscurrido += Time.unscaledDeltaTime;
            float progreso = Mathf.Clamp01(tiempoTranscurrido / tiempoMinimoCarga);

            // 1. Llenar el Slider
            if (sliderProgreso != null)
            {
                sliderProgreso.value = progreso;
            }

            // 2. Mover toda la fila de personajes sincronizada
            ActualizarPosicionFila(progreso);

            // 3. Texto opcional
            if (textoProgreso != null)
            {
                textoProgreso.text = $"{(int)(progreso * 100)}%";
            }

            yield return null;
        }

        // Fijar estado final al 100%
        if (sliderProgreso != null) sliderProgreso.value = 1f;
        ActualizarPosicionFila(1f);

        yield return new WaitForSecondsRealtime(0.2f);

        // 4. Desvanecimiento suave (Fade Out)
        if (canvasGroupCarga != null)
        {
            float tiempoFade = 0f;
            while (tiempoFade < duracionFadeOut)
            {
                tiempoFade += Time.unscaledDeltaTime;
                canvasGroupCarga.alpha = Mathf.Lerp(1f, 0f, tiempoFade / duracionFadeOut);
                yield return null;
            }
            canvasGroupCarga.blocksRaycasts = false;
        }

        // 5. Ocultar panel
        if (panelCarga != null)
        {
            panelCarga.SetActive(false);
        }

        coroutineCarga = null;
    }

    private void ActualizarPosicionFila(float progreso)
    {
        if (personajesEnFila == null || personajesEnFila.Length == 0) return;

        float posXBase = Mathf.Lerp(posicionXInicio, posicionXFin, progreso);

        for (int i = 0; i < personajesEnFila.Length; i++)
        {
            if (personajesEnFila[i] != null)
            {
                float offset = (offsetsRelativosX != null && i < offsetsRelativosX.Length) ? offsetsRelativosX[i] : 0f;
                personajesEnFila[i].anchoredPosition = new Vector2(posXBase + offset, posicionY);
            }
        }
    }
}

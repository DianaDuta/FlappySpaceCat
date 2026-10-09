using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crea un campo de destellos estelares (estrellas titilantes / constelaciones) parpadeando
/// suavemente en el fondo del Canvas UI.
/// Funciona en el Menú Principal con Time.unscaledDeltaTime (inmune a pausas del juego).
/// Cuenta con gestión de OnEnable/OnDisable para evitar que los destellos se queden congelados
/// al abrir o cerrar paneles y notificaciones.
/// </summary>
public class DestellosFondoUI : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS CONFIGURABLES
    // -----------------------------------------------------------------------------
    
    [Header("Configuración del Campo Estelar")]
    [Tooltip("Cantidad máxima de destellos parpadeando simultáneamente.")]
    [Range(5, 40)]
    public int cantidadDestellos = 18;

    [Tooltip("Tamaño mínimo de cada destello.")]
    public float tamanoMinimo = 10f;

    [Tooltip("Tamaño máximo de cada destello.")]
    public float tamanoMaximo = 26f;

    [Tooltip("Duración en segundos del ciclo de vida de un destello (aparición, brillo y desvanecimiento).")]
    public Vector2 duracionCiclo = new Vector2(1.5f, 3.5f);

    [Header("Colores Cósmicos")]
    [Tooltip("Variantes de color para las estrellitas de fondo.")]
    public Color[] coloresDestellos = new Color[]
    {
        new Color(1f, 1f, 1f, 0.9f),       // Blanco puro
        new Color(0.6f, 0.9f, 1f, 0.85f),  // Cyan suave
        new Color(0.95f, 0.7f, 1f, 0.85f), // Lavanda pastel
        new Color(1f, 0.95f, 0.7f, 0.85f)  // Dorado cálido
    };

    [Header("Sprite Opcional")]
    [Tooltip("Sprite con forma de destello de 4 puntas o estrella. Si está vacío, se generará una estrella de 4 puntas por código.")]
    public Sprite spriteDestello;

    private Sprite spriteProceduralDestello;
    private RectTransform rectTransformPadre;
    private List<Coroutine> corrutinasDestellos = new List<Coroutine>();

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    void Awake()
    {
        rectTransformPadre = GetComponent<RectTransform>();
        CrearTexturaEstrella4Puntas();
    }

    void OnEnable()
    {
        ReiniciarDestellos();
    }

    void OnDisable()
    {
        LimpiarYDetener();
    }

    private void LimpiarYDetener()
    {
        StopAllCoroutines();
        corrutinasDestellos.Clear();

        // Destruir cualquier destello huérfano
        foreach (Transform hijo in transform)
        {
            if (hijo.name.StartsWith("Destello_UI"))
            {
                Destroy(hijo.gameObject);
            }
        }
    }

    public void ReiniciarDestellos()
    {
        LimpiarYDetener();

        for (int i = 0; i < cantidadDestellos; i++)
        {
            Coroutine c = StartCoroutine(CicloVidaDestello(i * 0.15f));
            corrutinasDestellos.Add(c);
        }
    }

    /// <summary>
    /// Genera un sprite nítido de destello cósmico de 4 puntas.
    /// </summary>
    private void CrearTexturaEstrella4Puntas()
    {
        if (spriteDestello != null) return;

        int res = 64;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2(res * 0.5f, res * 0.5f);
        float radio = res * 0.5f;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx = Mathf.Abs(x - centro.x) / radio;
                float dy = Mathf.Abs(y - centro.y) / radio;

                // Ecuación de cruz/destello de 4 puntas suave
                float intensidadHorizontal = Mathf.Clamp01(1f - (dy * 6f)) * Mathf.Clamp01(1f - dx);
                float intensidadVertical = Mathf.Clamp01(1f - (dx * 6f)) * Mathf.Clamp01(1f - dy);
                float brilloCentro = Mathf.Clamp01(1f - (Vector2.Distance(new Vector2(x, y), centro) / (radio * 0.4f)));

                float alpha = Mathf.Clamp01(intensidadHorizontal + intensidadVertical + brilloCentro);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        spriteProceduralDestello = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    private IEnumerator CicloVidaDestello(float retrasoInicial)
    {
        if (retrasoInicial > 0f)
        {
            float tInicial = 0f;
            while (tInicial < retrasoInicial)
            {
                tInicial += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // Crear el objeto UI para este destello
        GameObject obj = new GameObject("Destello_UI", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(transform, false);
        obj.transform.SetAsFirstSibling(); // Asegurar que parpadea por detrás de botones y textos

        RectTransform rt = obj.GetComponent<RectTransform>();
        Image img = obj.GetComponent<Image>();
        img.sprite = spriteDestello != null ? spriteDestello : spriteProceduralDestello;
        img.raycastTarget = false;

        while (true)
        {
            // 1. Posición aleatoria dentro de los límites del Canvas
            Vector2 tamanoCanvas = rectTransformPadre != null ? rectTransformPadre.rect.size : new Vector2(1920, 1080);
            float posX = Random.Range(-tamanoCanvas.x * 0.48f, tamanoCanvas.x * 0.48f);
            float posY = Random.Range(-tamanoCanvas.y * 0.48f, tamanoCanvas.y * 0.48f);
            rt.anchoredPosition = new Vector2(posX, posY);

            // 2. Tamaño y color aleatorio
            float tamano = Random.Range(tamanoMinimo, tamanoMaximo);
            rt.sizeDelta = new Vector2(tamano, tamano);

            Color colorElegido = coloresDestellos != null && coloresDestellos.Length > 0
                ? coloresDestellos[Random.Range(0, coloresDestellos.Length)]
                : Color.white;

            float duracion = Random.Range(duracionCiclo.x, duracionCiclo.y);
            float rotacionInicial = Random.Range(0f, 90f);
            float velocidadRotacion = Random.Range(-45f, 45f);

            // 3. Animación de titileo (Fade In -> Brillo -> Fade Out)
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.unscaledDeltaTime;
                float progreso = Mathf.Clamp01(tiempo / duracion);

                // Curva suave de campana para el alfa y la escala
                float escalaAlpha = Mathf.Sin(progreso * Mathf.PI);

                img.color = new Color(colorElegido.r, colorElegido.g, colorElegido.b, escalaAlpha * colorElegido.a);
                rt.localRotation = Quaternion.Euler(0, 0, rotacionInicial + (velocidadRotacion * progreso));
                rt.localScale = Vector3.one * (0.5f + (escalaAlpha * 0.5f));

                yield return null;
            }

            img.color = new Color(colorElegido.r, colorElegido.g, colorElegido.b, 0f);

            // 4. Pequeña pausa aleatoria antes de renacer en otra posición
            float espera = Random.Range(0.5f, 2.5f);
            float tPausa = 0f;
            while (tPausa < espera)
            {
                tPausa += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}

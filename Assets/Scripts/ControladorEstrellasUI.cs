using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Genera estrellas fugaces en el Canvas de la UI (Screen Space - Overlay).
/// Configurado con una velocidad lenta, suave y cinematográfica (5 a 7.5 segundos por trayecto),
/// rica paleta neón multicolor y predominancia de trayectorias de Izquierda a Derecha.
/// </summary>
public class ControladorEstrellasUI : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // ESTRUCTURA DE DATOS
    // -----------------------------------------------------------------------------
    
    [System.Serializable]
    public class DatosEstrellaUI
    {
        [Header("Identificación")]
        public string nombre = "Estrella Rosa";

        [Header("Tiempos de Frecuencia (Segundos)")]
        public float tiempoMinimo = 5f;
        public float tiempoMaximo = 12f;

        [Header("Trayectoria en Canvas (Coordenadas locales UI)")]
        public Vector2 puntoInicio = new Vector2(-1050f, 450f);
        public Vector2 puntoFin = new Vector2(1050f, -350f);

        [Header("Movimiento y Aspecto")]
        [Tooltip("Duración en segundos del viaje (valores más altos = más lenta, serena y majestuosa).")]
        public float duracionVuelo = 6.0f;
        
        [Tooltip("Color de la cabeza y estela neón.")]
        public Color colorEstrella = new Color(1f, 0.35f, 0.85f, 1f);

        [Tooltip("Grosor / Tamaño del núcleo brillante.")]
        public float tamanoCabeza = 16f;

        [Tooltip("Longitud visual de la cola luminosa.")]
        public float longitudCola = 360f;
    }

    // -----------------------------------------------------------------------------
    // CONFIGURACIÓN DEL GENERADOR
    // -----------------------------------------------------------------------------

    [Header("Primera Estrella Obligatoria")]
    [Tooltip("Tiempo en segundos para lanzar la primera estrella al iniciar.")]
    public float tiempoPrimeraEstrella = 2.0f;

    [Header("Sprite Opcional")]
    public Sprite spriteEstrella;

    [Header("Lista de Estrellas (Velocidad Lenta y Predominio Izq -> Der)")]
    public List<DatosEstrellaUI> estrellas = new List<DatosEstrellaUI>()
    {
        // 1. [IZQ -> DER] Rosa Neón (Diagonal descendente suave)
        new DatosEstrellaUI() {
            nombre = "Rosa Neón (Izq -> Der)",
            tiempoMinimo = 5f,
            tiempoMaximo = 11f,
            puntoInicio = new Vector2(-1050f, 480f),
            puntoFin = new Vector2(1050f, -350f),
            duracionVuelo = 6.2f,
            colorEstrella = new Color(1f, 0.35f, 0.85f, 0.95f),
            tamanoCabeza = 16f,
            longitudCola = 380f
        },
        // 2. [IZQ -> DER] Cyan Cósmico (Diagonal ascendente suave)
        new DatosEstrellaUI() {
            nombre = "Cyan Cósmico (Izq -> Der)",
            tiempoMinimo = 6f,
            tiempoMaximo = 13f,
            puntoInicio = new Vector2(-1050f, -250f),
            puntoFin = new Vector2(1050f, 450f),
            duracionVuelo = 5.8f,
            colorEstrella = new Color(0f, 0.95f, 1f, 0.95f),
            tamanoCabeza = 15f,
            longitudCola = 360f
        },
        // 3. [IZQ -> DER] Dorado Solar (Cruce central suave)
        new DatosEstrellaUI() {
            nombre = "Dorado Solar (Izq -> Der)",
            tiempoMinimo = 7f,
            tiempoMaximo = 15f,
            puntoInicio = new Vector2(-1050f, 300f),
            puntoFin = new Vector2(1050f, -150f),
            duracionVuelo = 6.8f,
            colorEstrella = new Color(1f, 0.85f, 0.25f, 0.95f),
            tamanoCabeza = 16f,
            longitudCola = 400f
        },
        // 4. [IZQ -> DER] Menta / Esmeralda Neón (Diagonal larga)
        new DatosEstrellaUI() {
            nombre = "Esmeralda Neón (Izq -> Der)",
            tiempoMinimo = 8f,
            tiempoMaximo = 16f,
            puntoInicio = new Vector2(-1050f, -400f),
            puntoFin = new Vector2(1050f, 300f),
            duracionVuelo = 6.5f,
            colorEstrella = new Color(0.2f, 1f, 0.65f, 0.95f),
            tamanoCabeza = 14f,
            longitudCola = 350f
        },
        // 5. [IZQ -> DER] Naranja Fuego Astral (Diagonal completa majestuosa)
        new DatosEstrellaUI() {
            nombre = "Naranja Solar (Izq -> Der)",
            tiempoMinimo = 10f,
            tiempoMaximo = 18f,
            puntoInicio = new Vector2(-1050f, 550f),
            puntoFin = new Vector2(1050f, -500f),
            duracionVuelo = 7.5f,
            colorEstrella = new Color(1f, 0.55f, 0.15f, 0.95f),
            tamanoCabeza = 17f,
            longitudCola = 420f
        },
        // 6. [DER -> IZQ] Morado / Ultravioleta (Secundaria)
        new DatosEstrellaUI() {
            nombre = "Morado Neón (Der -> Izq)",
            tiempoMinimo = 9f,
            tiempoMaximo = 17f,
            puntoInicio = new Vector2(1050f, 500f),
            puntoFin = new Vector2(-1050f, -450f),
            duracionVuelo = 6.6f,
            colorEstrella = new Color(0.75f, 0.35f, 1f, 0.9f),
            tamanoCabeza = 15f,
            longitudCola = 370f
        },
        // 7. [DER -> IZQ] Blanco Diamante (Secundaria suave)
        new DatosEstrellaUI() {
            nombre = "Blanco Diamante (Der -> Izq)",
            tiempoMinimo = 12f,
            tiempoMaximo = 20f,
            puntoInicio = new Vector2(1050f, 200f),
            puntoFin = new Vector2(-1050f, -200f),
            duracionVuelo = 5.5f,
            colorEstrella = new Color(0.9f, 0.97f, 1f, 0.95f),
            tamanoCabeza = 14f,
            longitudCola = 340f
        }
    };

    private Sprite spritePorDefecto;
    private Sprite spriteColaGradiente;
    private List<Coroutine> corrutinasActivas = new List<Coroutine>();

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    void Awake()
    {
        CrearSpritesProcedurales();
    }

    void OnEnable()
    {
        ReiniciarBucleEstrellas();
    }

    void OnDisable()
    {
        LimpiarYDetener();
    }

    private void LimpiarYDetener()
    {
        StopAllCoroutines();
        corrutinasActivas.Clear();

        foreach (Transform hijo in transform)
        {
            if (hijo.name.StartsWith("EstrellaFugaz_"))
            {
                Destroy(hijo.gameObject);
            }
        }
    }

    public void ReiniciarBucleEstrellas()
    {
        LimpiarYDetener();

        if (estrellas != null && estrellas.Count > 0)
        {
            // 1. Primera estrella obligatoria al segundo 2 (Rosa Neón Izq -> Der)
            Coroutine cInicial = StartCoroutine(LanzarPrimeraEstrellaObligatoria());
            corrutinasActivas.Add(cInicial);

            // 2. Bucle periódico para todas las estrellas
            foreach (var estrella in estrellas)
            {
                Coroutine c = StartCoroutine(BucleEstrella(estrella, true));
                corrutinasActivas.Add(c);
            }
        }
    }

    private IEnumerator LanzarPrimeraEstrellaObligatoria()
    {
        float transcurrido = 0f;
        while (transcurrido < tiempoPrimeraEstrella)
        {
            transcurrido += Time.unscaledDeltaTime;
            yield return null;
        }

        if (estrellas.Count > 0)
        {
            StartCoroutine(DispararEstrella(estrellas[0]));
        }
    }

    private void CrearSpritesProcedurales()
    {
        if (spritePorDefecto == null && spriteEstrella == null)
        {
            int res = 64;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            Vector2 centro = new Vector2(res * 0.5f, res * 0.5f);
            float radioMax = res * 0.5f;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), centro);
                    float alpha = Mathf.Clamp01(1f - (dist / radioMax));
                    alpha = Mathf.Pow(alpha, 2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            spritePorDefecto = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        }

        if (spriteColaGradiente == null)
        {
            int anchoCola = 128;
            int altoCola = 16;
            Texture2D texCola = new Texture2D(anchoCola, altoCola, TextureFormat.RGBA32, false);
            for (int y = 0; y < altoCola; y++)
            {
                float factorY = 1f - Mathf.Abs((y - (altoCola * 0.5f)) / (altoCola * 0.5f));
                for (int x = 0; x < anchoCola; x++)
                {
                    float factorX = (float)x / anchoCola; // 0 en la punta trasera, 1 en la cabeza
                    float alpha = Mathf.Pow(factorX, 1.4f) * Mathf.Pow(factorY, 1.8f);
                    texCola.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            texCola.Apply();
            spriteColaGradiente = Sprite.Create(texCola, new Rect(0, 0, anchoCola, altoCola), new Vector2(1f, 0.5f));
        }
    }

    private IEnumerator BucleEstrella(DatosEstrellaUI datos, bool esInicio = false)
    {
        float esperaInicial = esInicio ? (tiempoPrimeraEstrella + Random.Range(datos.tiempoMinimo, datos.tiempoMaximo)) : Random.Range(datos.tiempoMinimo, datos.tiempoMaximo);
        
        float tEspera = 0f;
        while (tEspera < esperaInicial)
        {
            tEspera += Time.unscaledDeltaTime;
            yield return null;
        }

        StartCoroutine(DispararEstrella(datos));

        while (true)
        {
            float espera = Random.Range(datos.tiempoMinimo, datos.tiempoMaximo);
            float transcurrido = 0f;
            while (transcurrido < espera)
            {
                transcurrido += Time.unscaledDeltaTime;
                yield return null;
            }

            StartCoroutine(DispararEstrella(datos));
        }
    }

    private IEnumerator DispararEstrella(DatosEstrellaUI datos)
    {
        // 1. Crear contenedor de la estrella fugaz
        GameObject objEstrella = new GameObject("EstrellaFugaz_" + datos.nombre, typeof(RectTransform));
        objEstrella.transform.SetParent(transform, false);
        objEstrella.transform.SetAsFirstSibling(); // Asegurar que viaja por detrás de los botones y textos

        RectTransform rectContenedor = objEstrella.GetComponent<RectTransform>();
        rectContenedor.anchoredPosition = datos.puntoInicio;

        // Calcular ángulo de vuelo para orientar la cola
        Vector2 direccion = (datos.puntoFin - datos.puntoInicio).normalized;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        rectContenedor.localRotation = Quaternion.Euler(0, 0, angulo);

        // 2. Crear Cola / Estela Luminosa con gradiente suave
        GameObject objCola = new GameObject("Cola", typeof(RectTransform), typeof(Image));
        objCola.transform.SetParent(objEstrella.transform, false);

        RectTransform rectCola = objCola.GetComponent<RectTransform>();
        rectCola.pivot = new Vector2(1f, 0.5f);
        rectCola.anchoredPosition = Vector2.zero;
        rectCola.sizeDelta = new Vector2(datos.longitudCola, datos.tamanoCabeza * 0.9f);

        Image imgCola = objCola.GetComponent<Image>();
        imgCola.sprite = spriteColaGradiente;
        imgCola.raycastTarget = false;
        imgCola.color = datos.colorEstrella;

        // 3. Crear Cabeza brillante (Núcleo)
        GameObject objCabeza = new GameObject("Cabeza", typeof(RectTransform), typeof(Image));
        objCabeza.transform.SetParent(objEstrella.transform, false);

        RectTransform rectCabeza = objCabeza.GetComponent<RectTransform>();
        rectCabeza.anchoredPosition = Vector2.zero;
        rectCabeza.sizeDelta = new Vector2(datos.tamanoCabeza * 1.6f, datos.tamanoCabeza * 1.6f);

        Image imgCabeza = objCabeza.GetComponent<Image>();
        imgCabeza.sprite = spriteEstrella != null ? spriteEstrella : spritePorDefecto;
        imgCabeza.raycastTarget = false;
        imgCabeza.color = Color.white;

        // 4. Animación de vuelo suave y relajante
        float tiempo = 0f;
        while (tiempo < datos.duracionVuelo)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / datos.duracionVuelo);

            rectContenedor.anchoredPosition = Vector2.Lerp(datos.puntoInicio, datos.puntoFin, t);

            // Curva de desvanecimiento suave (fade in inicial, fade out final)
            float alpha = 1f;
            if (t < 0.12f) alpha = t / 0.12f;
            else if (t > 0.78f) alpha = (1f - t) / 0.22f;

            imgCabeza.color = new Color(1f, 1f, 1f, alpha);
            imgCola.color = new Color(datos.colorEstrella.r, datos.colorEstrella.g, datos.colorEstrella.b, alpha * datos.colorEstrella.a);

            yield return null;
        }

        Destroy(objEstrella);
    }
}

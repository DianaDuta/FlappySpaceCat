using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona las estrellas fugaces en el espacio 3D/Gameplay del juego.
/// Configurado con una velocidad lenta, serena y suave (5 a 7.5 segundos por trayecto),
/// rica paleta neón multicolor y predominancia de trayectorias de Izquierda a Derecha.
/// </summary>
public class ControladorEstrella3D : Generador
{
    [System.Serializable]
    public class DatosEstrellaFugaz
    {
        [Header("Identificación")]
        public string nombreIdentificador = "Estrella Fugaz";

        [Header("Tiempos de Aparición")]
        public float tiempoMinimo = 5f;
        public float tiempoMaximo = 14f;

        [Header("Trayectoria (Coordenadas del mundo)")]
        public Vector3 puntoInicio = new Vector3(-12f, 6f, 0f);
        public Vector3 puntoFin = new Vector3(12f, -5f, 0f);
        
        [Tooltip("Duración en segundos del viaje (valores más altos = más lenta, serena y majestuosa).")]
        public float duracionVuelo = 6.2f;

        [Header("Aspecto Visual Neón")]
        [Tooltip("Color neón principal de la estrella y su estela.")]
        public Color colorEstrella = new Color(1f, 0.35f, 0.85f, 0.95f);

        [Tooltip("Grosor / Tamaño del núcleo de la cabeza.")]
        public float tamanoCabeza = 0.35f;

        [Tooltip("Longitud de la cola luminosa.")]
        public float longitudCola = 7.5f;
    }

    [Header("Primera Estrella Obligatoria")]
    [Tooltip("Tiempo en segundos tras iniciar para lanzar la primera estrella garantizada.")]
    public float tiempoPrimeraEstrella = 2.0f;

    [Header("Sprite Opcional")]
    public Sprite spriteCabeza;

    [Header("Capa de Renderizado y Profundidad")]
    [Tooltip("Orden de renderizado 2D para colocarse delante del fondo (-10) y detrás del jugador y obstáculos (0).")]
    public int sortingOrderCola = -5;
    public int sortingOrderCabeza = -4;

    [Tooltip("Profundidad Z en el espacio 3D para volar detrás del plano del jugador (Z=0).")]
    public float zProfundidad = 1.0f;

    [Header("Lista de Estrellas (Velocidad Lenta y Predominio Izq -> Der)")]
    public List<DatosEstrellaFugaz> estrellasFugaces = new List<DatosEstrellaFugaz>()
    {
        // 1. [IZQ -> DER] Rosa Neón
        new DatosEstrellaFugaz() {
            nombreIdentificador = "Rosa Neón (Izq -> Der)",
            tiempoMinimo = 5f,
            tiempoMaximo = 11f,
            puntoInicio = new Vector3(-12f, 6f, 0f),
            puntoFin = new Vector3(12f, -5f, 0f),
            duracionVuelo = 6.2f,
            colorEstrella = new Color(1f, 0.35f, 0.85f, 0.95f),
            tamanoCabeza = 0.35f,
            longitudCola = 7.5f
        },
        // 2. [IZQ -> DER] Cyan Cósmico
        new DatosEstrellaFugaz() {
            nombreIdentificador = "Cyan Cósmico (Izq -> Der)",
            tiempoMinimo = 6f,
            tiempoMaximo = 13f,
            puntoInicio = new Vector3(-12f, -4f, 0f),
            puntoFin = new Vector3(12f, 6f, 0f),
            duracionVuelo = 5.8f,
            colorEstrella = new Color(0f, 0.95f, 1f, 0.95f),
            tamanoCabeza = 0.32f,
            longitudCola = 7.2f
        },
        // 3. [IZQ -> DER] Dorado Solar
        new DatosEstrellaFugaz() {
            nombreIdentificador = "Dorado Solar (Izq -> Der)",
            tiempoMinimo = 7f,
            tiempoMaximo = 15f,
            puntoInicio = new Vector3(-12f, 4f, 0f),
            puntoFin = new Vector3(12f, -2f, 0f),
            duracionVuelo = 6.8f,
            colorEstrella = new Color(1f, 0.85f, 0.25f, 0.95f),
            tamanoCabeza = 0.34f,
            longitudCola = 7.8f
        },
        // 4. [IZQ -> DER] Esmeralda / Menta Neón
        new DatosEstrellaFugaz() {
            nombreIdentificador = "Esmeralda Neón (Izq -> Der)",
            tiempoMinimo = 8f,
            tiempoMaximo = 16f,
            puntoInicio = new Vector3(-12f, -5f, 0f),
            puntoFin = new Vector3(12f, 4f, 0f),
            duracionVuelo = 6.5f,
            colorEstrella = new Color(0.2f, 1f, 0.65f, 0.95f),
            tamanoCabeza = 0.3f,
            longitudCola = 7.0f
        },
        // 5. [IZQ -> DER] Naranja Solar
        new DatosEstrellaFugaz() {
            nombreIdentificador = "Naranja Solar (Izq -> Der)",
            tiempoMinimo = 10f,
            tiempoMaximo = 18f,
            puntoInicio = new Vector3(-12f, 7f, 0f),
            puntoFin = new Vector3(12f, -6f, 0f),
            duracionVuelo = 7.5f,
            colorEstrella = new Color(1f, 0.55f, 0.15f, 0.95f),
            tamanoCabeza = 0.36f,
            longitudCola = 8.2f
        },
        // 6. [DER -> IZQ] Morado Neón (Secundaria)
        new DatosEstrellaFugaz() {
            nombreIdentificador = "Morado Neón (Der -> Izq)",
            tiempoMinimo = 9f,
            tiempoMaximo = 17f,
            puntoInicio = new Vector3(12f, 6f, 0f),
            puntoFin = new Vector3(-12f, -5f, 0f),
            duracionVuelo = 6.6f,
            colorEstrella = new Color(0.75f, 0.35f, 1f, 0.9f),
            tamanoCabeza = 0.32f,
            longitudCola = 7.4f
        },
        // 7. [DER -> IZQ] Blanco Diamante (Secundaria suave)
        new DatosEstrellaFugaz() {
            nombreIdentificador = "Blanco Diamante (Der -> Izq)",
            tiempoMinimo = 12f,
            tiempoMaximo = 20f,
            puntoInicio = new Vector3(12f, 2f, 0f),
            puntoFin = new Vector3(-12f, -2f, 0f),
            duracionVuelo = 5.5f,
            colorEstrella = new Color(0.9f, 0.97f, 1f, 0.95f),
            tamanoCabeza = 0.28f,
            longitudCola = 6.8f
        }
    };

    private Sprite spriteProceduralCabeza;
    private Sprite spriteProceduralCola;
    private List<Coroutine> corrutinasActivas = new List<Coroutine>();

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
        DetenerYLimpiar();
    }

    private void DetenerYLimpiar()
    {
        StopAllCoroutines();
        corrutinasActivas.Clear();

        foreach (Transform hijo in transform)
        {
            if (hijo.name.StartsWith("Estrella3D_"))
            {
                Destroy(hijo.gameObject);
            }
        }
    }

    public void ReiniciarBucleEstrellas()
    {
        DetenerYLimpiar();

        if (estrellasFugaces != null && estrellasFugaces.Count > 0)
        {
            // 1. Primera estrella obligatoria al segundo 2
            Coroutine cInicial = StartCoroutine(LanzarPrimeraEstrellaObligatoria());
            corrutinasActivas.Add(cInicial);

            // 2. Bucle periódico para todas las estrellas
            foreach (var estrella in estrellasFugaces)
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
            transcurrido += Time.deltaTime;
            yield return null;
        }

        if (estrellasFugaces.Count > 0)
        {
            StartCoroutine(VolarEstrella(estrellasFugaces[0]));
        }
    }

    private void CrearSpritesProcedurales()
    {
        if (spriteProceduralCabeza == null)
        {
            int res = 64;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            Vector2 centro = new Vector2(res * 0.5f, res * 0.5f);
            float radio = res * 0.5f;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), centro);
                    float alpha = Mathf.Clamp01(1f - (dist / radio));
                    alpha = Mathf.Pow(alpha, 2.2f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            spriteProceduralCabeza = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }

        if (spriteProceduralCola == null)
        {
            int ancho = 128;
            int alto = 16;
            Texture2D texCola = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
            for (int y = 0; y < alto; y++)
            {
                float factorY = 1f - Mathf.Abs((y - (alto * 0.5f)) / (alto * 0.5f));
                for (int x = 0; x < ancho; x++)
                {
                    float factorX = (float)x / ancho;
                    float alpha = Mathf.Pow(factorX, 1.4f) * Mathf.Pow(factorY, 1.8f);
                    texCola.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            texCola.Apply();
            spriteProceduralCola = Sprite.Create(texCola, new Rect(0, 0, ancho, alto), new Vector2(1f, 0.5f), 100f);
        }
    }

    public override void Generar(float posicionX, float posicionY)
    {
        if (estrellasFugaces.Count > 0)
        {
            int indice = Random.Range(0, estrellasFugaces.Count);
            StartCoroutine(VolarEstrella(estrellasFugaces[indice]));
        }
    }

    private IEnumerator BucleEstrella(DatosEstrellaFugaz datos, bool esInicio = false)
    {
        float esperaInicial = esInicio ? (tiempoPrimeraEstrella + Random.Range(datos.tiempoMinimo, datos.tiempoMaximo)) : Random.Range(datos.tiempoMinimo, datos.tiempoMaximo);

        float tEspera = 0f;
        while (tEspera < esperaInicial)
        {
            tEspera += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(VolarEstrella(datos));

        while (true)
        {
            float espera = Random.Range(datos.tiempoMinimo, datos.tiempoMaximo);
            
            float transcurrido = 0f;
            while (transcurrido < espera)
            {
                transcurrido += Time.deltaTime;
                yield return null;
            }

            StartCoroutine(VolarEstrella(datos));
        }
    }

    private IEnumerator VolarEstrella(DatosEstrellaFugaz datos)
    {
        Vector3 inicioConZ = new Vector3(datos.puntoInicio.x, datos.puntoInicio.y, zProfundidad);
        Vector3 finConZ = new Vector3(datos.puntoFin.x, datos.puntoFin.y, zProfundidad);

        // 1. Contenedor principal de la estrella
        GameObject objEstrella = new GameObject("Estrella3D_" + datos.nombreIdentificador);
        objEstrella.transform.SetParent(transform, false);
        objEstrella.transform.position = inicioConZ;

        // Calcular ángulo para orientar la cola de luz suave
        Vector3 dir = (finConZ - inicioConZ).normalized;
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        objEstrella.transform.rotation = Quaternion.Euler(0, 0, angulo);

        // 2. Cola luminosa degradada
        GameObject objCola = new GameObject("ColaLuminosa", typeof(SpriteRenderer));
        objCola.transform.SetParent(objEstrella.transform, false);
        objCola.transform.localPosition = Vector3.zero;
        objCola.transform.localRotation = Quaternion.identity;
        objCola.transform.localScale = new Vector3(datos.longitudCola, datos.tamanoCabeza * 1.8f, 1f);

        SpriteRenderer srCola = objCola.GetComponent<SpriteRenderer>();
        srCola.sprite = spriteProceduralCola;
        srCola.color = datos.colorEstrella;
        srCola.sortingOrder = sortingOrderCola;

        // 3. Cabeza brillante (núcleo)
        GameObject objCabeza = new GameObject("CabezaGlow", typeof(SpriteRenderer));
        objCabeza.transform.SetParent(objEstrella.transform, false);
        objCabeza.transform.localPosition = Vector3.zero;
        objCabeza.transform.localRotation = Quaternion.identity;
        objCabeza.transform.localScale = new Vector3(datos.tamanoCabeza * 1.5f, datos.tamanoCabeza * 1.5f, 1f);

        SpriteRenderer srCabeza = objCabeza.GetComponent<SpriteRenderer>();
        srCabeza.sprite = spriteCabeza != null ? spriteCabeza : spriteProceduralCabeza;
        srCabeza.color = Color.white;
        srCabeza.sortingOrder = sortingOrderCabeza;

        // 4. Vuelo lineal con velocidad lenta, suave y relajante
        float tiempo = 0f;
        while (tiempo < datos.duracionVuelo)
        {
            if (objEstrella == null) yield break;

            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / datos.duracionVuelo);

            if (objEstrella != null)
            {
                objEstrella.transform.position = Vector3.Lerp(inicioConZ, finConZ, t);
            }

            // Curva de opacidad (fade in al nacer, fade out al llegar)
            float alpha = 1f;
            if (t < 0.12f) alpha = t / 0.12f;
            else if (t > 0.78f) alpha = (1f - t) / 0.22f;

            if (srCabeza != null) srCabeza.color = new Color(1f, 1f, 1f, alpha);
            if (srCola != null) srCola.color = new Color(datos.colorEstrella.r, datos.colorEstrella.g, datos.colorEstrella.b, alpha * datos.colorEstrella.a);

            yield return null;
        }

        if (objEstrella != null)
        {
            Destroy(objEstrella);
        }
    }
}

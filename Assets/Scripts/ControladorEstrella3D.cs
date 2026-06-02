using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona la aparición asíncrona y el desplazamiento lineal de múltiples estrellas fugaces
/// representadas exclusivamente por rastros continuos (Trail Renderers).
/// Permite configurar diferentes colores, trayectorias y frecuencias de tiempo desde el Inspector.
/// </summary>
public class ControladorEstrella3D : Generador
{
    [System.Serializable]
    public class DatosEstrellaFugaz
    {
        [Header("Identificación")]
        [Tooltip("Nombre descriptivo para identificar esta estrella fugaz en el Inspector.")]
        public string nombreIdentificador = "Estrella Fugaz";

        [Header("Tiempos de Aparición")]
        [Tooltip("Tiempo mínimo de espera en segundos entre apariciones de esta estrella.")]
        public float tiempoMinimo = 5f;
        [Tooltip("Tiempo máximo de espera en segundos entre apariciones de esta estrella.")]
        public float tiempoMaximo = 15f;

        [Header("Trayectoria (Coordenadas del mundo)")]
        [Tooltip("Punto de inicio del trayecto en coordenadas del mundo (ej: arriba a la derecha).")]
        public Vector3 puntoInicio = new Vector3(10f, 6f, 0f);
        [Tooltip("Punto de finalización del trayecto en coordenadas del mundo (ej: abajo a la izquierda).")]
        public Vector3 puntoFin = new Vector3(-10f, -6f, 0f);
        
        [Tooltip("Duración en segundos del viaje desde el punto de inicio hasta el punto final.")]
        public float duracionVuelo = 1.2f;

        [Header("Aspecto Visual")]
        [Tooltip("Gradiente de color para este rastro de luz cósmica.")]
        public Gradient colorEstela;

        [Tooltip("Grosor inicial de la estela en su punto de nacimiento.")]
        public float grosorEstela = 0.4f;
    }

    [Header("Lista de Estrellas Fugaces")]
    [Tooltip("Define aquí tantas estrellas fugaces independientes como desees en tu fondo espacial.")]
    public List<DatosEstrellaFugaz> estrellasFugaces = new List<DatosEstrellaFugaz>();

    void Start()
    {
        if (prefab == null)
        {
            Debug.LogError("⚠️ [ControladorEstrella3D] No se ha asignado el Prefab de la Estela en el Inspector.");
            return;
        }

        // Se inicia un bucle de aparición independiente y paralelo para cada estrella configurada
        foreach (DatosEstrellaFugaz estrella in estrellasFugaces)
        {
            StartCoroutine(BucleEstrellaFugaz(estrella));
        }
    }

    /// <summary>
    /// Cumple el contrato de la clase abstracta base Generador.
    /// Despacha el vuelo de una estrella fugaz aleatoria de la lista configurada.
    /// </summary>
    /// <param name="posicionX">Coordenada horizontal (omitida al estar predefinida por trayectoria).</param>
    /// <param name="posicionY">Coordenada vertical (omitida al estar predefinida por trayectoria).</param>
    public override void Generar(float posicionX, float posicionY)
    {
        if (estrellasFugaces.Count > 0)
        {
            int indiceAleatorio = Random.Range(0, estrellasFugaces.Count);
            StartCoroutine(VolarEstrella(estrellasFugaces[indiceAleatorio]));
        }
    }

    /// <summary>
    /// Corrutina individual para cada estrella fugaz que gestiona su temporizador aleatorio.
    /// </summary>
    IEnumerator BucleEstrellaFugaz(DatosEstrellaFugaz datos)
    {
        while (true)
        {
            // Espera un intervalo aleatorio de tiempo configurado específicamente para esta estrella
            float tiempoEspera = Random.Range(datos.tiempoMinimo, datos.tiempoMaximo);
            yield return new WaitForSeconds(tiempoEspera);

            // Inicia el vuelo de la estrella fugaz
            StartCoroutine(VolarEstrella(datos));
        }
    }

    /// <summary>
    /// Instancia el rastro y lo desplaza suavemente de puntoInicio a puntoFin usando interpolación lineal (Lerp).
    /// Espera a que la cola se desvanezca por completo antes de destruir el objeto para optimizar el rendimiento.
    /// </summary>
    IEnumerator VolarEstrella(DatosEstrellaFugaz datos)
    {
        // 1. Instancia el Trail a partir del Prefab en las coordenadas de inicio
        GameObject estrellaInstancia = Instantiate(prefab, datos.puntoInicio, Quaternion.identity);
        estrellaInstancia.transform.SetParent(transform);

        TrailRenderer tr = estrellaInstancia.GetComponent<TrailRenderer>();
        if (tr != null)
        {
            // 2. Sobrescribe la curva de grosor de forma decreciente hacia cero
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, datos.grosorEstela);
            curve.AddKey(1.0f, 0.0f);
            tr.widthCurve = curve;

            // 3. Aplica el degradado personalizado configurado en el Inspector
            if (datos.colorEstela != null)
            {
                tr.colorGradient = datos.colorEstela;
            }
            
            // Se limpia cualquier residuo previo al instanciarse
            tr.Clear();
        }

        // 4. Desplaza el objeto a lo largo del tiempo de vuelo establecido
        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < datos.duracionVuelo)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / datos.duracionVuelo;
            
            // Movimiento suave lineal de punto de inicio a punto de fin
            estrellaInstancia.transform.position = Vector3.Lerp(datos.puntoInicio, datos.puntoFin, t);
            
            yield return null;
        }

        // Se asegura el posicionamiento final exacto del objeto
        estrellaInstancia.transform.position = datos.puntoFin;

        // 5. Espera a que el rastro se desvanezca por completo antes de su eliminación
        float tiempoDisolucion = tr != null ? tr.time : 1f;
        yield return new WaitForSeconds(tiempoDisolucion);

        // 6. Eliminación física del objeto de la jerarquía para liberar memoria
        Destroy(estrellaInstancia);
    }
}

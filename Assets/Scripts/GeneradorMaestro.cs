using UnityEngine;

/// <summary>
/// Gestiona el bucle algorítmico principal responsable del momento y espacio
/// en que los obstáculos emergen durante el ciclo de juego, adaptando las secuencias de spawn
/// proporcionalmente a la dificultad dinámica del sistema central.
/// </summary>
public class GeneradorMaestro : MonoBehaviour
{
    // ----------------------------------------------------------------------
    // CAMPOS
    // ----------------------------------------------------------------------
    public GeneradorObstaculo genObstaculos;
    
    [Header("Límites de Altura del Obstáculo")]
    public float limiteInferiorY = -2.5f;
    public float limiteSuperiorY = 2.5f;

    [Header("Configuración Orgánica")]
    public float variacionMaximaY = 1.5f;

    private float tiempoParaSiguiente = 0f;
    private float ultimaPosY = 0f;
    private bool esPrimerObstaculo = true;
    
    // ----------------------------------------------------------------------
    // MÉTODOS
    // ----------------------------------------------------------------------
    
    /// <summary>
    /// Restablece los temporizadores y la bandera del primer obstáculo,
    /// preparándose para un nuevo ciclo de ejecución en una partida reiniciada.
    /// </summary>
    public void Reiniciar()
    {
        tiempoParaSiguiente = 0f;
        esPrimerObstaculo = true;
    }

    /// <summary>
    /// Asegura el mapeo inicial del objeto generador a su componente específico en la jerarquía.
    /// </summary>
    void Awake()
    {
        if (genObstaculos == null)
        {
            genObstaculos = GetComponent<GeneradorObstaculo>();
        }
    }

    /// <summary>
    /// Calcula el temporizador descendente frame a frame. Determina las coordenadas, 
    /// restringiendo la variación del eje Y para crear un flujo orgánico y despacha la orden 
    /// de creación al componente abstracto correspondiente.
    /// </summary>
    void Update()
    {
        float frecuenciaBase = 3f; 
        if (LectorConfiguracion.Datos != null) frecuenciaBase = LectorConfiguracion.Datos.frecuenciaObstaculos;

        float frecuencia = frecuenciaBase;
        if (GameManager.Instancia != null)
        {
            float velInicial = GameManager.Instancia.velocidadInicial;
            float velActual = GameManager.Instancia.velocidadActual;
            
            if (velActual > 0) 
            {
                frecuencia = (velInicial * frecuenciaBase) / velActual;
            }
        }

        if (frecuencia < 0.5f) frecuencia = 0.5f;

        tiempoParaSiguiente -= Time.deltaTime;

        if (tiempoParaSiguiente <= 0)
        {
            float bordeDerecho = Camera.main.transform.position.x + (Camera.main.orthographicSize * Camera.main.aspect);
            float posX = bordeDerecho + 2f;
            
            float posY_Obstaculo;
            if (esPrimerObstaculo)
            {
                posY_Obstaculo = Random.Range(limiteInferiorY, limiteSuperiorY);
                esPrimerObstaculo = false;
            }
            else
            {
                float minY = Mathf.Max(limiteInferiorY, ultimaPosY - variacionMaximaY);
                float maxY = Mathf.Min(limiteSuperiorY, ultimaPosY + variacionMaximaY);
                posY_Obstaculo = Random.Range(minY, maxY);
            }
            ultimaPosY = posY_Obstaculo;

            if (genObstaculos != null) genObstaculos.Generar(posX, posY_Obstaculo);
            
            tiempoParaSiguiente = frecuencia;
        }
    }
}
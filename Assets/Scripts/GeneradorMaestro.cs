using UnityEngine;
/*
* CLASE GENERADOR MAESTRO:
* Se encarga de gestionar el ritmo y la posicion donde aparecen los obstáculos.
* Delega la instanciación a clases hijas de la clase abstracta Generador.
*/

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
    public void Reiniciar()
    {
        tiempoParaSiguiente = 0f;
        esPrimerObstaculo = true;
    }

    /*
    * Método Awake:
    * Busca el componente GeneradorObstaculo automáticamente para evitar problemas en el Inspector.
    */
    void Awake()
    {
        if (genObstaculos == null)
        {
            genObstaculos = GetComponent<GeneradorObstaculo>();
        }
    }

    /*
    * Método Update:
    * Se ejecuta 1 vez por frame.
    * Actualiza los temporizadores y, si debe, genera un nuevo obstáculo.
    */
    void Update()
    {
        // Frecuencia base desde tu JSON
        float frecuenciaBase = 3f; 
        if (LectorConfiguracion.Datos != null) frecuenciaBase = LectorConfiguracion.Datos.frecuenciaObstaculos;

        // Ajuste dinámico PROPORCIONAL para mantener SIEMPRE la misma distancia física entre obstáculos
        float frecuencia = frecuenciaBase;
        if (GameManager.Instancia != null)
        {
            float velInicial = GameManager.Instancia.velocidadInicial;
            float velActual = GameManager.Instancia.velocidadActual;
            
            // Fórmula: NuevoTiempo = (VelocidadVieja * TiempoViejo) / VelocidadNueva
            if (velActual > 0) 
            {
                frecuencia = (velInicial * frecuenciaBase) / velActual;
            }
        }

        // Asegurar que no bajen de medio segundo para que no sean humanamente imposibles de esquivar
        if (frecuencia < 0.5f) frecuencia = 0.5f;

        tiempoParaSiguiente -= Time.deltaTime;

        if (tiempoParaSiguiente <= 0)
        {
            // Calculamos la X fuera de la cámara
            float bordeDerecho = Camera.main.transform.position.x + (Camera.main.orthographicSize * Camera.main.aspect);
            float posX = bordeDerecho + 2f;
            
            // El Maestro decide la altura del obstáculo de forma "orgánica"
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

            // 1. Instanciamos el asteroide en la X y la Y
            if (genObstaculos != null) genObstaculos.Generar(posX, posY_Obstaculo);
            
            tiempoParaSiguiente = frecuencia;
        }
    }
}
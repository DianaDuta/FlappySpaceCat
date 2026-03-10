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

    private float tiempoParaSiguiente = 0f;
    // ----------------------------------------------------------------------
    // MÉTODOS
    // ----------------------------------------------------------------------
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
        //Frecuencia de actualización:
        float frecuencia = 3f; 
        if (LectorConfiguracion.Datos != null) frecuencia = LectorConfiguracion.Datos.frecuenciaObstaculos;
        if (frecuencia < 0.5f) frecuencia = 0.5f;

        tiempoParaSiguiente -= Time.deltaTime;

        if (tiempoParaSiguiente <= 0)
        {
            // Calculamos la X fuera de la cámara
            float bordeDerecho = Camera.main.transform.position.x + (Camera.main.orthographicSize * Camera.main.aspect);
            float posX = bordeDerecho + 2f;
            
            // El Maestro decide la altura del obstáculo
            float posY_Obstaculo = Random.Range(limiteInferiorY, limiteSuperiorY);

            // 1. Instanciamos el asteroide en la X y la Y
            if (genObstaculos != null) genObstaculos.Generar(posX, posY_Obstaculo);
            
            tiempoParaSiguiente = frecuencia;
        }
    }
}
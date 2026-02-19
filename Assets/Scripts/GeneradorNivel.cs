using UnityEngine;

/*
* Controla la generación de obstáculos en el juego.
* El script se adjunta a un GameObject vacío que actúa como "Generador".
* El prefab del obstáculo se arrastra en el inspector, y el script se encarga de instanciarlo a intervalos regulares con una altura aleatoria.
* La frecuencia de generación y la velocidad de los obstáculos se pueden configurar desde un archivo JSON, (ajustar la dificultad del juego sin modificar el código).
*/ 
public class GeneradorObstaculo : MonoBehaviour
{
    //--------------------------------
    // CAMPOS
    //--------------------------------
    public GameObject prefabObstaculo;
    private float tiempoParaSiguiente;
    public float limiteInferior = -2.5f;
    public float limiteSuperior = 2.5f;

    //--------------------------------
    // MÉTODOS
    //--------------------------------
    /*
    * Método Update, se obtiene la frecuencia de generación desde el JSON y se utiliza un temporizador para controlar cuándo crear un nuevo obstáculo.
    * Cuando el temporizador llega a cero, se llama al método CrearObstaculo y se reinicia el temporizador con la frecuencia obtenida del JSON.
    */
    void Update()
    {
        // Frecuencia de generación viene del JSON. Si no existe, usa 2.5 por defecto.
        float frecuencia = 2.5f; 
        if (LectorConfiguracion.Datos != null)
        {
            frecuencia = LectorConfiguracion.Datos.frecuenciaObstaculos;
        }

        // Temporizador para controlar la generación de obstáculos
        tiempoParaSiguiente -= Time.deltaTime; // Resta el tiempo
        if (tiempoParaSiguiente <= 0)
        {
            CrearObstaculo();
            tiempoParaSiguiente = frecuencia; // Reinicia el contador

        }
    }

    /*
    * Metodo CrearObstaculo:
    * Calcula el borde derecho de la camara para crear los obstaculos fuera de ella, establece la altura y lo instancia
    */
    void CrearObstaculo()
    {
        // POSICION DE NACIMIENTO
        // Calcula el borde derecho exacto de la camara
        float bordeDerechoCamara = Camera.main.transform.position.x + (Camera.main.orthographicSize * Camera.main.aspect);
        // Le suma 2 para asegura de que nace totalmente fuera de la pantalla
        float posicionX = bordeDerechoCamara + 10f;
        // Altura aleatoria para el hueco
        float alturaAleatoria = Random.Range(limiteInferior, limiteSuperior);

        Vector3 posicionNacimiento = new Vector3(posicionX, alturaAleatoria, 0);

        // Instancia el prefab del obstáculo en la posición de nacimiento sin rotación
        Instantiate(prefabObstaculo, posicionNacimiento, Quaternion.identity);
    }
}
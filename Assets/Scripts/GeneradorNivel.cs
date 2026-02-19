using UnityEngine;

/*
* Controla la generación de obstáculos en el juego.
* El script se adjunta a un GameObject vacío que actúa como "Generador".
* El prefab del obstáculo se arrastra en el inspector, y el script se encarga de instanciarlo a intervalos regulares con una altura aleatoria.
* La frecuencia de generación y la velocidad de los obstáculos se pueden configurar desde un archivo JSON, (ajustar la dificultad del juego sin modificar el código).
*/ 
public class GeneradorNivel : MonoBehaviour
{
    //--------------------------------
    // CAMPOS
    //--------------------------------
    public GameObject prefabObstaculo;
    private float tiempoParaSiguiente;

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
        if (tiempoParaSiguiente <= 0)
        {
            CrearObstaculo();
            tiempoParaSiguiente = frecuencia; // Reinicia el contador
        }
        else
        {
            tiempoParaSiguiente -= Time.deltaTime; // Resta el tiempo
        }
    }

    /*
    * Método CrearObstaculo, se calcula una altura aleatoria dentro de un rango seguro para que el obstáculo no aparezca demasiado alto o bajo.
    * Luego se instancia el prefab del obstáculo en la posición del generador con la altura aleatoria.
    */
    void CrearObstaculo()
    {
        // POSICIÓN DE NACIMIENTO
        // Usamos la posición del Generador (X) y calculamos una altura aleatoria (Y)
        // Rango seguro es entre -2 y 2
        float alturaAleatoria = Random.Range(-2f, 2f);
        Vector3 posicionNacimiento = new Vector3(transform.position.x, alturaAleatoria, 0);

        // Instancia el prefab del obstáculo en la posición de nacimiento sin rotación
        Instantiate(prefabObstaculo, posicionNacimiento, Quaternion.identity);
    }
}
using UnityEngine;

/// <summary>
/// Controla la translación espacial de los elementos generados en la escena, 
/// su autodestrucción por fuera de cámara y la lógica de puntuación pasiva.
/// </summary>
public class MovimientoObjetos : MonoBehaviour
{
    //--------------------------------
    // CAMPOS
    //--------------------------------
    
    [Tooltip("Coordenada X en la cual el objeto será destruido para liberar memoria.")]
    public float limiteIzquierda = -12f; 

    private bool haPuntuado = false;
    private bool esObstaculoPuntuable = false;
    private ControladorJugagor jugador;

    /// <summary>
    /// Captura la referencia del jugador y determina si este objeto cuenta como obstáculo puntuable.
    /// </summary>
    void Start()
    {
        jugador = FindAnyObjectByType<ControladorJugagor>();
        
        if (gameObject.name.Contains("Obstaculo"))
        {
            esObstaculoPuntuable = true;
        }
    }

    //--------------------------------
    // MÉTODOS
    //--------------------------------
    
    /// <summary>
    /// Traslada el objeto a la izquierda, basándose en la velocidad asíncrona del gestor principal.
    /// Detecta pasivamente cuando se ha sobrepasado al jugador para sumar puntos, 
    /// y destruye el componente al rebasar el límite izquierdo.
    /// </summary>
    void Update()
    {
        // Se evita el desplazamiento y la puntuación de obstáculos si se está esperando la reanudación del juego
        if (GameManager.Instancia != null && GameManager.Instancia.estaEnEsperaDeContinuacion)
        {
            return;
        }

        float velocidad = 3f;
        if (GameManager.Instancia != null)
        {
            velocidad = GameManager.Instancia.velocidadActual;
        }

        transform.Translate(Vector3.left * velocidad * Time.deltaTime, Space.World);

        if (esObstaculoPuntuable && !haPuntuado && jugador != null)
        {
            if (transform.position.x < jugador.transform.position.x)
            {
                haPuntuado = true;
                if (GameManager.Instancia != null) GameManager.Instancia.SumarPunto();
            }
        }

        if (transform.position.x < limiteIzquierda)
        {
            Destroy(gameObject);
        }
    }
}
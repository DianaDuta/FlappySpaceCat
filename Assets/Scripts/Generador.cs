using UnityEngine;

/// <summary>
/// Clase abstracta base que establece el contrato estandarizado para la creación 
/// e instanciación de cualquier tipo de elemento dinámico en la escena.
/// </summary>
public abstract class Generador : MonoBehaviour
{
    //------------------------------------------
    // CAMPOS
    //------------------------------------------
    public GameObject prefab;
    public float limiteInferiorY = -2.5f;
    public float limiteSuperiorY = 2.5f;

    //---------------------------------------------
    // METODOS
    //--------------------------------------------
    
    /// <summary>
    /// Método abstracto requerido en clases derivadas para definir la lógica 
    /// de inicialización del objeto en las coordenadas estipuladas.
    /// </summary>
    /// <param name="posicionX">Coordenada horizontal de instanciación.</param>
    /// <param name="posicionY">Coordenada vertical de instanciación.</param>
    public abstract void Generar(float posicionX, float posicionY);
}
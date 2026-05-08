using UnityEngine;

/// <summary>
/// Clase derivada de Generador que instancia físicamente los modelos 3D/2D 
/// de los elementos adversos en las coordenadas despachadas por la clase maestra.
/// </summary>
public class GeneradorObstaculo : Generador
{
    //--------------------------------
    // METODOS
    //--------------------------------
    
    /// <summary>
    /// Crea el componente físico en la memoria y lo sitúa en el mundo en la rotación por defecto.
    /// </summary>
    /// <param name="posicionX">Eje horizontal.</param>
    /// <param name="posicionY">Eje vertical.</param>
    public override void Generar(float posicionX, float posicionY)
    {
        Vector3 posicionNacimiento = new Vector3(posicionX, posicionY, 0);
        Instantiate(prefab, posicionNacimiento, Quaternion.identity);
    }
}
using UnityEngine;

/*
 * CLASE PADRE ABSTRACTA.
 * Define propiedades de cualquier generador
 * Obliga a las clases hijas a definir sus propias reglas de creacion.
 */
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
    /*
    * Metodo de creacion del objeto
    */
    public abstract void Generar(float posicionX, float posicionY);
}
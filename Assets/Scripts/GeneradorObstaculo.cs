using UnityEngine;

/*
* Hereda de GeneradorBase:
* Instancia directamente un obstaculo
*/ 
public class GeneradorObstaculo : Generador
{
    //--------------------------------
    // METODOS
    //--------------------------------
    /*
    * Metodo Generar:
    * Establece la posición y lo instancia
    */
    public override void Generar(float posicionX, float posicionY)
    {

        Vector3 posicionNacimiento = new Vector3(posicionX, posicionY, 0);
        // Instancia el prefab del obstáculo en la posición de nacimiento
        Instantiate(prefab, posicionNacimiento, Quaternion.identity);
    }
}
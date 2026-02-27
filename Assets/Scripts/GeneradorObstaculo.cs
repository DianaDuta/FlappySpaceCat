using UnityEngine;

/*
* Hereda de GeneradorBase:
* Instancia directamente un obstaculo
*/ 
public class GeneradorObstaculo : Generador
{
    //--------------------------------
    // CAMPOS
    //--------------------------------
    private float tiempoParaSiguiente = 0f;
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

    /*
    * Método Update:
    * Se ejecuta 1 vez por frame.
    * Actualiza los temporizadores y, si debe, genera un nuevo obstáculo.
    */
    void Update()
    {
        //Frecuencia del JSON:
        float frecuencia = 3f; 
        if (LectorConfiguracion.Datos != null) frecuencia = LectorConfiguracion.Datos.frecuenciaObstaculos;
        if (frecuencia < 0.5f) frecuencia = 0.5f;

        //Actualización del reloj:
        tiempoParaSiguiente -= Time.deltaTime;

        if (tiempoParaSiguiente <= 0)
        {
            // Calcula la X fuera de la cámara
            float bordeDerecho = Camera.main.transform.position.x + (Camera.main.orthographicSize * Camera.main.aspect);
            float posX = bordeDerecho + 2f;
            
            // Altura del obstáculo
            float posY = Random.Range(limiteInferiorY, limiteSuperiorY);

            // Instancia el asteroide
            Generar(posX, posY);
            
            tiempoParaSiguiente = frecuencia;
        }
    }
}
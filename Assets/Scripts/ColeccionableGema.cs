using UnityEngine;

/// <summary>
/// Gestiona la recolección y ciclo de vida del objeto gema en la escena, 
/// aplicando incrementos en el sistema de puntos y reproduciendo efectos sonoros al colisionar.
/// </summary>
public class ColeccionableGema : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    
    [Tooltip("Cantidad de puntos que se otorgan al recolectar este objeto.")]
    public int valorGema = 1;

    [Header("Efectos de Sonido")]
    public AudioClip sonidoRecoger; 
    
    [Range(0f, 1f)]
    public float volumenSonido = 1f; 

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------
    
    /// <summary>
    /// Detecta colisiones mediante físicas 2D (triggers). Si el objeto colisionador 
    /// posee la etiqueta de jugador, incrementa la moneda global, reproduce retroalimentación 
    /// auditiva y procede a la destrucción del componente.
    /// </summary>
    /// <param name="colision">Los datos del colisionador que entró en contacto.</param>
    private void OnTriggerEnter2D(Collider2D colision)
    {
        if (colision.CompareTag("Player"))
        {
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.SumarGema(valorGema);
            }

            if (sonidoRecoger != null)
            {
                Vector3 posicionCamara = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(sonidoRecoger, posicionCamara, volumenSonido);
            }

            Destroy(gameObject);
        }
    }
}
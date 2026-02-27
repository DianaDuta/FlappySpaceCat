using UnityEngine;

/*
 * SE DEBE ADJUNTAR AL PREFAB DEL OBSTÁCULO.
 * Al nacer el obstáculo, crea una gema que coloca en uno de los nodos.
 */
public class GeneradorGemas : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    [Header("Configuración")]
    public GameObject prefabGema;
    
    [Tooltip("Probabilidad de que este obstáculo traiga una gema (0-100)")]
    public float probabilidadAparicion = 100f;

    [Header("Nodos")]
    public Transform[] nodosPosibles;

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /*
    * Método Start: Genera una gema aleatoriamente en uno de los nodos del obstáculo
    */
    void Start()
    {
        // Elección si sale gema o no. 
        if (Random.Range(0f, 100f) > probabilidadAparicion) return;

        // Combrobación: gema no sea nulo y que la lista no esté vacía
        if (nodosPosibles.Length > 0 && prefabGema != null)
        {
            int indiceAleatorio = Random.Range(0, nodosPosibles.Length);
            Transform nodoElegido = nodosPosibles[indiceAleatorio];

            // 'nodoElegido' como 4º parámetro, para que Unity pegue la gema al obstáculo.
            Instantiate(prefabGema, nodoElegido.position, Quaternion.identity, nodoElegido);
            
            Debug.Log("Gema generada y pegada con éxito en el nodo: " + nodoElegido.name);
        }
    }
}
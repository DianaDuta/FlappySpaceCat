#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class AjustadorAnchors
{
    // Esto crea un botón nuevo en el menú superior de Unity
    [MenuItem("Herramientas UI/Ajustar Anchors al Objeto")]
    static void Ajustar()
    {
        // 1. Obtenemos el objeto UI que tengas seleccionado con el ratón
        GameObject objetoSeleccionado = Selection.activeGameObject;

        if (objetoSeleccionado != null && objetoSeleccionado.GetComponent<RectTransform>() != null)
        {
            RectTransform t = objetoSeleccionado.GetComponent<RectTransform>();
            RectTransform pt = objetoSeleccionado.transform.parent as RectTransform;

            if (t == null || pt == null) return;

            // 2. Registramos el cambio para que puedas hacer CTRL+Z si te equivocas
            Undo.RecordObject(t, "Ajustar Anchors");

            // 3. Calculamos la posición relativa respecto a su padre (el Canvas u otro panel)
            Vector2 nuevaAnclaMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                                                t.anchorMin.y + t.offsetMin.y / pt.rect.height);
            Vector2 nuevaAnclaMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                                                t.anchorMax.y + t.offsetMax.y / pt.rect.height);

            // 4. Aplicamos las nuevas anclas
            t.anchorMin = nuevaAnclaMin;
            t.anchorMax = nuevaAnclaMax;

            // 5. Ponemos los márgenes a cero para que el objeto se quede pegado a las anclas
            t.offsetMin = Vector2.zero;
            t.offsetMax = Vector2.zero;
        }
        else
        {
            Debug.LogWarning("¡Debes seleccionar un objeto de la UI (con RectTransform) primero!");
        }
    }
}
#endif
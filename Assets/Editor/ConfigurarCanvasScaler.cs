using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

/*
* Script de Editor para configurar el CanvasScaler de ambas escenas.
* Cambia el modo a "Scale With Screen Size" con resolución de referencia 800x480
* y matchWidthOrHeight = 0.5 para que la UI sea responsive en landscape y portrait.
*/
public class ConfigurarCanvasScaler
{
    public static void Execute()
    {
        // Buscar el Canvas en la escena activa
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas canvas in canvases)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // Cambiar a modo "Scale With Screen Size"
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

                // Resolución de referencia: 800x480 (la resolución de diseño actual)
                scaler.referenceResolution = new Vector2(800f, 480f);

                // Match 0.5 = equilibrio entre ancho y alto para adaptarse a landscape y portrait
                scaler.matchWidthOrHeight = 0.5f;

                // Marcar como modificado para que Unity guarde los cambios
                EditorUtility.SetDirty(scaler);

                Debug.Log("CanvasScaler configurado correctamente en: " + canvas.gameObject.name +
                          " | Modo: ScaleWithScreenSize | Ref: 800x480 | Match: 0.5");
            }
        }

        // Marcar la escena como modificada para que se pueda guardar
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Escena marcada como modificada. Recuerda guardar (Ctrl+S).");
    }
}

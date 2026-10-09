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

    [MenuItem("Herramientas UI/Configurar Player Settings (Landscape e Icono)")]
    public static void ConfigurarPlayerSettings()
    {
        // 1. Configurar Orientación a Landscape (Horizontal)
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        Debug.Log("✅ Orientación configurada exitosamente: Forzado modo Landscape (Horizontal).");

        // 2. Asignar Icono de la Aplicación
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/app_icon.png");
        if (icon == null)
        {
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/icono.png");
        }

        if (icon != null)
        {
            PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new Texture2D[] { icon }, IconKind.Application);
            PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Android, new Texture2D[] { icon }, IconKind.Application);
            Debug.Log("✅ Icono de aplicación asignado correctamente en Player Settings.");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró la textura del icono en 'Assets/app_icon.png' ni en 'Assets/Sprites/icono.png'.");
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
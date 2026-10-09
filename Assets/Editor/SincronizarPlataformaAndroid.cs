#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramienta de soporte para Unity 6:
/// Sincroniza la plataforma Android activa de EditorUserBuildSettings
/// con el nuevo sistema de Build Profiles de Unity 6 para que Google External Dependency Manager
/// (Android Resolver / EDM4U) la reconozca sin errores.
/// </summary>
public static class SincronizarPlataformaAndroid
{
    [MenuItem("Herramientas UI/Sincronizar Plataforma Android", false, 0)]
    public static void Sincronizar()
    {
        Debug.Log("[Sincronización Android] Forzando Target de compilación C# a Android...");
        
        bool resultado = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        
        if (resultado)
        {
            Debug.Log("[Sincronización Android] ¡Éxito! EditorUserBuildSettings.activeBuildTarget ahora es Android.");
            EditorUtility.DisplayDialog("Plataforma Sincronizada", 
                "Se ha sincronizado la plataforma Android con Google External Dependency Manager exitosamente.\n\nAhora puedes ejecutar Force Resolve sin errores.", 
                "Aceptar");
        }
        else
        {
            Debug.LogWarning("[Sincronización Android] La plataforma ya se encuentra en Android o requiere confirmación.");
        }
    }
}
#endif

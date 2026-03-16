using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

/*
* Script de Editor para guardar las escenas correctamente en su ubicación original
* y limpiar archivos duplicados que se hayan creado por error.
*/
public class GuardarEscenas
{
    public static void Execute()
    {
        // Guardar la escena activa
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Escenas abiertas guardadas correctamente.");

        // Limpiar archivos duplicados en la raíz de Assets si existen
        string[] archivosALimpiar = new string[]
        {
            "Assets/MenuPrincipal.unity",
            "Assets/Juego.unity"
        };

        foreach (string archivo in archivosALimpiar)
        {
            if (File.Exists(archivo))
            {
                AssetDatabase.DeleteAsset(archivo);
                Debug.Log("Archivo duplicado eliminado: " + archivo);
            }
        }

        AssetDatabase.Refresh();
    }
}

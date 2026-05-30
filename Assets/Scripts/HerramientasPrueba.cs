using UnityEngine;
using Firebase.Auth;

/// <summary>
/// Script temporal para pruebas y depuración (A BORRAR ANTES DE PUBLICAR).
/// Contiene funciones para facilitar el testeo de la interfaz, la tienda y la conexión.
/// </summary>
public class HerramientasPrueba : MonoBehaviour
{
    /// <summary>
    /// Otorga 100,000 gemas para poder probar la tienda libremente.
    /// </summary>
    public void DarCienMilGemas()
    {
        SecurePrefs.SetInt("GemasLocales", 100000);
        SecurePrefs.Save();
        Debug.Log("DEBUG: Se han añadido 100000 gemas.");
    }

    /// <summary>
    /// Elimina el desbloqueo de todas las skins, equipa la predeterminada y otorga 100,000 gemas.
    /// </summary>
    public void ResetearSkinsYDarGemas()
    {
        // Se borra el registro de las posibles skins (suponiendo un máximo de 100 por seguridad)
        for (int i = 1; i <= 100; i++)
        {
            SecurePrefs.DeleteKey("SkinDesbloqueada_" + i);
        }
        
        // Equipar la predeterminada (índice 0)
        SecurePrefs.SetInt("SkinEquipada", 0);
        
        // Dar gemas
        SecurePrefs.SetInt("GemasLocales", 100000);
        
        SecurePrefs.Save();
        Debug.Log("DEBUG: Skins reseteadas, equipada la predeterminada y se añadieron 100000 gemas.");
    }

    /// <summary>
    /// Realiza un borrado total: 0 gemas, 0 skins desbloqueadas, 0 de récord, cierra sesión
    /// de Firebase y vuelve a activar el panel de inicio de sesión para el próximo Game Over.
    /// </summary>
    public void ResetearProgresoCompleto()
    {
        // Borrar skins
        for (int i = 1; i <= 100; i++)
        {
            SecurePrefs.DeleteKey("SkinDesbloqueada_" + i);
        }
        SecurePrefs.SetInt("SkinEquipada", 0);
        
        // 0 gemas y borrar récord
        SecurePrefs.SetInt("GemasLocales", 0);
        SecurePrefs.SetInt("MejorPuntuacion", 0);

        // Reactivar el flag para que salga el panel de "Guardar Progreso" al morir
        SecurePrefs.SetInt("PrimeraVez", 1);
        
        SecurePrefs.Save();

        // Desloguear de Firebase
        if (FirebaseAuth.DefaultInstance != null && FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
            Debug.Log("DEBUG: Sesión de Firebase cerrada.");
        }

        Debug.Log("DEBUG: Progreso reseteado al 100%. Borradas las skins, gemas, récord, sesión cerrada y forzado el panel de login.");
    }
}

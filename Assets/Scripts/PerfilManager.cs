using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona la visualización de los datos del jugador en la interfaz gráfica, 
/// sincronizando estadísticas locales con la base de datos en la nube.
/// </summary>
public class PerfilManager : MonoBehaviour
{
    [Header("UI: Panel y Botón")]
    public GameObject panelPerfil;
    public Image imagenBotonSkin; 

    [Header("Textos del Perfil")]
    public TextMeshProUGUI txtGemasTotales;
    public TextMeshProUGUI txtMejorPuntuacion;

    [Header("Base de Datos de Skins")]
    public Sprite[] iconosSkinsDisponibles;

    /// <summary>
    /// Oculta el panel por defecto en el arranque y establece visualmente la skin 
    /// que el usuario tenga marcada como equipada.
    /// </summary>
    void Start()
    {
        panelPerfil.SetActive(false);

        int indiceSkinActiva = SecurePrefs.GetInt("SkinEquipada", 0);
        ActualizarIconoSkin(indiceSkinActiva);
    }

    /// <summary>
    /// Despliega el panel de perfil, carga las gemas del almacenamiento local
    /// y solicita de manera asíncrona la mejor puntuación a la base de datos.
    /// </summary>
    public void AbrirPerfil()
    {
        panelPerfil.SetActive(true);

        int gemas = SecurePrefs.GetInt("GemasLocales", 0);
        txtGemasTotales.text = " " + gemas.ToString();

        txtMejorPuntuacion.text = " ..."; 

        Firebase.Auth.FirebaseUser usuario = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (usuario != null && DatabaseManager.Instancia != null)
        {
            DatabaseManager.Instancia.ObtenerMejorPuntuacion(usuario.UserId, (recordNube) =>
            {
                if (txtMejorPuntuacion != null)
                {
                    txtMejorPuntuacion.text = " " + recordNube.ToString();
                }
                
                SecurePrefs.SetInt("MejorPuntuacion", recordNube);
                SecurePrefs.Save();
            });
        }
        else
        {
            int recordLocal = SecurePrefs.GetInt("MejorPuntuacion", 0);
            txtMejorPuntuacion.text = " " + recordLocal.ToString();
        }
    }

    /// <summary>
    /// Cierra el panel de perfil del jugador.
    /// </summary>
    public void CerrarPerfil()
    {
        panelPerfil.SetActive(false);
    }

    /// <summary>
    /// Modifica la representación gráfica del avatar en el botón principal.
    /// </summary>
    /// <param name="indice">Posición numérica de la imagen deseada dentro del catálogo de skins.</param>
    public void ActualizarIconoSkin(int indice)
    {
        if (iconosSkinsDisponibles.Length > 0 && indice < iconosSkinsDisponibles.Length)
        {
            imagenBotonSkin.sprite = iconosSkinsDisponibles[indice];
        }
    }
}

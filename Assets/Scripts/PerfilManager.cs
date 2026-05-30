using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona la visualización de los datos del jugador en la interfaz gráfica, 
/// sincronizando estadísticas locales con la base de datos en la nube.
/// </summary>
public class PerfilManager : MonoBehaviour
{
    public static PerfilManager Instancia;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }

        // Se inicializa la skin equipada al inicio para que ya esté cargada
        int indiceSkinActiva = SecurePrefs.GetInt("SkinEquipada", 0);
        ActualizarIconoSkin(indiceSkinActiva);

        // Se desactiva el panel de perfil de inmediato en el primer frame para que empiece oculto,
        // pero asegurando que este Awake() se haya ejecutado para registrar la Instancia.
        if (panelPerfil != null)
        {
            panelPerfil.SetActive(false);
        }
    }

    [Header("UI: Panel y Botón")]
    public GameObject panelPerfil;
    public RectTransform contenedorSkinPerfil;

    [Header("Textos del Perfil")]
    public TextMeshProUGUI txtGemasTotales;
    public TextMeshProUGUI txtMejorPuntuacion;

    [Header("Base de Datos de Skins")]
    public GameObject[] prefabsSkinsUI;

    [Header("Contenedores para Prefabs Animados")]
    [Tooltip("El contenedor (Cats_Animations_UI) del panel del menú principal.")]
    public RectTransform contenedorSkinMenu;

    private GameObject skinInstanciadaPerfil;
    private GameObject skinInstanciadaMenu;

    /// <summary>
    /// Despliega el panel de perfil, carga las gemas del almacenamiento local
    /// y solicita de manera asíncrona la mejor puntuación a la base de datos.
    /// </summary>
    public void AbrirPerfil()
    {
        panelPerfil.SetActive(true);

        int indiceSkinActiva = SecurePrefs.GetInt("SkinEquipada", 0);
        ActualizarIconoSkin(indiceSkinActiva);

        int gemas = SecurePrefs.GetInt("GemasLocales", 0);
        txtGemasTotales.text = " " + gemas.ToString();

        txtMejorPuntuacion.text = " ..."; 

        Firebase.Auth.FirebaseUser usuario = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (usuario != null && DatabaseManager.Instancia != null)
        {
            DatabaseManager.Instancia.ObtenerMejorPuntuacion(usuario.UserId, (recordNube) =>
            {
                int recordLocal = SecurePrefs.GetInt("MejorPuntuacion", 0);
                int mejorFinal = Mathf.Max(recordNube, recordLocal);
                
                if (txtMejorPuntuacion != null)
                {
                    txtMejorPuntuacion.text = " " + mejorFinal.ToString();
                }
                
                SecurePrefs.SetInt("MejorPuntuacion", mejorFinal);
                SecurePrefs.Save();
                
                if (recordLocal > recordNube && DatabaseManager.Instancia != null)
                {
                    DatabaseManager.Instancia.GuardarMejorPuntuacionEnNube(usuario.UserId, recordLocal);
                }
            });

            DatabaseManager.Instancia.ObtenerGemasTotales(usuario.UserId, (gemasNube) =>
            {
                int gemasLocales = SecurePrefs.GetInt("GemasLocales", 0);
                int gemasFinal = Mathf.Max(gemasNube, gemasLocales);

                if (txtGemasTotales != null)
                {
                    txtGemasTotales.text = " " + gemasFinal.ToString();
                }

                SecurePrefs.SetInt("GemasLocales", gemasFinal);
                SecurePrefs.Save();

                if (gemasLocales > gemasNube && DatabaseManager.Instancia != null)
                {
                    DatabaseManager.Instancia.GuardarGemasEnNube(usuario.UserId, gemasLocales);
                }
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
        // 1. Destruir las skins animadas anteriores si existen
        if (skinInstanciadaPerfil != null) Destroy(skinInstanciadaPerfil);
        if (skinInstanciadaMenu != null) Destroy(skinInstanciadaMenu);

        // 2. Instanciar los nuevos prefabs animados si están configurados
        if (prefabsSkinsUI != null && prefabsSkinsUI.Length > 0 && indice < prefabsSkinsUI.Length)
        {
            GameObject prefabACrear = prefabsSkinsUI[indice];
            if (prefabACrear != null)
            {
                // A. Instanciar en el contenedor del Perfil (Btn_Skin_Tienda o su hijo vacío)
                if (contenedorSkinPerfil != null)
                {
                    skinInstanciadaPerfil = Instantiate(prefabACrear, contenedorSkinPerfil);
                    AjustarRectTransform(skinInstanciadaPerfil.GetComponent<RectTransform>());
                }

                // B. Instanciar en el contenedor del Menú Principal
                if (contenedorSkinMenu != null)
                {
                    skinInstanciadaMenu = Instantiate(prefabACrear, contenedorSkinMenu);
                    AjustarRectTransform(skinInstanciadaMenu.GetComponent<RectTransform>());
                }
            }
        }
    }

    /// <summary>
    /// Utilidad para centrar y escalar de forma correcta los prefabs animados en sus contenedores de UI.
    /// </summary>
    private void AjustarRectTransform(RectTransform rt)
    {
        if (rt != null)
        {
            // Se fuerza al prefab a tener un tamaño fijo de 100x100 (el tamaño original de diseño).
            // Esto evita que al cambiar de padre, el prefab colapse a 0x0 o se estire de forma
            // desproporcionada, lo cual causaba que las partes del gato (cabeza, cola...) se separaran.
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(100f, 100f);

            // Se conserva la escala original del prefab animado (por ejemplo, el tamaño
            // y la escala negativa en X para mantener al gato volteado mirando a la derecha).
        }
    }

    /// <summary>
    /// Llama al GameManager para configurar y abrir el inicio de sesión.
    /// Cierra el panel de perfil para evitar solapamiento de interfaces.
    /// </summary>
    public void AbrirInicioSesion()
    {
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.AbrirInicioSesionDesdePerfil();
            CerrarPerfil();
        }
    }
}

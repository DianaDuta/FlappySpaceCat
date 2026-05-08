using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa los datos estructurales y visuales de una skin o personaje seleccionable.
/// </summary>
[System.Serializable]
public class DatosSkin
{
    public string nombrePersonaje;
    public int precioGemas;        
    public Sprite icono;           
}

/// <summary>
/// Gestiona la lógica de la tienda de skins, el desbloqueo de personajes
/// y la actualización visual de la interfaz correspondiente.
/// </summary>
public class TiendaSkinsManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelTienda;
    public GameObject panelPerfil;

    [Header("Botón del Perfil")]
    public Image imagenBotonPerfil; 
    public Image imagenBotonPerfilAdentro; 

    [Header("Base de Datos de Tienda")]
    public DatosSkin[] todasLasSkins; 

    [Header("Contenedor del Scroll")]
    public Transform contenidoScroll;
    public GameObject prefabTarjetaSkin; 

    /// <summary>
    /// Inicializa la interfaz de la tienda y actualiza el botón de perfil con la skin equipada actualmente.
    /// </summary>
    void Start()
    {
        panelTienda.SetActive(false);
        ActualizarBotonPerfil();
    }

    /// <summary>
    /// Despliega el panel de la tienda y genera dinámicamente las tarjetas de personajes disponibles.
    /// </summary>
    public void AbrirTienda()
    {
        panelPerfil.SetActive(false);
        panelTienda.SetActive(true);
        GenerarBotonesSkins();
    }

    /// <summary>
    /// Oculta el panel de la tienda, destruye los elementos generados en el scroll
    /// y retorna el estado de la interfaz al menú base.
    /// </summary>
    public void CerrarTienda()
    {
        panelTienda.SetActive(false);
        panelPerfil.SetActive(false); 
        ActualizarBotonPerfil();
        
        foreach (Transform hijo in contenidoScroll)
        {
            Destroy(hijo.gameObject);
        }
    }

    /// <summary>
    /// Actualiza las imágenes asociadas al jugador en el perfil principal
    /// tomando como referencia la skin actualmente guardada en la persistencia local.
    /// </summary>
    public void ActualizarBotonPerfil()
    {
        int skinActiva = SecurePrefs.GetInt("SkinEquipada", 0);
        if (todasLasSkins != null && todasLasSkins.Length > 0 && skinActiva < todasLasSkins.Length)
        {
            if (imagenBotonPerfil != null) imagenBotonPerfil.sprite = todasLasSkins[skinActiva].icono;
            if (imagenBotonPerfilAdentro != null) imagenBotonPerfilAdentro.sprite = todasLasSkins[skinActiva].icono;
        }
    }

    /// <summary>
    /// Elimina las tarjetas existentes en el contenedor UI y genera una nueva lista completa
    /// asignando eventos, datos visuales y verificando el estado de desbloqueo.
    /// </summary>
    private void GenerarBotonesSkins()
    {
        foreach (Transform hijo in contenidoScroll) Destroy(hijo.gameObject);

        int skinActiva = SecurePrefs.GetInt("SkinEquipada", 0);

        for (int i = 0; i < todasLasSkins.Length; i++)
        {
            GameObject nuevaTarjeta = Instantiate(prefabTarjetaSkin, contenidoScroll);
            TarjetaSkinUI scriptTarjeta = nuevaTarjeta.GetComponent<TarjetaSkinUI>();

            if (scriptTarjeta != null)
            {
                scriptTarjeta.iconoGato.sprite = todasLasSkins[i].icono;
                scriptTarjeta.txtNombre.text = todasLasSkins[i].nombrePersonaje;

                int indiceSkin = i;
                bool desbloqueada = (indiceSkin == 0) || (SecurePrefs.GetInt("SkinDesbloqueada_" + indiceSkin, 0) == 1);

                if (desbloqueada)
                {
                    if (skinActiva == indiceSkin)
                    {
                        scriptTarjeta.txtPrecio.text = "EQUIPADO";
                    }
                    else
                    {
                        scriptTarjeta.txtPrecio.text = "SELECCIONAR";
                    }
                }
                else
                {
                    scriptTarjeta.txtPrecio.text = todasLasSkins[i].precioGemas.ToString() + " G";
                }

                scriptTarjeta.botonAccion.onClick.RemoveAllListeners();
                scriptTarjeta.botonAccion.onClick.AddListener(() => 
                {
                    IntentarComprarOEquipar(indiceSkin);
                });
            }
        }
    }

    /// <summary>
    /// Procesa la selección del usuario sobre una skin específica. 
    /// Equipará la skin si está desbloqueada o deducirá el costo y la desbloqueará si los fondos son suficientes.
    /// </summary>
    /// <param name="indice">Índice numérico de la skin seleccionada en la base de datos.</param>
    public void IntentarComprarOEquipar(int indice)
    {
        bool desbloqueada = (indice == 0) || (SecurePrefs.GetInt("SkinDesbloqueada_" + indice, 0) == 1);

        if (desbloqueada)
        {
            SecurePrefs.SetInt("SkinEquipada", indice);
            ActualizarBotonPerfil();
            GenerarBotonesSkins(); 
        }
        else
        {
            int gemas = SecurePrefs.GetInt("GemasLocales", 0);
            int precio = todasLasSkins[indice].precioGemas;

            if (gemas >= precio)
            {
                int nuevasGemas = gemas - precio;
                SecurePrefs.SetInt("GemasLocales", nuevasGemas);
                
                SecurePrefs.SetInt("SkinDesbloqueada_" + indice, 1);
                
                SecurePrefs.SetInt("SkinEquipada", indice);
                SecurePrefs.Save();

                Firebase.Auth.FirebaseUser usuario = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
                if (usuario != null && DatabaseManager.Instancia != null)
                {
                    DatabaseManager.Instancia.GuardarGemasEnNube(usuario.UserId, nuevasGemas);
                }
                
                OpcionesManager.VibrarSiEstaActivado();

                ActualizarBotonPerfil();
                GenerarBotonesSkins(); 
            }
            else
            {
                Debug.LogWarning("Saldo insuficiente para adquirir: " + todasLasSkins[indice].nombrePersonaje);
            }
        }
    }
}

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

    [Header("Sprites de Botones")]
    public Sprite spriteBotonEquipado;
    public Sprite spriteBotonComprar;
    public Sprite spriteBotonDesbloqueado;

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

        // Notifica de inmediato al PerfilManager para actualizar los prefabs animados en el menú y perfil
        if (PerfilManager.Instancia != null)
        {
            PerfilManager.Instancia.ActualizarIconoSkin(skinActiva);
        }

        if (todasLasSkins != null && todasLasSkins.Length > 0 && skinActiva < todasLasSkins.Length)
        {
            if (imagenBotonPerfil != null) imagenBotonPerfil.sprite = todasLasSkins[skinActiva].icono;
            if (imagenBotonPerfilAdentro != null) imagenBotonPerfilAdentro.sprite = todasLasSkins[skinActiva].icono;
        }
    }

    /// <summary>
    /// Actualiza tanto el icono del perfil como la lista de tarjetas de la tienda en tiempo real.
    /// </summary>
    public void RefrescarTiendaCompleta()
    {
        ActualizarBotonPerfil();
        GenerarBotonesSkins();
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

                // Si el personaje es el oso (Bear), su sprite de origen está orientado en sentido opuesto
                // Se invierte la escala horizontal para homogeneizar la vista en las tarjetas de la tienda
                string nombreMin = todasLasSkins[i].nombrePersonaje.ToLower();
                if (nombreMin.Contains("bear") || nombreMin.Contains("oso"))
                {
                    scriptTarjeta.iconoGato.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else
                {
                    scriptTarjeta.iconoGato.rectTransform.localScale = new Vector3(1f, 1f, 1f);
                }

                int indiceSkin = i;
                bool desbloqueada = (indiceSkin == 0) || (SecurePrefs.GetInt("SkinDesbloqueada_" + indiceSkin, 0) == 1);

                if (desbloqueada)
                {
                    // Como no hay precio que mostrar, se desactiva el icono de la gema
                    if (scriptTarjeta.iconoGema != null) scriptTarjeta.iconoGema.gameObject.SetActive(false);

                    if (skinActiva == indiceSkin)
                    {
                        scriptTarjeta.txtPrecio.text = "EQUIPADO";
                        if (scriptTarjeta.botonAccion.image != null && spriteBotonEquipado != null) 
                        {
                            scriptTarjeta.botonAccion.image.sprite = spriteBotonEquipado;
                            scriptTarjeta.botonAccion.image.color = Color.white; // Evita que se tiña el sprite
                        }
                    }
                    else
                    {
                        scriptTarjeta.txtPrecio.text = "SELECCIONAR";
                        if (scriptTarjeta.botonAccion.image != null && spriteBotonDesbloqueado != null) 
                        {
                            scriptTarjeta.botonAccion.image.sprite = spriteBotonDesbloqueado;
                            scriptTarjeta.botonAccion.image.color = Color.white;
                        }
                    }
                }
                else
                {
                    // Como cuesta dinero, se muestra solo el número y se activa el icono de la gema
                    scriptTarjeta.txtPrecio.text = todasLasSkins[i].precioGemas.ToString();
                    if (scriptTarjeta.iconoGema != null) scriptTarjeta.iconoGema.gameObject.SetActive(true);
                    
                    if (scriptTarjeta.botonAccion.image != null && spriteBotonComprar != null) 
                    {
                        scriptTarjeta.botonAccion.image.sprite = spriteBotonComprar;
                        scriptTarjeta.botonAccion.image.color = Color.white;
                    }
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
            
            // Se actualiza el avatar 3D/2D del menú de fondo en tiempo real
            if (GameManager.Instancia != null) GameManager.Instancia.GenerarJugadorConSkin();
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

                // Se actualiza el avatar 3D/2D del menú de fondo en tiempo real
                if (GameManager.Instancia != null) GameManager.Instancia.GenerarJugadorConSkin();

                // Se registran los logros de la tienda
                if (LogrosManager.Instancia != null)
                {
                    LogrosManager.Instancia.DesbloquearLogro(TipoLogro.CambioLook);
                    ComprobarLogroColeccionista();
                }
            }
            else
            {
                Debug.LogWarning("Saldo insuficiente para adquirir: " + todasLasSkins[indice].nombrePersonaje);
            }
        }
    }

    /// <summary>
    /// Comprueba si el usuario ha desbloqueado todas las skins disponibles
    /// para otorgarle el logro correspondiente.
    /// </summary>
    private void ComprobarLogroColeccionista()
    {
        if (todasLasSkins == null || todasLasSkins.Length == 0) return;

        bool todasDesbloqueadas = true;
        // Se empieza desde 1 porque la skin 0 siempre es gratis/por defecto
        for (int i = 1; i < todasLasSkins.Length; i++)
        {
            if (SecurePrefs.GetInt("SkinDesbloqueada_" + i, 0) == 0)
            {
                todasDesbloqueadas = false;
                break;
            }
        }

        if (todasDesbloqueadas)
        {
            LogrosManager.Instancia.DesbloquearLogro(TipoLogro.Coleccionista);
        }
    }
}

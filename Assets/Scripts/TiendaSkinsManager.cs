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
/// Gestiona la lógica de la tienda de skins, el desbloqueo de personajes,
/// la reproducción del sonido propio de cada animal al seleccionarlo/comprarlo,
/// y la visualización de animaciones de UI perfectamente encuadradas dentro de las tarjetas.
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

    [Header("Prefabs Animados Opcionales (UI)")]
    [Tooltip("Prefabs con animación de los personajes para mostrar en las tarjetas de la tienda.")]
    public GameObject[] prefabsSkinsUI;

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
        if (panelTienda != null) panelTienda.SetActive(false);
        ActualizarBotonPerfil();
    }

    /// <summary>
    /// Despliega el panel de la tienda y genera dinámicamente las tarjetas de personajes disponibles.
    /// </summary>
    public void AbrirTienda()
    {
        if (panelPerfil != null) panelPerfil.SetActive(false);
        if (panelTienda != null) panelTienda.SetActive(true);
        GenerarBotonesSkins();
    }

    /// <summary>
    /// Oculta el panel de la tienda, destruye los elementos generados en el scroll
    /// y retorna el estado de la interfaz al menú base.
    /// </summary>
    public void CerrarTienda()
    {
        if (panelTienda != null) panelTienda.SetActive(false);
        if (panelPerfil != null) panelPerfil.SetActive(false); 
        ActualizarBotonPerfil();
        
        if (contenidoScroll != null)
        {
            foreach (Transform hijo in contenidoScroll)
            {
                Destroy(hijo.gameObject);
            }
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
    /// asignando eventos, animaciones perfectamente encuadradas, sonidos y verificando el estado de desbloqueo.
    /// </summary>
    private void GenerarBotonesSkins()
    {
        if (contenidoScroll == null || prefabTarjetaSkin == null) return;

        foreach (Transform hijo in contenidoScroll) Destroy(hijo.gameObject);

        int skinActiva = SecurePrefs.GetInt("SkinEquipada", 0);

        // Si no se asignaron los prefabs directamente en este componente, intentar obtenerlos de PerfilManager
        GameObject[] listaPrefabsAnimados = (prefabsSkinsUI != null && prefabsSkinsUI.Length > 0)
            ? prefabsSkinsUI
            : (PerfilManager.Instancia != null ? PerfilManager.Instancia.prefabsSkinsUI : null);

        for (int i = 0; i < todasLasSkins.Length; i++)
        {
            GameObject nuevaTarjeta = Instantiate(prefabTarjetaSkin, contenidoScroll);
            TarjetaSkinUI scriptTarjeta = nuevaTarjeta.GetComponent<TarjetaSkinUI>();

            if (scriptTarjeta != null)
            {
                int indiceSkin = i;

                // 1. Asignar nombre del personaje
                if (scriptTarjeta.txtNombre != null)
                {
                    scriptTarjeta.txtNombre.text = todasLasSkins[i].nombrePersonaje;
                }

                // 2. Configurar visualización: Prefab animado o Sprite estático
                bool animacionInstanciada = false;
                if (listaPrefabsAnimados != null && i < listaPrefabsAnimados.Length && listaPrefabsAnimados[i] != null)
                {
                    Transform contenedor = scriptTarjeta.contenedorAnimacion != null 
                        ? scriptTarjeta.contenedorAnimacion 
                        : (scriptTarjeta.iconoGato != null ? scriptTarjeta.iconoGato.transform : scriptTarjeta.transform);

                    if (contenedor != null)
                    {
                        GameObject animado = Instantiate(listaPrefabsAnimados[i], contenedor);
                        
                        // 1. Iniciar la animación en un punto aleatorio del ciclo para intercalarlas de forma orgánica
                        Animator anim = animado.GetComponent<Animator>();
                        if (anim == null) anim = animado.GetComponentInChildren<Animator>();
                        if (anim != null)
                        {
                            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                            float puntoAleatorio = Random.Range(0f, 1f);
                            anim.Play(0, -1, puntoAleatorio);
                        }

                        // 2. Configurar RectTransform con los valores exactos
                        RectTransform rt = animado.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            // Configurar anclajes en el centro
                            rt.anchorMin = new Vector2(0.5f, 0.5f);
                            rt.anchorMax = new Vector2(0.5f, 0.5f);
                            rt.pivot = new Vector2(0.5f, 0.5f);
                            
                            // Posición y dimensiones exactas del Inspector
                            rt.anchoredPosition = new Vector2(0f, 16f);
                            rt.sizeDelta = new Vector2(107.1415f, 107.1415f);

                            // Escala exacta (2.2x) respetando la orientación del prefab original
                            float escalaBase = 2.2f;
                            float escalaXOriginal = listaPrefabsAnimados[i].transform.localScale.x;
                            float direccionX = (escalaXOriginal < 0 ? -1f : 1f) * escalaBase;

                            rt.localScale = new Vector3(direccionX, escalaBase, escalaBase);
                        }

                        // Desactivar el componente Image de iconoGato para que no dibuje el sprite estático debajo
                        if (scriptTarjeta.iconoGato != null)
                        {
                            scriptTarjeta.iconoGato.enabled = false;
                        }
                        animacionInstanciada = true;
                    }
                }

                // Fallback: Si no hay prefab animado, usar el icono estático
                if (!animacionInstanciada && scriptTarjeta.iconoGato != null)
                {
                    scriptTarjeta.iconoGato.enabled = true;
                    scriptTarjeta.iconoGato.gameObject.SetActive(true);
                    scriptTarjeta.iconoGato.sprite = todasLasSkins[i].icono;

                    // Ajuste de orientación para el oso
                    string nombreMin = todasLasSkins[i].nombrePersonaje.ToLower();
                    if (nombreMin.Contains("bear") || nombreMin.Contains("oso"))
                    {
                        scriptTarjeta.iconoGato.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
                    }
                    else
                    {
                        scriptTarjeta.iconoGato.rectTransform.localScale = new Vector3(1f, 1f, 1f);
                    }
                }

                // 3. Estado de Desbloqueo y Botones
                bool desbloqueada = (indiceSkin == 0) || (SecurePrefs.GetInt("SkinDesbloqueada_" + indiceSkin, 0) == 1);

                if (desbloqueada)
                {
                    if (scriptTarjeta.iconoGema != null) scriptTarjeta.iconoGema.gameObject.SetActive(false);

                    if (skinActiva == indiceSkin)
                    {
                        string txtEquipado = LocalizationManager.Instancia != null 
                            ? LocalizationManager.Instancia.ObtenerTexto("tienda_equipado", "EQUIPADO") 
                            : "EQUIPADO";

                        if (scriptTarjeta.txtPrecio != null) scriptTarjeta.txtPrecio.text = txtEquipado;
                        if (scriptTarjeta.botonAccion != null && scriptTarjeta.botonAccion.image != null && spriteBotonEquipado != null) 
                        {
                            scriptTarjeta.botonAccion.image.sprite = spriteBotonEquipado;
                            scriptTarjeta.botonAccion.image.color = Color.white;
                        }
                    }
                    else
                    {
                        string txtSeleccionar = LocalizationManager.Instancia != null 
                            ? LocalizationManager.Instancia.ObtenerTexto("tienda_seleccionar", "SELECCIONAR") 
                            : "SELECCIONAR";

                        if (scriptTarjeta.txtPrecio != null) scriptTarjeta.txtPrecio.text = txtSeleccionar;
                        if (scriptTarjeta.botonAccion != null && scriptTarjeta.botonAccion.image != null && spriteBotonDesbloqueado != null) 
                        {
                            scriptTarjeta.botonAccion.image.sprite = spriteBotonDesbloqueado;
                            scriptTarjeta.botonAccion.image.color = Color.white;
                        }
                    }
                }
                else
                {
                    if (scriptTarjeta.txtPrecio != null) scriptTarjeta.txtPrecio.text = todasLasSkins[i].precioGemas.ToString();
                    if (scriptTarjeta.iconoGema != null) scriptTarjeta.iconoGema.gameObject.SetActive(true);
                    
                    if (scriptTarjeta.botonAccion != null && scriptTarjeta.botonAccion.image != null && spriteBotonComprar != null) 
                    {
                        scriptTarjeta.botonAccion.image.sprite = spriteBotonComprar;
                        scriptTarjeta.botonAccion.image.color = Color.white;
                    }
                }

                // 4. Asignar eventos de clic con reproducción del sonido característico de cada animal
                if (scriptTarjeta.botonAccion != null)
                {
                    scriptTarjeta.botonAccion.onClick.RemoveAllListeners();
                    scriptTarjeta.botonAccion.onClick.AddListener(() => 
                    {
                        IntentarComprarOEquipar(indiceSkin);
                    });
                }

                if (scriptTarjeta.botonTarjeta != null)
                {
                    scriptTarjeta.botonTarjeta.onClick.RemoveAllListeners();
                    scriptTarjeta.botonTarjeta.onClick.AddListener(() =>
                    {
                        if (SonidosUIManager.Instancia != null)
                        {
                            SonidosUIManager.Instancia.ReproducirSonidoSkin(indiceSkin);
                        }
                    });
                }
            }
        }
    }

    /// <summary>
    /// Procesa la selección del usuario sobre una skin específica. 
    /// Equipará la skin si está desbloqueada o deducirá el costo y la desbloqueará si los fondos son suficientes.
    /// Reproduce automáticamente el sonido característico del animal seleccionado.
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
            
            // Reproducir sonido del animal al equiparlo
            if (SonidosUIManager.Instancia != null)
            {
                SonidosUIManager.Instancia.ReproducirSonidoSkin(indice);
            }

            // Actualizar avatar 3D/2D del menú de fondo en tiempo real
            if (GameManager.Instancia != null) GameManager.Instancia.GenerarJugadorConSkin();

            if (RetosDiariosManager.Instancia != null)
            {
                RetosDiariosManager.Instancia.RegistrarCambioSkin();
            }

            if (LogrosManager.Instancia != null)
            {
                if (indice == 5) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.MuuuyAlto);
                if (indice == 4) LogrosManager.Instancia.DesbloquearLogro(TipoLogro.OsoOrbital);
            }
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

                // Reproducir sonido del animal al comprarlo
                if (SonidosUIManager.Instancia != null)
                {
                    SonidosUIManager.Instancia.ReproducirSonidoSkin(indice);
                }

                ActualizarBotonPerfil();
                GenerarBotonesSkins();

                if (GameManager.Instancia != null) GameManager.Instancia.GenerarJugadorConSkin();

                if (RetosDiariosManager.Instancia != null)
                {
                    RetosDiariosManager.Instancia.RegistrarCambioSkin();
                }

                // Desbloquear logros de la tienda
                if (LogrosManager.Instancia != null)
                {
                    // Logro 12: Comprar tu primera skin
                    LogrosManager.Instancia.DesbloquearLogro(TipoLogro.CambioLook);

                    // Logro 13: Desbloquear todas las skins de la tienda
                    bool todasDesbloqueadas = true;
                    if (todasLasSkins != null)
                    {
                        for (int k = 0; k < todasLasSkins.Length; k++)
                        {
                            if (k > 0 && SecurePrefs.GetInt("SkinDesbloqueada_" + k, 0) != 1)
                            {
                                todasDesbloqueadas = false;
                                break;
                            }
                        }
                    }
                    if (todasDesbloqueadas)
                    {
                        LogrosManager.Instancia.DesbloquearLogro(TipoLogro.Coleccionista);
                    }
                }

                // Guardar en la nube si hay sesión activa
                Firebase.Auth.FirebaseUser usuario = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
                if (usuario != null && DatabaseManager.Instancia != null)
                {
                    DatabaseManager.Instancia.GuardarGemasEnNube(usuario.UserId, nuevasGemas);
                    DatabaseManager.Instancia.GuardarSkinsDesbloqueadas(usuario.UserId, indice);
                }
            }
            else
            {
                // Sonido de botón normal/fallo si no alcanza el dinero
                if (SonidosUIManager.Instancia != null)
                {
                    SonidosUIManager.Instancia.ReproducirSonidoBoton();
                }
            }
        }
    }
}

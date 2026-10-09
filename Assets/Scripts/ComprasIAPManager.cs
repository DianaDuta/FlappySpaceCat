using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using TMPro;

/// <summary>
/// Gestor central de compras integradas (In-App Purchases) con dinero real para Google Play.
/// Controla el panel emergente de compra de gemas, los paquetes disponibles (1.000 y 3.000 gemas),
/// la adaptación automática a las divisas y monedas de cada país, el almacenamiento seguro
/// en SecurePrefs y la sincronización con Firebase en la nube.
/// </summary>
public class ComprasIAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static ComprasIAPManager Instancia;

    [Header("Panel Emergente de Compras (UI)")]
    [Tooltip("Panel emergente que se abre al pulsar el botón de comprar gemas.")]
    public GameObject panelComprasGemas;

    [Header("Configuración de Productos (Google Play)")]
    [Tooltip("ID único del pack de 1.000 gemas en Google Play Console.")]
    public string idPack1000Gemas = "pack_gemas_1000";
    public int cantidadGemasPack1000 = 1000;

    [Tooltip("ID único del pack de 3.000 gemas en Google Play Console.")]
    public string idPack3000Gemas = "pack_gemas_3000";
    public int cantidadGemasPack3000 = 3000;

    [Header("Textos de Precio de la Interfaz (Adaptación Multi-Divisa)")]
    [Tooltip("Texto para el precio del pack de 1.000 gemas (ej: 1,99 €, $1.99, etc.).")]
    public TextMeshProUGUI txtPrecioPack1000;

    [Tooltip("Texto para el precio del pack de 3.000 gemas (ej: 4,99 €, $4.99, etc.).")]
    public TextMeshProUGUI txtPrecioPack3000;

    [Header("Sonidos (Opcional)")]
    [Tooltip("Sonido que se reproduce al completar una compra con éxito.")]
    public AudioClip sonidoCompraExitosa;

    // Controlador interno de la tienda de Unity IAP
    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    // ---------------------------------------------------------------------------------
    // CICLO DE VIDA
    // ---------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (panelComprasGemas != null)
        {
            panelComprasGemas.SetActive(false);
        }

        InicializarComprasIAP();
    }

    // ---------------------------------------------------------------------------------
    // CONTROL DEL PANEL EMERGENTE (UI)
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// Abre el panel emergente con los paquetes de gemas y actualiza los precios locales.
    /// Vincular al botón de 'Comprar Gemas' / '+' de la tienda o menú.
    /// </summary>
    public void AbrirPanelCompras()
    {
        if (panelComprasGemas != null)
        {
            panelComprasGemas.SetActive(true);
            panelComprasGemas.transform.SetAsLastSibling(); // Traer al frente
        }

        if (SonidosUIManager.Instancia != null)
        {
            SonidosUIManager.Instancia.ReproducirSonidoBoton();
        }

        ActualizarPreciosLocalizadosUI();
    }

    /// <summary>
    /// Cierra el panel emergente de compra de gemas.
    /// Vincular al botón 'X' o 'Cerrar' del panel emergente.
    /// </summary>
    public void CerrarPanelCompras()
    {
        if (panelComprasGemas != null)
        {
            panelComprasGemas.SetActive(false);
        }

        if (SonidosUIManager.Instancia != null)
        {
            SonidosUIManager.Instancia.ReproducirSonidoBoton();
        }
    }

    // ---------------------------------------------------------------------------------
    // INICIALIZACIÓN DE GOOGLE PLAY BILLING
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// Registra el catálogo de productos y conecta con los servidores de Google Play.
    /// </summary>
    public void InicializarComprasIAP()
    {
        if (EstaInicializado()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Ambos productos son CONSUMIBLES (el jugador puede comprarlos tantas veces como desee)
        builder.AddProduct(idPack1000Gemas, ProductType.Consumable);
        builder.AddProduct(idPack3000Gemas, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// Comprueba si la pasarela de pagos está conectada y lista.
    /// </summary>
    public bool EstaInicializado()
    {
        return storeController != null && extensionProvider != null;
    }

    // ---------------------------------------------------------------------------------
    // CALLBACKS DE UNITY IAP (IStoreListener)
    // ---------------------------------------------------------------------------------

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        Debug.Log("✅ [IAP] Pasarela de compras inicializada correctamente.");

        ActualizarPreciosLocalizadosUI();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"❌ [IAP] Error al inicializar compras: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"❌ [IAP] Error al inicializar compras: {error} - Detalle: {message}");
    }

    /// <summary>
    /// Se ejecuta automáticamente cuando el pago con dinero real se autoriza en Google Play.
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productoId = args.purchasedProduct.definition.id;

        if (string.Equals(productoId, idPack1000Gemas, StringComparison.Ordinal))
        {
            OtorgarGemas(cantidadGemasPack1000, "Pack 1.000 Gemas");
        }
        else if (string.Equals(productoId, idPack3000Gemas, StringComparison.Ordinal))
        {
            OtorgarGemas(cantidadGemasPack3000, "Pack 3.000 Gemas");
        }
        else
        {
            Debug.LogWarning($"⚠️ [IAP] Producto desconocido recibido: {productoId}");
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"⚠️ [IAP] Compra cancelada o fallida: {product.definition.id} - Razón: {failureReason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogWarning($"⚠️ [IAP] Compra fallida: {product.definition.id} - Detalle: {failureDescription.message}");
    }

    // ---------------------------------------------------------------------------------
    // FUNCIONES DE COMPRA (BOTONES DE LA UI)
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// Inicia la compra del Pack de 1.000 Gemas (1,99 €).
    /// Asignar al botón 'Comprar 1000 Gemas' en el Inspector OnClick().
    /// </summary>
    public void ComprarPack1000Gemas()
    {
        IniciarCompra(idPack1000Gemas);
    }

    /// <summary>
    /// Inicia la compra del Pack de 3.000 Gemas (4,99 €).
    /// Asignar al botón 'Comprar 3000 Gemas' en el Inspector OnClick().
    /// </summary>
    public void ComprarPack3000Gemas()
    {
        IniciarCompra(idPack3000Gemas);
    }

    private void IniciarCompra(string idProducto)
    {
        if (EstaInicializado())
        {
            Product producto = storeController.products.WithID(idProducto);

            if (producto != null && producto.availableToPurchase)
            {
                Debug.Log($"💳 [IAP] Iniciando pasarela de pago para: {producto.definition.id}");
                storeController.InitiatePurchase(producto);
            }
            else
            {
                Debug.LogWarning($"⚠️ [IAP] El producto '{idProducto}' no está disponible para compra en este momento.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ [IAP] El sistema de compras aún no se ha inicializado. Reintentando...");
            InicializarComprasIAP();
        }
    }

    /// <summary>
    /// Suma las gemas compradas al almacenamiento seguro y las guarda en la nube.
    /// </summary>
    private void OtorgarGemas(int cantidad, string nombrePack)
    {
        int gemasActuales = SecurePrefs.GetInt("GemasLocales", 0);
        int nuevoTotal = gemasActuales + cantidad;

        SecurePrefs.SetInt("GemasLocales", nuevoTotal);
        SecurePrefs.Save();

        Debug.Log($"💎 [IAP] ¡Compra de '{nombrePack}' exitosa! +{cantidad} gemas otorgadas. Total: {nuevoTotal}");

        // 1. Sincronizar con Firebase Database si hay sesión activa
        if (DatabaseManager.Instancia != null && Firebase.Auth.FirebaseAuth.DefaultInstance != null)
        {
            Firebase.Auth.FirebaseUser usuario = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
            if (usuario != null)
            {
                DatabaseManager.Instancia.GuardarGemasEnNube(usuario.UserId, nuevoTotal);
            }
        }

        // 2. Actualizar el texto de gemas del perfil si está presente
        if (PerfilManager.Instancia != null && PerfilManager.Instancia.txtGemasTotales != null)
        {
            PerfilManager.Instancia.txtGemasTotales.text = " " + nuevoTotal.ToString();
        }

        // 3. Reproducir sonido de compra exitosa
        if (SonidosUIManager.Instancia != null)
        {
            if (sonidoCompraExitosa != null)
            {
                SonidosUIManager.Instancia.ReproducirSonidoBoton(sonidoCompraExitosa);
            }
            else
            {
                SonidosUIManager.Instancia.ReproducirSonidoBoton();
            }
        }
    }

    // ---------------------------------------------------------------------------------
    // ADAPTACIÓN AUTOMÁTICA A MONEDAS DE OTROS PAÍSES
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// Consulta los metadatos de Google Play y actualiza automáticamente los textos
    /// con el precio oficial formateado en la moneda local del país del jugador
    /// (ej: '1,99 €' en España, '$1.99' en USA, 'MX$39.00' en México, '£1.79' en UK, etc.).
    /// Si no hay conexión o se prueba en el editor, muestra los precios base predeterminados.
    /// </summary>
    public void ActualizarPreciosLocalizadosUI()
    {
        if (EstaInicializado())
        {
            // Pack 1: 1.000 Gemas
            if (txtPrecioPack1000 != null)
            {
                Product p1 = storeController.products.WithID(idPack1000Gemas);
                if (p1 != null && p1.metadata != null && !string.IsNullOrEmpty(p1.metadata.localizedPriceString))
                {
                    txtPrecioPack1000.text = p1.metadata.localizedPriceString;
                }
                else
                {
                    txtPrecioPack1000.text = "1,99 €";
                }
            }

            // Pack 2: 3.000 Gemas
            if (txtPrecioPack3000 != null)
            {
                Product p2 = storeController.products.WithID(idPack3000Gemas);
                if (p2 != null && p2.metadata != null && !string.IsNullOrEmpty(p2.metadata.localizedPriceString))
                {
                    txtPrecioPack3000.text = p2.metadata.localizedPriceString;
                }
                else
                {
                    txtPrecioPack3000.text = "4,99 €";
                }
            }
        }
        else
        {
            // Valores por defecto si la tienda no está inicializada todavía
            if (txtPrecioPack1000 != null) txtPrecioPack1000.text = "1,99 €";
            if (txtPrecioPack3000 != null) txtPrecioPack3000.text = "4,99 €";
        }
    }
}

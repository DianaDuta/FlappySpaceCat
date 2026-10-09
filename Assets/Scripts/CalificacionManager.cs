using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Analytics;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

/// <summary>
/// Gestiona la lógica de los paneles de calificación y agradecimiento de la interfaz de usuario,
/// procesando la selección de valoraciones y su envío a servicios externos como Google Play o Firebase.
/// </summary>
public class CalificacionManager : MonoBehaviour
{
    /// <summary>
    /// Instancia estática de la clase para permitir acceso global (patrón Singleton).
    /// </summary>
    public static CalificacionManager Instancia { get; private set; }


    [Header("Paneles de la Interfaz")]
    [Tooltip("Panel principal de calificación.")]
    public GameObject panelCalificacion;

    [Tooltip("Panel de agradecimiento.")]
    public GameObject panelAgradecimiento;

    [Tooltip("Panel de opciones del juego, utilizado para retornar en el flujo de navegación.")]
    public GameObject panelOpciones;

    [Header("Componentes de Estrellas")]
    [Tooltip("Arreglo ordenado de 5 botones de estrella (de 1 a 5).")]
    public Button[] botonesEstrellas;

    [Tooltip("Color por defecto del botón de estrella.")]
    public Color colorNormal = Color.white;

    [Tooltip("Color aplicado al botón de la estrella seleccionada para denotar su selección.")]
    public Color colorSeleccionado = new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("UI Elementos y Configuración")]
    [Tooltip("Texto utilizado para mostrar advertencias de validación al usuario.")]
    public TextMeshProUGUI textoAviso;

    [Tooltip("URL correspondiente a la ficha de la tienda de aplicaciones de Google Play.")]
    public string urlGooglePlay = "market://details?id=com.Dianarcado.FlappySpaceCat";

    // Almacena el valor de la valoración seleccionada por el usuario (0 indica que no hay selección).
    private int estrellaSeleccionada = 0;

    /// <summary>
    /// Configura la instancia Singleton durante la inicialización del objeto.
    /// </summary>
    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inicializa el estado de los componentes y asigna de manera programática los listeners a los botones de estrellas.
    /// </summary>
    private void Start()
    {
        // Ocultar paneles por defecto al inicio de la escena
        if (panelCalificacion != null) panelCalificacion.SetActive(false);
        if (panelAgradecimiento != null) panelAgradecimiento.SetActive(false);

        // Asignación de listeners para los botones de estrellas
        if (botonesEstrellas != null)
        {
            for (int i = 0; i < botonesEstrellas.Length; i++)
            {
                int valorEstrella = i + 1;
                botonesEstrellas[i].onClick.AddListener(() => SeleccionarEstrella(valorEstrella));
            }
        }
    }

    /// <summary>
    /// Despliega el panel de calificación, ocultando el panel de opciones y restableciendo la selección previa.
    /// </summary>
    public void AbrirCalificacion()
    {
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (panelCalificacion != null)
        {
            panelCalificacion.SetActive(true);
        }

        if (panelAgradecimiento != null)
        {
            panelAgradecimiento.SetActive(false);
        }

        // Restablecer valores de inicialización
        estrellaSeleccionada = 0;
        if (textoAviso != null)
        {
            textoAviso.text = string.Empty;
        }

        RestablecerColoresEstrellas();
    }

    /// <summary>
    /// Cierra el panel de calificación y reactiva el panel de opciones del juego.
    /// </summary>
    public void CerrarCalificacion()
    {
        if (panelCalificacion != null)
        {
            panelCalificacion.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }
    }

    /// <summary>
    /// Cierra el panel de agradecimiento, el panel de calificación y reactiva el panel de opciones.
    /// </summary>
    public void CerrarAgradecimiento()
    {
        if (panelAgradecimiento != null)
        {
            panelAgradecimiento.SetActive(false);
        }

        if (panelCalificacion != null)
        {
            panelCalificacion.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }

        // Notificar a OpcionesManager para que oculte el botón de calificar si ya se envió valoración
        OpcionesManager opciones = FindAnyObjectByType<OpcionesManager>();
        if (opciones != null)
        {
            opciones.ActualizarBotonCalificar();
        }
    }

    /// <summary>
    /// Registra la selección del botón de estrella correspondiente y actualiza la apariencia visual.
    /// </summary>
    /// <param name="valorEstrella">El valor numérico asignado a la estrella pulsada (1 a 5).</param>
    public void SeleccionarEstrella(int valorEstrella)
    {
        estrellaSeleccionada = valorEstrella;

        if (textoAviso != null)
        {
            textoAviso.text = string.Empty;
        }

        // Actualizar la apariencia visual de los botones de estrella
        if (botonesEstrellas != null)
        {
            for (int i = 0; i < botonesEstrellas.Length; i++)
            {
                Image imagenBoton = botonesEstrellas[i].GetComponent<Image>();
                if (imagenBoton != null)
                {
                    // Iluminar la estrella seleccionada
                    imagenBoton.color = (i == valorEstrella - 1) ? colorSeleccionado : colorNormal;
                }
            }
        }
    }

    /// <summary>
    /// Procesa el envío de la calificación:
    /// - Si es 5 estrellas: inicia el flujo oficial de Google Play In-App Review (sin salir del juego).
    /// - Si es 1, 2, 3 o 4 estrellas: guarda la valoración en Firebase Firestore y Analytics.
    /// - Muestra el panel de agradecimiento.
    /// </summary>
    public void EnviarCalificacion()
    {
        if (estrellaSeleccionada == 0)
        {
            if (textoAviso != null)
            {
                string aviso = LocalizationManager.Instancia != null 
                    ? LocalizationManager.Instancia.ObtenerTexto("calificacion_aviso_estrella", "Por favor, seleccione alguna estrella.") 
                    : "Por favor, seleccione alguna estrella.";
                textoAviso.text = aviso;
            }
            return;
        }

        // Almacenar localmente el valor seleccionado
        SecurePrefs.SetInt("CalificacionEstrellas", estrellaSeleccionada);
        SecurePrefs.Save();

        // Notificar al gestor de opciones
        OpcionesManager opciones = FindAnyObjectByType<OpcionesManager>();
        if (opciones != null)
        {
            opciones.ActualizarBotonCalificar();
        }

        // 1. Si es 5 estrellas -> In-App Review de Google Play (dentro del juego)
        if (estrellaSeleccionada == 5)
        {
            StartCoroutine(IniciarFlujoInAppReview());
        }
        // 2. Si son 1, 2, 3 o 4 estrellas -> Guardar en Firebase
        else
        {
            EnviarDatosAFirebase(estrellaSeleccionada);

            // Cerrar calificación y abrir agradecimiento inmediatamente
            if (panelCalificacion != null) panelCalificacion.SetActive(false);
            if (panelAgradecimiento != null) panelAgradecimiento.SetActive(true);
        }
    }

    /// <summary>
    /// Ejecuta el flujo oficial de Google Play In-App Review.
    /// Despliega una tarjeta flotante de Google sobre el juego sin que el usuario tenga que salir a la Play Store.
    /// Al finalizar, cierra el panel de calificación y despliega el de agradecimiento.
    /// </summary>
    private System.Collections.IEnumerator IniciarFlujoInAppReview()
    {
        // Registro telemétrico previo
        if (AnalyticsManager.Instancia != null)
        {
            AnalyticsManager.Instancia.RegistrarEventoSimple("valoracion_5_estrellas_inappreview");
        }

        // Guardar también registro en Firebase Firestore
        EnviarDatosAFirebase(5);

#if UNITY_ANDROID && !UNITY_EDITOR
        bool flujoIniciado = false;
        AndroidJavaObject manager = null;
        AndroidJavaObject requestTask = null;
        AndroidJavaObject currentActivity = null;

        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
            using (AndroidJavaClass reviewFactory = new AndroidJavaClass("com.google.android.play.core.review.ReviewManagerFactory"))
            {
                if (reviewFactory != null && currentActivity != null)
                {
                    manager = reviewFactory.CallStatic<AndroidJavaObject>("create", currentActivity);
                    if (manager != null)
                    {
                        requestTask = manager.Call<AndroidJavaObject>("requestReviewFlow");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("In-App Review no disponible al inicializar: " + ex.Message);
        }

        if (requestTask != null)
        {
            float tiempoEspera = 0f;
            while (!EsTareaCompletada(requestTask) && tiempoEspera < 3f)
            {
                tiempoEspera += Time.unscaledDeltaTime;
                yield return null;
            }

            AndroidJavaObject launchTask = null;
            try
            {
                if (EsTareaExitosa(requestTask))
                {
                    AndroidJavaObject reviewInfo = requestTask.Call<AndroidJavaObject>("getResult");
                    if (manager != null && currentActivity != null && reviewInfo != null)
                    {
                        launchTask = manager.Call<AndroidJavaObject>("launchReviewFlow", currentActivity, reviewInfo);
                        flujoIniciado = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("In-App Review error al lanzar flujo: " + ex.Message);
            }

            if (launchTask != null)
            {
                while (!EsTareaCompletada(launchTask))
                {
                    yield return null;
                }
                Debug.Log("✅ Flujo In-App Review de Google Play completado exitosamente dentro del juego.");
            }
        }

        if (!flujoIniciado)
        {
            Debug.Log("Continuando flujo estándar de agradecimiento dentro del juego.");
        }
#else
        // Simulación en Editor / PC
        Debug.Log("⭐ [Editor] Simulando Google Play In-App Review (Tarjeta flotante de 5 estrellas en Android real).");
        yield return new WaitForSecondsRealtime(0.5f);
#endif

        // Ocultar panel de calificación y mostrar panel de agradecimiento
        if (panelCalificacion != null)
        {
            panelCalificacion.SetActive(false);
        }

        if (panelAgradecimiento != null)
        {
            panelAgradecimiento.SetActive(true);
        }
    }

    /// <summary>
    /// Envía el valor de la calificación a los servicios de Firebase Analytics y Firebase Firestore.
    /// </summary>
    /// <param name="estrellas">Número de estrellas otorgadas por el usuario.</param>
    private void EnviarDatosAFirebase(int estrellas)
    {
        // 1. Registro telemétrico a través de Firebase Analytics
        try
        {
            if (AnalyticsManager.Instancia != null)
            {
                AnalyticsManager.Instancia.RegistrarEventoSimple("valoracion_" + estrellas + "_estrellas");
            }
            else
            {
                FirebaseAnalytics.LogEvent("valoracion_usuario", "estrellas", estrellas);
            }
            Debug.Log($"📊 Analítica de valoración ({estrellas} estrellas) registrada.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error al registrar evento en Firebase Analytics: " + ex.Message);
        }

        // 2. Registro de datos estructurados en Firestore
        try
        {
            string idUsuario = null;
            if (Firebase.Auth.FirebaseAuth.DefaultInstance != null && Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                idUsuario = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            }

            if (!string.IsNullOrEmpty(idUsuario))
            {
                if (DatabaseManager.Instancia != null)
                {
                    DatabaseManager.Instancia.GuardarCalificacionEnNube(idUsuario, estrellas);
                }
                else
                {
                    FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
                    if (db != null)
                    {
                        DocumentReference docRef = db.Collection("Jugadores").Document(idUsuario);
                        Dictionary<string, object> datosValoracion = new Dictionary<string, object>
                        {
                            { "calificacionEstrellas", estrellas },
                            { "fechaCalificacion", FieldValue.ServerTimestamp }
                        };
                        docRef.SetAsync(datosValoracion, SetOptions.MergeAll);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Calificación registrada localmente y en Analytics (usuario no autenticado en Firestore).");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error durante el proceso de persistencia en Firestore: " + ex.Message);
        }
    }



    /// <summary>
    /// Restablece el color de todos los botones de estrellas al estado por defecto.
    /// </summary>
    private void RestablecerColoresEstrellas()
    {
        if (botonesEstrellas != null)
        {
            for (int i = 0; i < botonesEstrellas.Length; i++)
            {
                Image imagenBoton = botonesEstrellas[i].GetComponent<Image>();
                if (imagenBoton != null)
                {
                    imagenBoton.color = colorNormal;
                }
            }
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private bool EsTareaCompletada(AndroidJavaObject task)
    {
        try
        {
            return task != null && task.Call<bool>("isComplete");
        }
        catch
        {
            return true;
        }
    }

    private bool EsTareaExitosa(AndroidJavaObject task)
    {
        try
        {
            return task != null && task.Call<bool>("isSuccessful");
        }
        catch
        {
            return false;
        }
    }
#endif
}

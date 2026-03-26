using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;
using Google;

/**
* CLASE AUTH MANAGER:
* Se encarga exclusivamente de la comunicación con Firebase Authentication y Google.
* Lee los datos de la interfaz y crea los usuarios.
*/
public class AuthManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    [Header("Conexión Firebase")]
    private FirebaseAuth auth;

    [Header("Interfaz Gráfica (UI)")]
    [Tooltip("Arrastra aquí el Input_Email")]
    public TMP_InputField inputEmail;
    
    [Tooltip("Arrastra aquí el Input_Password")]
    public TMP_InputField inputPassword;

    [Tooltip("Arrastra aquí el Txt_Subtitulo (para dar avisos)")]
    public TextMeshProUGUI textoAvisos;

    [Header("Google Sign-In")]
    [Tooltip("Pega aquí el ID de cliente web larguísimo que copiaste de Firebase")]
    public string webClientId = "";

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    /*
    * Método RegistrarUsuarioConEmail():
    * Se ejecuta al pulsar el botón de "Guardar con Email".
    */
    public void RegistrarUsuarioConEmail()
    {
        string idUsuario = inputEmail.text.Trim(); // Limpia los espacios
        string password = inputPassword.text;

        if (string.IsNullOrEmpty(idUsuario) || string.IsNullOrEmpty(password))
        {
            if (textoAvisos != null) textoAvisos.text = "Por favor, rellena tu ID y contraseña.";
            return;
        }

        // Firebase exige que la contraseña tenga mínimo 6 carácteres
        if (password.Length < 6)
        {
            if (textoAvisos != null) textoAvisos.text = "Error: La contraseña debe tener 6 o más números/letras.";
            return;
        }

        // TRUCO: Si el usuario escribe solo un nombre ("dianarcado"), Firebase dirá que el formato está mal.
        // Simulamos un correo invisible añadiendo un dominio para que Firebase lo acepte súper feliz.
        string emailParaFirebase = idUsuario;
        if (!emailParaFirebase.Contains("@"))
        {
            emailParaFirebase = idUsuario + "@flappyspacecat.com";
        }

        if (textoAvisos != null) textoAvisos.text = "Conectando con la base estelar...";

        auth.CreateUserWithEmailAndPasswordAsync(emailParaFirebase, password).ContinueWithOnMainThread(tarea =>
        {
            if (tarea.IsCanceled)
            {
                Debug.LogError("El registro fue cancelado.");
                if (textoAvisos != null) textoAvisos.text = "Registro cancelado.";
                return;
            }
            if (tarea.IsFaulted)
            {
                Debug.LogError("Error en el registro: " + tarea.Exception);
                
                // Intentamos dar un mensaje más claro dependiendo del error de Firebase
                string mensajeError = "Error al crear la cuenta. Revisa los datos.";
                if (tarea.Exception.ToString().Contains("EmailAlreadyInUse"))
                {
                    mensajeError = "Ese Nombre (ID) ya está cogido. ¡Elige otro!";
                }

                if (textoAvisos != null) textoAvisos.text = mensajeError;
                return;
            }

            // Firebase devuelve un AuthResult para el Email
            AuthResult resultado = tarea.Result;
            Debug.Log("¡Piloto registrado con éxito! ID: " + resultado.User.UserId);
            if (textoAvisos != null) textoAvisos.text = "¡Cuenta creada con éxito!";

            // FUNNEL REGISTRO
            AnalyticsManager.Instancia.RegistrarEventoSimple("registro_email_exito");

            // Guarda las gemas en Firebase
            int gemasLocales = PlayerPrefs.GetInt("GemasLocales", 0);
            
            // Saca el ID del User que está dentro del resultado
            string idUnico = resultado.User.UserId; 
            
            if (DatabaseManager.Instancia != null)
            {
                DatabaseManager.Instancia.GuardarGemasEnNube(idUnico, gemasLocales);
                Debug.Log("Gemas guardadas en la nube.");
            }
            else
            {
                Debug.LogError("No se encontró el DatabaseManager en la escena.");
            } 

            // Cerrar el panel de inicio de sesión y mostrar el Game Over para que decidan si ver anuncio
            StartCoroutine(TransicionAGameOver());
        });
    }

    /*
    * Método TransicionAGameOver():
    * Espera un segundo para que el usuario pueda leer el mensaje de éxito antes de cambiar de pantalla.
    */
    private System.Collections.IEnumerator TransicionAGameOver()
    {
        yield return new WaitForSecondsRealtime(1.5f); // Usamos Realtime porque Time.timeScale suele ser 0 en menús

        if (GameManager.Instancia != null)
        {
            if (GameManager.Instancia.panelInicioSesion != null)
                GameManager.Instancia.panelInicioSesion.SetActive(false);

            if (GameManager.Instancia.panelGameOver != null)
                GameManager.Instancia.panelGameOver.SetActive(true);
        }
    }

    /*
    * Método RegistrarUsuarioConGoogle():
    * Se ejecuta al pulsar el botón de "Guardar con Google".
    */
    public void RegistrarUsuarioConGoogle()
    {
        if (textoAvisos != null) textoAvisos.text = "Abriendo conexión con Google...";

        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = webClientId
        };

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(tareaGoogle =>
        {
            if (tareaGoogle.IsCanceled)
            {
                Debug.LogError("Inicio de sesión de Google cancelado.");
                if (textoAvisos != null) textoAvisos.text = "Conexión cancelada.";
                return;
            }
            if (tareaGoogle.IsFaulted)
            {
                Debug.LogError("Error en Google Sign-In: " + tareaGoogle.Exception);
                if (textoAvisos != null) textoAvisos.text = "Error al conectar con Google.";
                return;
            }

            string idToken = tareaGoogle.Result.IdToken;

            if (textoAvisos != null) textoAvisos.text = "Autenticando en la base estelar...";

            Credential credencial = GoogleAuthProvider.GetCredential(idToken, null);

            auth.SignInWithCredentialAsync(credencial).ContinueWithOnMainThread(tareaFirebase =>
            {
                if (tareaFirebase.IsCanceled || tareaFirebase.IsFaulted)
                {
                    Debug.LogError("Error al entrar en Firebase con Google.");
                    if (textoAvisos != null) textoAvisos.text = "Error de conexión con la base.";
                    return;
                }

                // Firebase devuelve directamente el FirebaseUser para Google
                FirebaseUser usuarioGoogle = tareaFirebase.Result;
                Debug.Log("¡Piloto de Google registrado! ID: " + usuarioGoogle.UserId);

                // FUNNEL REGISTRO
                AnalyticsManager.Instancia.RegistrarEventoSimple("registro_google_exito");
                
                int gemasLocales = PlayerPrefs.GetInt("GemasLocales", 0);
                
                // Saca el ID directo del usuario
                string idUnico = usuarioGoogle.UserId; 
                
                if (DatabaseManager.Instancia != null)
                {
                    DatabaseManager.Instancia.GuardarGemasEnNube(idUnico, gemasLocales);
                }

                if (textoAvisos != null) textoAvisos.text = "¡Cuenta de Google conectada con éxito!";

                // Cerrar el panel de inicio de sesión y mostrar el Game Over
                StartCoroutine(TransicionAGameOver());
            });
        });
    }
}
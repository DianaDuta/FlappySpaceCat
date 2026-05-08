using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Extensions;
using Google;

/// <summary>
/// Gestiona la autenticación de usuarios mediante Firebase Authentication,
/// incluyendo el registro y acceso a través de correo electrónico y Google Sign-In.
/// </summary>
public class AuthManager : MonoBehaviour
{
    // -----------------------------------------------------------------------------
    // CAMPOS
    // -----------------------------------------------------------------------------
    [Header("Conexión Firebase")]
    private FirebaseAuth auth;

    [Header("Interfaz Gráfica (UI)")]
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;
    public TextMeshProUGUI textoAvisos;

    [Header("Google Sign-In")]
    public string webClientId = "";

    // -----------------------------------------------------------------------------
    // MÉTODOS
    // -----------------------------------------------------------------------------

    /// <summary>
    /// Inicializa la instancia predeterminada del servicio de autenticación de Firebase.
    /// </summary>
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    /// <summary>
    /// Intenta registrar o autenticar al usuario utilizando las credenciales de correo electrónico
    /// ingresadas en la interfaz de usuario, validando previamente los requisitos de formato y seguridad.
    /// </summary>
    public void RegistrarUsuarioConEmail()
    {
        string idUsuario = inputEmail.text.Trim(); 
        string password = inputPassword.text;

        if (string.IsNullOrEmpty(idUsuario) || string.IsNullOrEmpty(password))
        {
            if (textoAvisos != null) textoAvisos.text = "Por favor, introduzca un identificador y una contraseña.";
            return;
        }

        if (password.Length < 8)
        {
            if (textoAvisos != null) textoAvisos.text = "Error: La contraseña debe poseer un mínimo de 8 caracteres.";
            return;
        }

        bool tieneLetra = false;
        bool tieneNumero = false;
        foreach (char c in password)
        {
            if (char.IsLetter(c)) tieneLetra = true;
            if (char.IsDigit(c)) tieneNumero = true;
        }

        if (!tieneLetra || !tieneNumero)
        {
            if (textoAvisos != null) textoAvisos.text = "Error: La contraseña requiere al menos una letra y un dígito.";
            return;
        }

        string emailParaFirebase = idUsuario;
        if (!emailParaFirebase.Contains("@"))
        {
            emailParaFirebase = idUsuario + "@flappyspacecat.com";
        }

        if (textoAvisos != null) textoAvisos.text = "Estableciendo conexión...";

        auth.CreateUserWithEmailAndPasswordAsync(emailParaFirebase, password).ContinueWithOnMainThread(tarea =>
        {
            if (tarea.IsCanceled)
            {
                Debug.LogError("El proceso de registro fue cancelado.");
                if (textoAvisos != null) textoAvisos.text = "Registro cancelado.";
                return;
            }
            if (tarea.IsFaulted)
            {
                Debug.LogError("Excepción en el registro: " + tarea.Exception);
                
                string mensajeError = "Error al crear la cuenta. Verifique los datos.";
                if (tarea.Exception.ToString().Contains("EmailAlreadyInUse"))
                {
                    mensajeError = "El identificador proporcionado ya se encuentra registrado.";
                }

                if (textoAvisos != null) textoAvisos.text = mensajeError;
                return;
            }

            AuthResult resultado = tarea.Result;
            Debug.Log("Usuario registrado exitosamente. ID: " + resultado.User.UserId);
            if (textoAvisos != null) textoAvisos.text = "Cuenta creada exitosamente.";

            AnalyticsManager.Instancia.RegistrarEventoSimple("registro_email_exito");

            int gemasLocales = SecurePrefs.GetInt("GemasLocales", 0);
            string idUnico = resultado.User.UserId; 
            
            if (DatabaseManager.Instancia != null)
            {
                DatabaseManager.Instancia.GuardarGemasEnNube(idUnico, gemasLocales);
            }
            else
            {
                Debug.LogError("Instancia de DatabaseManager no encontrada en la jerarquía.");
            } 

            StartCoroutine(TransicionAGameOver());
        });
    }

    /// <summary>
    /// Retrasa la transición de la interfaz para asegurar que el mensaje informativo sea legible 
    /// antes de ocultar el panel de inicio de sesión.
    /// </summary>
    private System.Collections.IEnumerator TransicionAGameOver()
    {
        yield return new WaitForSecondsRealtime(1.5f); 

        if (GameManager.Instancia != null)
        {
            if (GameManager.Instancia.panelInicioSesion != null)
                GameManager.Instancia.panelInicioSesion.SetActive(false);

            if (GameManager.Instancia.panelGameOver != null)
                GameManager.Instancia.panelGameOver.SetActive(true);
        }
    }

    /// <summary>
    /// Inicia el proceso de autenticación federada utilizando los servicios de Google Sign-In,
    /// obteniendo un token de acceso y validándolo con Firebase Auth.
    /// </summary>
    public void RegistrarUsuarioConGoogle()
    {
        if (textoAvisos != null) textoAvisos.text = "Iniciando servicio de Google...";

        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = webClientId
        };

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(tareaGoogle =>
        {
            if (tareaGoogle.IsCanceled)
            {
                Debug.LogError("Operación de Google Sign-In cancelada.");
                if (textoAvisos != null) textoAvisos.text = "Conexión cancelada.";
                return;
            }
            if (tareaGoogle.IsFaulted)
            {
                Debug.LogError("Fallo en Google Sign-In: " + tareaGoogle.Exception);
                if (textoAvisos != null) textoAvisos.text = "Error al conectar con los servicios de Google.";
                return;
            }

            string idToken = tareaGoogle.Result.IdToken;

            if (textoAvisos != null) textoAvisos.text = "Verificando credenciales...";

            Credential credencial = GoogleAuthProvider.GetCredential(idToken, null);

            auth.SignInWithCredentialAsync(credencial).ContinueWithOnMainThread(tareaFirebase =>
            {
                if (tareaFirebase.IsCanceled || tareaFirebase.IsFaulted)
                {
                    Debug.LogError("Error en la validación de credenciales con Firebase.");
                    if (textoAvisos != null) textoAvisos.text = "Error de conexión con la base de datos.";
                    return;
                }

                FirebaseUser usuarioGoogle = tareaFirebase.Result;
                Debug.Log("Usuario validado mediante Google. ID: " + usuarioGoogle.UserId);

                AnalyticsManager.Instancia.RegistrarEventoSimple("registro_google_exito");
                
                int gemasLocales = SecurePrefs.GetInt("GemasLocales", 0);
                string idUnico = usuarioGoogle.UserId; 
                
                if (DatabaseManager.Instancia != null)
                {
                    DatabaseManager.Instancia.GuardarGemasEnNube(idUnico, gemasLocales);
                }

                if (textoAvisos != null) textoAvisos.text = "Sincronización con Google completada.";

                StartCoroutine(TransicionAGameOver());
            });
        });
    }
}
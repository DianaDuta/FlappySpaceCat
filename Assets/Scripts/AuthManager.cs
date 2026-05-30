using UnityEngine;
using TMPro;
using Firebase;
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

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;

        // Se asegura de que la caja de la contraseña empiece enmascarada (con asteriscos) por defecto
        if (inputPassword != null)
        {
            inputPassword.contentType = TMP_InputField.ContentType.Password;
            inputPassword.ForceLabelUpdate();
        }
    }

    /// <summary>
    /// Alterna la visualización de la contraseña entre asteriscos (*) y texto legible.
    /// Puede enlazarse directamente al OnClick() de un botón de visibilidad de contraseña (icono de ojo).
    /// </summary>
    public void AlternarVisibilidadPassword()
    {
        if (inputPassword == null) return;

        if (inputPassword.contentType == TMP_InputField.ContentType.Password)
        {
            inputPassword.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            inputPassword.contentType = TMP_InputField.ContentType.Password;
        }

        // Fuerza a TextMeshPro a actualizar y redibujar el texto visible inmediatamente
        inputPassword.ForceLabelUpdate();
    }

    /// <summary>
    /// Intenta registrar o autenticar al usuario utilizando las credenciales de correo electrónico
    /// ingresadas en la interfaz de usuario, validando previamente los requisitos de formato y seguridad.
    /// </summary>
    public void RegistrarUsuarioConEmail()
    {
        string idUsuario = inputEmail.text.Trim(); 
        string password = inputPassword.text;

        // 1. Validaciones Locales - Identificador (Username)
        if (string.IsNullOrEmpty(idUsuario))
        {
            if (textoAvisos != null) textoAvisos.text = "Por favor, introduce un identificador.";
            return;
        }

        if (idUsuario.Contains(" "))
        {
            if (textoAvisos != null) textoAvisos.text = "El ID no puede contener espacios en blanco.";
            return;
        }

        if (idUsuario.Length < 3)
        {
            if (textoAvisos != null) textoAvisos.text = "El ID debe tener al menos 3 caracteres.";
            return;
        }

        // 2. Validaciones Locales - Contraseña (Password)
        if (string.IsNullOrEmpty(password))
        {
            if (textoAvisos != null) textoAvisos.text = "Por favor, introduce una contraseña.";
            return;
        }

        if (password.Length < 8)
        {
            if (textoAvisos != null) textoAvisos.text = "La contraseña debe tener al menos 8 caracteres.";
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
            if (textoAvisos != null) textoAvisos.text = "La contraseña requiere al menos una letra y un número.";
            return;
        }

        // Formatear identificador a correo si no contiene @
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
                if (textoAvisos != null) textoAvisos.text = "Conexión cancelada.";
                return;
            }
            if (tarea.IsFaulted)
            {
                Debug.LogError("Excepción en el registro: " + tarea.Exception);
                
                string mensajeError = "Error al crear la cuenta. Verifique los datos.";
                
                // Extraer la excepción interna de Firebase
                FirebaseException firebaseEx = null;
                if (tarea.Exception != null)
                {
                    foreach (var inner in tarea.Exception.InnerExceptions)
                    {
                        if (inner is FirebaseException)
                        {
                            firebaseEx = inner as FirebaseException;
                            break;
                        }
                    }
                }

                if (firebaseEx != null)
                {
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    switch (errorCode)
                    {
                        case AuthError.EmailAlreadyInUse:
                            mensajeError = "Este ID ya existe. Por favor, elige otro.";
                            break;
                        case AuthError.InvalidEmail:
                            mensajeError = "El ID no tiene un formato válido.";
                            break;
                        case AuthError.WeakPassword:
                            mensajeError = "La contraseña es demasiado débil.";
                            break;
                        case AuthError.NetworkRequestFailed:
                            mensajeError = "Error de red. Verifica tu conexión.";
                            break;
                        default:
                            mensajeError = "Error de registro: " + firebaseEx.Message;
                            break;
                    }
                }
                else
                {
                    // Fallback de contingencia mediante búsqueda por texto
                    if (tarea.Exception.ToString().Contains("EmailAlreadyInUse") || tarea.Exception.ToString().Contains("already in use"))
                    {
                        mensajeError = "Este ID ya existe. Por favor, elige otro.";
                    }
                }

                if (textoAvisos != null) textoAvisos.text = mensajeError;
                return;
            }

            AuthResult resultado = tarea.Result;
            Debug.Log("Usuario registrado exitosamente. ID: " + resultado.User.UserId);
            if (textoAvisos != null) textoAvisos.text = "Cuenta creada exitosamente.";

            AnalyticsManager.Instancia.RegistrarEventoSimple("registro_email_exito");

            int gemasLocales = SecurePrefs.GetInt("GemasLocales", 0);
            int mejorPuntuacion = SecurePrefs.GetInt("MejorPuntuacion", 0);
            string idUnico = resultado.User.UserId; 
            
            if (DatabaseManager.Instancia != null)
            {
                DatabaseManager.Instancia.GuardarGemasEnNube(idUnico, gemasLocales);
                DatabaseManager.Instancia.GuardarMejorPuntuacionEnNube(idUnico, mejorPuntuacion);
            }
            else
            {
                Debug.LogError("Instancia de DatabaseManager no encontrada en la jerarquía.");
            } 

            ComprobarRecompensaRegistro();
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
                int mejorPuntuacion = SecurePrefs.GetInt("MejorPuntuacion", 0);
                string idUnico = usuarioGoogle.UserId; 
                
                if (DatabaseManager.Instancia != null)
                {
                    DatabaseManager.Instancia.GuardarGemasEnNube(idUnico, gemasLocales);
                    DatabaseManager.Instancia.GuardarMejorPuntuacionEnNube(idUnico, mejorPuntuacion);
                }

                if (textoAvisos != null) textoAvisos.text = "Sincronización con Google completada.";

                ComprobarRecompensaRegistro();
                StartCoroutine(TransicionAGameOver());
            });
        });
    }

    /// <summary>
    /// Comprueba si el usuario se ha registrado dentro de las primeras 24 horas 
    /// desde que abrió el juego por primera vez para regalarle la skin de Cowsmo.
    /// </summary>
    private void ComprobarRecompensaRegistro()
    {
        // Se comprueba si ya se le ha entregado la recompensa antes para no dársela dos veces
        if (SecurePrefs.GetInt("RecompensaCowEntregada", 0) == 1) return;

        string fechaString = SecurePrefs.GetString("FechaPrimeraApertura", "");
        if (!string.IsNullOrEmpty(fechaString))
        {
            System.DateTime fechaPrimeraApertura;
            if (System.DateTime.TryParse(fechaString, null, System.Globalization.DateTimeStyles.RoundtripKind, out fechaPrimeraApertura))
            {
                System.TimeSpan tiempoTranscurrido = System.DateTime.Now - fechaPrimeraApertura;
                
                // Si han pasado 24 horas o menos
                if (tiempoTranscurrido.TotalHours <= 24)
                {
                    // Skin de Cow en la lista de skins = 4
                    int indiceSkinCow = 4; 
                    
                    SecurePrefs.SetInt("SkinDesbloqueada_" + indiceSkinCow, 1);
                    SecurePrefs.SetInt("RecompensaCowEntregada", 1);
                    SecurePrefs.Save();
                    
                    Debug.Log("¡Recompensa de registro entregada! Skin de Cow desbloqueada.");
                    if (textoAvisos != null) textoAvisos.text = "¡Felicidades! Has recibido la skin Cow por registrarte hoy.";
                }
            }
        }
    }
}
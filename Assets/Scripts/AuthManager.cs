using UnityEngine;
using UnityEngine.UI;
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
        AsegurarReferenciasYConfiguracion(true);
    }

    void OnEnable()
    {
        AsegurarReferenciasYConfiguracion(true);
    }

    /// <summary>
    /// Realiza de forma robusta la búsqueda y configuración de las referencias a los campos de entrada,
    /// asegurando que la contraseña comience enmascarada y que el botón de visibilidad esté enlazado.
    /// </summary>
    public void AsegurarReferenciasYConfiguracion(bool forzarEnmascarado = false)
    {
        // Si las referencias del Inspector están vacías, se realiza una búsqueda profunda (incluyendo objetos inactivos)
        // a partir del contenedor del panel de inicio de sesión registrado en el GameManager.
        if ((inputEmail == null || inputPassword == null) && GameManager.Instancia != null && GameManager.Instancia.panelInicioSesion != null)
        {
            TMP_InputField[] inputFields = GameManager.Instancia.panelInicioSesion.GetComponentsInChildren<TMP_InputField>(true);
            foreach (TMP_InputField field in inputFields)
            {
                if (field.gameObject.name == "Username_Input")
                {
                    inputEmail = field;
                }
                else if (field.gameObject.name == "Password_Input")
                {
                    inputPassword = field;
                }
            }
        }

        // Se asegura de que la caja de la contraseña empiece enmascarada (con asteriscos) por defecto.
        if (inputPassword != null)
        {
            // Se fuerza el enmascarado inicial como contraseña si se solicita de forma explícita.
            if (forzarEnmascarado)
            {
                inputPassword.contentType = TMP_InputField.ContentType.Password;
                inputPassword.inputType = TMP_InputField.InputType.Password;
                inputPassword.ForceLabelUpdate();
            }

            // Vinculación programática del evento Click del botón de visibilidad mediante una búsqueda recursiva.
            Button btnVisibilidad = null;
            foreach (Button btn in inputPassword.GetComponentsInChildren<Button>(true))
            {
                if (btn.gameObject.name == "Btn_Visibilidad")
                {
                    btnVisibilidad = btn;
                    break;
                }
            }

            if (btnVisibilidad != null)
            {
                btnVisibilidad.onClick.RemoveListener(AlternarVisibilidadPassword);
                btnVisibilidad.onClick.AddListener(AlternarVisibilidadPassword);
            }
        }
    }

    /// <summary>
    /// Alterna la visualización de la contraseña entre asteriscos (*) y texto legible.
    /// Reproduce el sonido de clic de la UI cada vez que se pulsa.
    /// </summary>
    public void AlternarVisibilidadPassword()
    {
        // Reproducir sonido de clic de UI en cada pulsación
        if (SonidosUIManager.Instancia != null)
        {
            SonidosUIManager.Instancia.ReproducirSonidoBoton();
        }

        AsegurarReferenciasYConfiguracion(false);
        if (inputPassword == null) return;

        if (inputPassword.contentType == TMP_InputField.ContentType.Password)
        {
            inputPassword.contentType = TMP_InputField.ContentType.Standard;
            inputPassword.inputType = TMP_InputField.InputType.Standard;
        }
        else
        {
            inputPassword.contentType = TMP_InputField.ContentType.Password;
            inputPassword.inputType = TMP_InputField.InputType.Password;
        }

        // Fuerza a TextMeshPro a actualizar y redibujar el texto visible inmediatamente.
        inputPassword.ForceLabelUpdate();
    }

    /// <summary>
    /// Intenta registrar o autenticar al usuario utilizando las credenciales de correo electrónico
    /// <summary>
    /// Muestra un mensaje en textoAvisos traducido al idioma activo.
    /// </summary>
    private void MostrarAviso(string clave, string fallback)
    {
        if (textoAvisos == null) return;
        textoAvisos.text = LocalizationManager.Instancia != null 
            ? LocalizationManager.Instancia.ObtenerTexto(clave, fallback) 
            : fallback;
    }

    /// <summary>
    /// Inicia el flujo de registro de un nuevo usuario empleando las credenciales
    /// ingresadas en la interfaz de usuario, validando previamente los requisitos de formato y seguridad.
    /// </summary>
    public void RegistrarUsuarioConEmail()
    {
        string idUsuario = inputEmail.text.Trim(); 
        string password = inputPassword.text;

        // 1. Validaciones Locales - Identificador (Username)
        if (string.IsNullOrEmpty(idUsuario))
        {
            MostrarAviso("auth_aviso_id_vacio", "Por favor, introduce un identificador.");
            return;
        }

        if (idUsuario.Contains(" "))
        {
            MostrarAviso("auth_aviso_id_espacios", "El ID no puede contener espacios en blanco.");
            return;
        }

        if (idUsuario.Length < 3)
        {
            MostrarAviso("auth_aviso_id_corto", "El ID debe tener al menos 3 caracteres.");
            return;
        }

        // 2. Validaciones Locales - Contraseña (Password)
        if (string.IsNullOrEmpty(password))
        {
            MostrarAviso("auth_aviso_pass_vacio", "Por favor, introduce una contraseña.");
            return;
        }

        if (password.Length < 8)
        {
            MostrarAviso("auth_aviso_pass_corta", "La contraseña debe tener al menos 8 caracteres.");
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
            MostrarAviso("auth_aviso_pass_formato", "La contraseña requiere al menos una letra y un número.");
            return;
        }

        // Formatear identificador a correo si no contiene @
        string emailParaFirebase = idUsuario;
        if (!emailParaFirebase.Contains("@"))
        {
            emailParaFirebase = idUsuario + "@flappyspacecat.com";
        }

        MostrarAviso("auth_aviso_conectando", "Estableciendo conexión...");

        auth.CreateUserWithEmailAndPasswordAsync(emailParaFirebase, password).ContinueWithOnMainThread(tarea =>
        {
            if (tarea.IsCanceled)
            {
                Debug.LogError("El proceso de registro fue cancelado.");
                MostrarAviso("auth_aviso_cancelado", "Conexión cancelada.");
                return;
            }
            if (tarea.IsFaulted)
            {
                Debug.LogError("Excepción en el registro: " + tarea.Exception);
                
                string claveError = "auth_error_crear_cuenta";
                string fallbackError = "Error al crear la cuenta. Verifique los datos.";
                
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
                            IniciarSesionUsuarioConEmail(emailParaFirebase, password);
                            return;
                        case AuthError.InvalidEmail:
                            claveError = "auth_error_id_invalido";
                            fallbackError = "El ID no tiene un formato válido.";
                            break;
                        case AuthError.WeakPassword:
                            claveError = "auth_error_pass_debil";
                            fallbackError = "La contraseña es demasiado débil.";
                            break;
                        case AuthError.NetworkRequestFailed:
                            claveError = "auth_error_red";
                            fallbackError = "Error de red. Verifica tu conexión.";
                            break;
                        default:
                            fallbackError = "Error de registro: " + firebaseEx.Message;
                            break;
                    }
                }
                else
                {
                    // Fallback de contingencia mediante búsqueda por texto
                    if (tarea.Exception.ToString().Contains("EmailAlreadyInUse") || tarea.Exception.ToString().Contains("already in use"))
                    {
                        IniciarSesionUsuarioConEmail(emailParaFirebase, password);
                        return;
                    }
                }

                MostrarAviso(claveError, fallbackError);
                return;
            }

            AuthResult resultado = tarea.Result;
            Debug.Log("Usuario registrado exitosamente. ID: " + resultado.User.UserId);
            MostrarAviso("auth_exito_creada", "Cuenta creada exitosamente.");

            // Guardar confirmación persistente de inicio de sesión establecido
            SecurePrefs.SetInt("SesionIniciadaConExito", 1);
            PlayerPrefs.SetInt("SesionIniciadaConExito", 1);
            SecurePrefs.Save();
            PlayerPrefs.Save();

            if (PerfilManager.Instancia != null)
            {
                PerfilManager.Instancia.ActualizarVisibilidadBotonLogin();
            }

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
    /// Intenta iniciar sesión con el correo y contraseña proporcionados cuando el usuario ya existe.
    /// </summary>
    private void IniciarSesionUsuarioConEmail(string email, string password)
    {
        MostrarAviso("auth_aviso_iniciando", "Iniciando sesión...");

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(tarea =>
        {
            if (tarea.IsCanceled)
            {
                Debug.LogError("El proceso de inicio de sesión fue cancelado.");
                MostrarAviso("auth_aviso_cancelado", "Conexión cancelada.");
                return;
            }
            if (tarea.IsFaulted)
            {
                Debug.LogError("Excepción en el inicio de sesión: " + tarea.Exception);
                
                string claveError = "auth_error_pass_incorrecta";
                string fallbackError = "La contraseña no coincide con el ID registrado.";
                
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
                        case AuthError.WrongPassword:
                            claveError = "auth_error_pass_incorrecta";
                            fallbackError = "La contraseña no coincide con el ID registrado.";
                            break;
                        case AuthError.UserNotFound:
                            claveError = "auth_error_usuario_no_encontrado";
                            fallbackError = "Usuario no encontrado.";
                            break;
                        case AuthError.NetworkRequestFailed:
                            claveError = "auth_error_red";
                            fallbackError = "Error de red. Verifica tu conexión.";
                            break;
                        default:
                            if (firebaseEx.Message.Contains("password") || firebaseEx.Message.Contains("credential") || firebaseEx.Message.Contains("error"))
                            {
                                claveError = "auth_error_pass_incorrecta";
                                fallbackError = "La contraseña no coincide con el ID registrado.";
                            }
                            else
                            {
                                fallbackError = "Error al iniciar sesión: " + firebaseEx.Message;
                            }
                            break;
                    }
                }

                MostrarAviso(claveError, fallbackError);
                return;
            }

            AuthResult resultado = tarea.Result;
            Debug.Log("Usuario inició sesión exitosamente. ID: " + resultado.User.UserId);
            MostrarAviso("auth_exito_iniciada", "Sesión iniciada correctamente.");

            // Guardar confirmación persistente de inicio de sesión establecido
            SecurePrefs.SetInt("SesionIniciadaConExito", 1);
            PlayerPrefs.SetInt("SesionIniciadaConExito", 1);
            SecurePrefs.Save();
            PlayerPrefs.Save();

            if (PerfilManager.Instancia != null)
            {
                PerfilManager.Instancia.ActualizarVisibilidadBotonLogin();
            }

            AnalyticsManager.Instancia.RegistrarEventoSimple("login_email_exito");

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

            // Solo mostrar Panel GameOver si no estamos en el menú principal o perfil
            if (PerfilManager.Instancia != null && PerfilManager.Instancia.panelPerfil != null && PerfilManager.Instancia.panelPerfil.activeSelf)
            {
                // Estamos en el panel de perfil
            }
            else if (GameManager.Instancia.panelMenuPrincipal != null && GameManager.Instancia.panelMenuPrincipal.activeSelf)
            {
                // Estamos en el menú principal
            }
            else if (GameManager.Instancia.panelGameOver != null)
            {
                GameManager.Instancia.panelGameOver.SetActive(true);
            }
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

                // Guardar confirmación persistente de inicio de sesión establecido
                SecurePrefs.SetInt("SesionIniciadaConExito", 1);
                PlayerPrefs.SetInt("SesionIniciadaConExito", 1);
                SecurePrefs.Save();
                PlayerPrefs.Save();

                if (PerfilManager.Instancia != null)
                {
                    PerfilManager.Instancia.ActualizarVisibilidadBotonLogin();
                }

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
    /// desde que abrió el juego por primera vez para regalarle la skin de Cowsmo (índice 5).
    /// El logro MuuuyAlto se desbloqueará cuando juegue su primera partida con dicha skin.
    /// </summary>
    private void ComprobarRecompensaRegistro()
    {
        // Se comprueba si ya se le ha entregado la skin de Cowsmo (índice 5) para no repetir
        if (SecurePrefs.GetInt("RecompensaCowEntregada", 0) == 1 && SecurePrefs.GetInt("SkinDesbloqueada_5", 0) == 1)
        {
            return;
        }

        string fechaString = SecurePrefs.GetString("FechaPrimeraApertura", "");
        
        // Fallback: Si no hay fecha registrada por seguridad extra, se establece ahora mismo
        if (string.IsNullOrEmpty(fechaString))
        {
            fechaString = System.DateTime.Now.ToString("O");
            SecurePrefs.SetString("FechaPrimeraApertura", fechaString);
            SecurePrefs.Save();
        }

        System.DateTime fechaPrimeraApertura;
        // Se usa TryParse genérico para evitar fallos en el formato de fecha regional
        if (System.DateTime.TryParse(fechaString, out fechaPrimeraApertura))
        {
            System.TimeSpan tiempoTranscurrido = System.DateTime.Now - fechaPrimeraApertura;
            
            // Si han pasado 24 horas o menos
            if (tiempoTranscurrido.TotalHours <= 24)
            {
                // Skin de Cow (Cowsmo) en la lista de skins = 5
                int indiceSkinCow = 5; 
                
                SecurePrefs.SetInt("SkinDesbloqueada_" + indiceSkinCow, 1);
                SecurePrefs.SetInt("RecompensaCowEntregada", 1);
                SecurePrefs.Save();
                
                Debug.Log("¡Recompensa de registro entregada! Skin de Cowsmo (índice 5) desbloqueada.");
                if (textoAvisos != null) 
                    textoAvisos.text = "¡Felicidades! Has recibido la skin Cowsmo por registrarte hoy.";

                // Guardar también en la base de datos Firestore si el usuario está autenticado
                if (auth != null && auth.CurrentUser != null && DatabaseManager.Instancia != null)
                {
                    DatabaseManager.Instancia.GuardarSkinsDesbloqueadas(auth.CurrentUser.UserId, indiceSkinCow);
                }

                // IMPORTANTE: Notificar de inmediato a la tienda y actualizar la UI para que se visualice
                TiendaSkinsManager tienda = FindAnyObjectByType<TiendaSkinsManager>();
                if (tienda != null)
                {
                    tienda.RefrescarTiendaCompleta();
                }
            }
        }
    }
}
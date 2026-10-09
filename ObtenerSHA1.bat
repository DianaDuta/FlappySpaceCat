@echo off
setlocal enabledelayedexpansion

echo ==========================================================
echo    Flappy Space Cat - Extractor de Huellas SHA-1 / SHA-256
echo ==========================================================
echo.

:: 1. Buscar debug.keystore
set "DEBUG_KEYSTORE=%USERPROFILE%\.android\debug.keystore"
if not exist "%DEBUG_KEYSTORE%" (
    echo [AVISO] No se encontro debug.keystore en %DEBUG_KEYSTORE%.
    echo Si aun no has compilado para Android, es normal.
    echo.
) else (
    echo [INFO] Keystore de desarrollo encontrado: %DEBUG_KEYSTORE%
)

:: 2. Buscar keytool.exe
set "KEYTOOL_PATH=keytool"

:: Comprobar si keytool esta en el PATH
where keytool >nul 2>nul
if %errorlevel% equ 0 (
    echo [INFO] 'keytool' detectado en las variables de entorno.
) else (
    echo [INFO] 'keytool' no esta en el PATH. Buscando en la ruta de Unity Hub...
    
    :: Buscar en Program Files\Unity\Hub\Editor
    set "UNITY_HUB_DIR=%PROGRAMFILES%\Unity\Hub\Editor"
    set "FOUND_KEYTOOL="
    
    if exist "!UNITY_HUB_DIR!" (
        for /r "!UNITY_HUB_DIR!" %%f in (keytool.exe) do (
            if exist "%%f" (
                set "FOUND_KEYTOOL=%%f"
                goto :found_keytool
            )
        )
    )
    
    :found_keytool
    if defined FOUND_KEYTOOL (
        set "KEYTOOL_PATH=!FOUND_KEYTOOL!"
        echo [INFO] 'keytool' encontrado en: !KEYTOOL_PATH!
    ) else (
        echo [ALERTA] No se pudo encontrar 'keytool.exe' automaticamente.
        echo Asegurate de tener Java JDK instalado o de ejecutar este script
        echo despues de agregar keytool a las variables de entorno.
        echo.
        goto :menu
    )
)

:menu
echo ----------------------------------------------------------
echo Elige una opcion:
echo 1. Obtener SHA-1 y SHA-256 del Keystore de Desarrollo (Debug)
echo 2. Obtener SHA-1 y SHA-256 de un Keystore de Produccion (Release)
echo 3. Salir
echo ----------------------------------------------------------
set /p opcion="Selecciona una opcion (1-3): "

if "%opcion%"=="1" goto :debug_keystore
if "%opcion%"=="2" goto :release_keystore
if "%opcion%"=="3" goto :eof
goto :menu

:debug_keystore
if not exist "%DEBUG_KEYSTORE%" (
    echo [ERROR] No existe el keystore de desarrollo en %DEBUG_KEYSTORE%.
    echo Abre Unity, compila el juego una vez para Android y se generara automaticamente.
    pause
    goto :menu
)
echo.
echo Ejecutando keytool para el keystore de desarrollo...
echo La contrasena por defecto es 'android' (se ingresa automaticamente).
echo.
"%KEYTOOL_PATH%" -list -v -keystore "%DEBUG_KEYSTORE%" -storepass android -alias androidtype 2>nul
if %errorlevel% neq 0 (
    :: Reintentar sin alias por si acaso
    "%KEYTOOL_PATH%" -list -v -keystore "%DEBUG_KEYSTORE%" -storepass android
)
echo.
echo ==========================================================
echo Copia la huella SHA-1 de arriba y pegala en la consola de Firebase.
echo ==========================================================
pause
goto :menu

:release_keystore
echo.
set /p path_keystore="Arrastra o escribe la ruta de tu keystore de produccion (.keystore): "
:: Limpiar comillas
set "path_keystore=%path_keystore:"=%"

if not exist "%path_keystore%" (
    echo [ERROR] Archivo no encontrado en: %path_keystore%
    pause
    goto :menu
)

set /p alias_keystore="Escribe el Alias de la clave: "
echo.
echo Ejecutando keytool para el keystore de produccion...
echo Se te solicitara la contrasena del Keystore.
echo.
"%KEYTOOL_PATH%" -list -v -keystore "%path_keystore%" -alias "%alias_keystore%"
if %errorlevel% neq 0 (
    :: Reintentar sin alias
    "%KEYTOOL_PATH%" -list -v -keystore "%path_keystore%"
)
echo.
echo ==========================================================
echo Copia la huella SHA-1 de arriba y pegala en la consola de Firebase.
echo ==========================================================
pause
goto :menu

:eof
echo Saliendo...

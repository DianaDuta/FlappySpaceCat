using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

/**
 * CLASE SECURE PREFS
 * Protege los datos locales (Gemas, Récords, etc.) encriptándolos 
 * para que los jugadores no puedan modificar el archivo de guardado del móvil haciendo trampas.
 */
public static class SecurePrefs
{
    // Clave secreta para encriptar los datos del juego
    private static readonly string secretKey = "fL4pPy_C4t_s3cR3t_k3Y_2026!"; 
    private static readonly byte[] salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

    public static void SetInt(string key, int value)
    {
        SetString(key, value.ToString());
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        string result = GetString(key, "");
        if (string.IsNullOrEmpty(result)) return defaultValue;
        
        if (int.TryParse(result, out int res)) return res;
        return defaultValue;
    }

    public static void SetString(string key, string value)
    {
        string encryptedValue = Encrypt(value);
        string hash = GenerateHash(encryptedValue);
        PlayerPrefs.SetString(key, encryptedValue);
        PlayerPrefs.SetString(key + "_HASH", hash);
    }

    public static string GetString(string key, string defaultValue = "")
    {
        string encryptedValue = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(encryptedValue)) return defaultValue;

        string savedHash = PlayerPrefs.GetString(key + "_HASH", "");
        string expectedHash = GenerateHash(encryptedValue);

        // Si el hash no cuadra, alguien ha modificado el archivo de guardado trampa
        if (savedHash != expectedHash)
        {
            Debug.LogWarning("SecurePrefs: ¡Alerta de trampas! Datos corruptos o alterados en la clave: " + key);
            return defaultValue;
        }

        try {
            return Decrypt(encryptedValue);
        } catch {
            return defaultValue;
        }
    }

    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.DeleteKey(key + "_HASH");
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    // --- MÉTODOS DE ENCRIPTACIÓN (XOR Cifrado Básico + Sal) ---
    private static string Encrypt(string plainText)
    {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] keyBytes = new Rfc2898DeriveBytes(secretKey, salt, 1000).GetBytes(16);
        for(int i = 0; i < plainTextBytes.Length; i++) {
             plainTextBytes[i] = (byte)(plainTextBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }
        return Convert.ToBase64String(plainTextBytes);
    }

    private static string Decrypt(string encryptedText)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] keyBytes = new Rfc2898DeriveBytes(secretKey, salt, 1000).GetBytes(16);
        for(int i = 0; i < encryptedBytes.Length; i++) {
             encryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }
        return Encoding.UTF8.GetString(encryptedBytes);
    }

    private static string GenerateHash(string data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data + secretKey));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes) builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}

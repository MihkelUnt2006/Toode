using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tooded_EFCore;

public class EmailAndmed
{
    public string Saatja { get; set; } = "";
    public string Parool { get; set; } = "";
}

// E-posti konto andmed. Parooli EI kirjutata koodi.
// 1) Keskkonnamuutujad TOODED_SMTP_USER ja TOODED_SMTP_PASS (kui mõlemad on olemas)
// 2) Fail AppData\Local\ToodedApp\email.dat: parool on krüpteeritud Windows DPAPI-ga
//    (seda saab lahti krüpteerida ainult sama Windowsi kasutaja samas arvutis).
public static class EmailSeaded
{
    private static readonly string Fail = Path.Combine(Kaustad.Juur, "email.dat");

    internal class Salvestatud
    {
        public string Saatja { get; set; } = "";
        public string ParoolKrupteeritud { get; set; } = "";
    }

    public static EmailAndmed? Lae()
    {
        string? kasutaja = Environment.GetEnvironmentVariable("TOODED_SMTP_USER");
        string? parool = Environment.GetEnvironmentVariable("TOODED_SMTP_PASS");
        if (!string.IsNullOrWhiteSpace(kasutaja) && !string.IsNullOrWhiteSpace(parool))
            return new EmailAndmed { Saatja = kasutaja.Trim(), Parool = parool.Replace(" ", "") };

        try
        {
            if (!File.Exists(Fail)) return null;
            var s = JsonSerializer.Deserialize<Salvestatud>(File.ReadAllText(Fail));
            if (s == null || s.Saatja == "" || s.ParoolKrupteeritud == "") return null;

            byte[] lahti = ProtectedData.Unprotect(
                Convert.FromBase64String(s.ParoolKrupteeritud), null, DataProtectionScope.CurrentUser);
            return new EmailAndmed { Saatja = s.Saatja, Parool = Encoding.UTF8.GetString(lahti) };
        }
        catch
        {
            return null;   // fail rikutud või teise kasutaja loodud
        }
    }

    // Ainult saatja aadress (seadete vormi eeltäitmiseks)
    public static string LaeSaatja()
    {
        try
        {
            if (!File.Exists(Fail)) return "";
            return JsonSerializer.Deserialize<Salvestatud>(File.ReadAllText(Fail))?.Saatja ?? "";
        }
        catch { return ""; }
    }

    public static void Salvesta(string saatja, string parool)
    {
        byte[] krupt = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(parool.Replace(" ", "")), null, DataProtectionScope.CurrentUser);

        var s = new Salvestatud { Saatja = saatja.Trim(), ParoolKrupteeritud = Convert.ToBase64String(krupt) };
        Directory.CreateDirectory(Kaustad.Juur);
        File.WriteAllText(Fail, JsonSerializer.Serialize(s));
    }
}

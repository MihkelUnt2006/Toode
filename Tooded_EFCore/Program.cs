using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Kaustad.Loo();

        try
        {
            using var db = new ToodedContext();
            db.Database.Migrate();      // loob andmebaasi ja rakendab migratsioonid
            LooAlgkasutajad(db);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Andmebaasi ettevalmistamine ebaõnnestus:\n" + ex.Message,
                "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(new LoginForm());
    }

    private static void LooAlgkasutajad(ToodedContext db)
    {
        if (db.Kasutajad.Any()) return;

        db.Kasutajad.AddRange(
            new Kasutaja { Nimi = "Admin", Email = "admin@admin.ee",
                           ParoolHash = PasswordHelper.Hash("admin123"), Roll = "admin" },
            new Kasutaja { Nimi = "Kasutaja", Email = "kasutaja@pood.ee",
                           ParoolHash = PasswordHelper.Hash("kasutaja123"), Roll = "kasutaja" });
        db.SaveChanges();
    } 
}

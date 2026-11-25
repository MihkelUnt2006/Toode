using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public class OstukorviRida
{
    public Toode Toode { get; set; } = null!;
    public int Kogus { get; set; }

    public decimal Summa => Toode.Hind * Kogus;

    public override string ToString() =>
        $"{Toode.Toodenimetus} – {Kogus} tk × {Toode.Hind:F2} € = {Summa:F2} €";
}

public static class Ostukorv
{
    public static List<OstukorviRida> Ridad { get; } = new();

    public static decimal Kokku => Ridad.Sum(r => r.Summa);

    // Kui toode on juba korvis, suurendatakse kogust.
    // Tagastab false, kui kogus ületaks laoseisu (siis korvi ei muudeta).
    public static bool Lisa(Toode toode, int kogus)
    {
        var olemas = Ridad.FirstOrDefault(r => r.Toode.Id == toode.Id);
        int uusKogus = (olemas?.Kogus ?? 0) + kogus;
        if (kogus < 1 || uusKogus > toode.Kogus) return false;

        if (olemas != null)
        {
            olemas.Kogus = uusKogus;
            olemas.Toode = toode;       // värske laoseis ja hind
        }
        else
            Ridad.Add(new OstukorviRida { Toode = toode, Kogus = kogus });
        return true;
    }

    // Muudab rea koguse. false = kogus < 1 või rohkem kui laos
    public static bool MuudaKogust(OstukorviRida rida, int uusKogus)
    {
        if (uusKogus < 1 || uusKogus > rida.Toode.Kogus) return false;
        rida.Kogus = uusKogus;
        return true;
    }

    public static int KorvisKogus(int toodeId) =>
        Ridad.FirstOrDefault(r => r.Toode.Id == toodeId)?.Kogus ?? 0;

    // Loeb andmebaasist uuesti iga korvi toote hinna ja laoseisu
    public static void UuendaLaoseis()
    {
        using var db = new ToodedContext();
        foreach (var rida in Ridad)
        {
            var uus = db.Toodetabel.Find(rida.Toode.Id);
            if (uus != null) rida.Toode = uus;
        }
    }

    // Kinnitab ostu: kontrollib laoseisu, vähendab kogused, salvestab ostu.
    // Kõik toimub ühes tehingus: kui midagi läheb valesti, ei muutu andmebaasis midagi.
    public static Ost? Kinnita(int kasutajaId, out string? viga)
    {
        viga = null;
        if (Ridad.Count == 0)
        {
            viga = "Ostukorv on tühi.";
            return null;
        }

        try
        {
            using var db = new ToodedContext();
            using var tehing = db.Database.BeginTransaction();

            var ost = new Ost { Kuupaev = DateTime.Now, KasutajaId = kasutajaId };

            foreach (var rida in Ridad)
            {
                var toode = db.Toodetabel.Find(rida.Toode.Id);
                if (toode == null)
                {
                    viga = $"Toodet \"{rida.Toode.Toodenimetus}\" ei ole enam olemas. Eemalda see korvist.";
                    return null;
                }
                if (rida.Kogus > toode.Kogus)
                {
                    viga = $"Toodet \"{toode.Toodenimetus}\" on laos ainult {toode.Kogus} tk, korvis on {rida.Kogus} tk.";
                    return null;
                }

                toode.Kogus -= rida.Kogus;   // laoseisu vähendamine
                ost.Read.Add(new OstuRida
                {
                    ToodeId = toode.Id,
                    Toodenimetus = toode.Toodenimetus,
                    Kogus = rida.Kogus,
                    Hind = toode.Hind        // hind andmebaasist, mitte korvist
                });
            }

            db.Ostud.Add(ost);
            db.SaveChanges();
            tehing.Commit();

            Ridad.Clear();
            return ost;
        }
        catch (Exception ex)
        {
            viga = "Ostu salvestamine ebaõnnestus: " + ex.Message;
            return null;
        }
    }

    public static void Tyhjenda() => Ridad.Clear();
}

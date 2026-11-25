using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

// Üks kinnitatud ost (arve päis)
public class Ost
{
    public int Id { get; set; }
    public DateTime Kuupaev { get; set; }

    public int KasutajaId { get; set; }
    public Kasutaja Kasutaja { get; set; } = null!;

    public ICollection<OstuRida> Read { get; set; } = new List<OstuRida>();

    [NotMapped]
    public decimal Kokku => Read.Sum(r => r.Summa);
}

// Ostu üks rida. Nimetus ja hind salvestatakse ostu hetkel,
// et ajalugu jääks õigeks ka siis, kui toodet hiljem muudetakse või kustutatakse.
public class OstuRida
{
    public int Id { get; set; }

    public int OstId { get; set; }
    public Ost Ost { get; set; } = null!;

    public int? ToodeId { get; set; }        // null = toode on vahepeal kustutatud
    public Toode? Toode { get; set; }

    public string Toodenimetus { get; set; } = "";
    public int Kogus { get; set; }

    [Precision(10, 2)]
    public decimal Hind { get; set; }

    [NotMapped]
    public decimal Summa => Hind * Kogus;

    public override string ToString() =>
        $"{Toodenimetus} – {Kogus} tk × {Hind:F2} € = {Summa:F2} €";
}

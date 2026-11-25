using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Tooded_EFCore;

// PDF-arve loomine salvestatud ostu põhjal (andmed tulevad andmebaasist, mitte ostukorvist)
public static class ArvePdf
{
    public static string Loo(Ost ost, string klient)
    {
        string tee = Path.Combine(Kaustad.Arved, $"Arve_{ost.Id}_{ost.Kuupaev:yyyyMMdd_HHmmss}.pdf");

        var opts = new XPdfFontOptions(PdfFontEncoding.Unicode);
        var pealkiri = new XFont("Arial", 20, XFontStyle.Bold, opts);
        var tavaline = new XFont("Arial", 12, XFontStyle.Regular, opts);
        var paks = new XFont("Arial", 12, XFontStyle.Bold, opts);
        var suur = new XFont("Arial", 14, XFontStyle.Bold, opts);

        using var dokument = new PdfDocument();
        PdfPage leht = dokument.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(leht);

        try
        {
            gfx.DrawString($"ARVE nr {ost.Id}", pealkiri, XBrushes.Black,
                new XRect(0, 30, leht.Width, 30), XStringFormats.TopCenter);
            gfx.DrawString($"Kuupäev: {ost.Kuupaev:dd.MM.yyyy HH:mm}", tavaline, XBrushes.Black, 40, 90);
            gfx.DrawString($"Klient: {klient}", tavaline, XBrushes.Black, 40, 110);

            double y = 145;
            TabeliPais(gfx, paks, y);
            y += 24;

            foreach (var rida in ost.Read.OrderBy(r => r.Id))
            {
                if (y > leht.Height.Point - 90)   // lehe lõpp: uus leht
                {
                    gfx.Dispose();
                    leht = dokument.AddPage();
                    gfx = XGraphics.FromPdfPage(leht);
                    y = 40;
                    TabeliPais(gfx, paks, y);
                    y += 24;
                }

                string nimi = rida.Toodenimetus.Length > 34
                    ? rida.Toodenimetus.Substring(0, 33) + "…"
                    : rida.Toodenimetus;

                gfx.DrawString(nimi, tavaline, XBrushes.Black, new XRect(40, y, 260, 18), XStringFormats.TopLeft);
                gfx.DrawString(rida.Kogus.ToString(), tavaline, XBrushes.Black, new XRect(300, y, 50, 18), XStringFormats.TopRight);
                gfx.DrawString($"{rida.Hind:F2} €", tavaline, XBrushes.Black, new XRect(360, y, 80, 18), XStringFormats.TopRight);
                gfx.DrawString($"{rida.Summa:F2} €", tavaline, XBrushes.Black, new XRect(450, y, 100, 18), XStringFormats.TopRight);
                y += 20;
            }

            gfx.DrawLine(XPens.Black, 40, y + 4, 550, y + 4);
            gfx.DrawString($"Kokku: {ost.Kokku:F2} €", suur, XBrushes.Black,
                new XRect(300, y + 12, 250, 22), XStringFormats.TopRight);
        }
        finally
        {
            gfx.Dispose();
        }

        dokument.Save(tee);
        return tee;
    }

    private static void TabeliPais(XGraphics gfx, XFont font, double y)
    {
        gfx.DrawString("Toode", font, XBrushes.Black, new XRect(40, y, 260, 18), XStringFormats.TopLeft);
        gfx.DrawString("Kogus", font, XBrushes.Black, new XRect(300, y, 50, 18), XStringFormats.TopRight);
        gfx.DrawString("Hind", font, XBrushes.Black, new XRect(360, y, 80, 18), XStringFormats.TopRight);
        gfx.DrawString("Summa", font, XBrushes.Black, new XRect(450, y, 100, 18), XStringFormats.TopRight);
        gfx.DrawLine(XPens.Black, 40, y + 19, 550, y + 19);
    }
}

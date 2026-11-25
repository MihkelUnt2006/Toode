using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class Form3 : Form
{
    private readonly ToodedContext _db = new();
    private readonly TabControl tabPood = new();

    // Otsing ja sorteerimine (ülemine riba)
    private readonly Panel paneelYlal = new();
    private readonly Label lblOtsi = new(), lblSorteeri = new();
    private readonly TextBox txtOtsing = new();
    private readonly ComboBox comboSorteeri = new();

    private List<Kategooria> _kategooriad = new();
    private readonly Dictionary<string, Image?> _pildid = new();   // pildid laetakse kettalt ainult ühe korra

    public Form3()
    {
        InitializeComponent();
        Text = "Pood";
        ClientSize = new Size(930, 630);

        tabPood.Dock = DockStyle.Fill;
        Controls.Add(tabPood);

        paneelYlal.Dock = DockStyle.Top;
        paneelYlal.Height = 38;

        lblOtsi.SetBounds(10, 11, 40, 20);
        lblOtsi.Text = "Otsi:";
        txtOtsing.SetBounds(55, 6, 220, 27);

        lblSorteeri.SetBounds(300, 11, 65, 20);
        lblSorteeri.Text = "Sorteeri:";
        comboSorteeri.SetBounds(370, 6, 220, 27);
        comboSorteeri.DropDownStyle = ComboBoxStyle.DropDownList;
        comboSorteeri.Items.AddRange(new object[]
        {
            "Nimetus A-Z",
            "Hind: odavamast kallimani",
            "Hind: kallimast odavamani"
        });
        comboSorteeri.SelectedIndex = 0;

        paneelYlal.Controls.AddRange(new Control[] { lblOtsi, txtOtsing, lblSorteeri, comboSorteeri });
        Controls.Add(paneelYlal);   // lisatakse pärast TabControli, et ta jääks ülesse

        _kategooriad = _db.Kategooriatabel
            .Include(k => k.Tooded)          // laeb kohe ka kategooria tooted
            .OrderBy(k => k.Kategooria_nimetus)
            .ToList();

        txtOtsing.TextChanged += (s, e) => LaePood();
        comboSorteeri.SelectedIndexChanged += (s, e) => LaePood();

        LaePood();
    }

    private Image? PiltMallist(string? failinimi)
    {
        if (string.IsNullOrEmpty(failinimi)) return null;
        if (!_pildid.TryGetValue(failinimi, out var pilt))
        {
            pilt = Kaustad.LaePilt(failinimi);
            _pildid[failinimi] = pilt;
        }
        return pilt;
    }

    private void LaePood()
    {
        int varemValitud = tabPood.SelectedIndex;
        string otsing = txtOtsing.Text.Trim();
        int sort = comboSorteeri.SelectedIndex;

        foreach (var vana in tabPood.TabPages.Cast<TabPage>().ToList())
        {
            tabPood.TabPages.Remove(vana);
            vana.Dispose();
        }

        int esimeneTulemusega = -1;

        foreach (var kat in _kategooriad)
        {
            var leht = new TabPage(kat.Kategooria_nimetus) { AutoScroll = true };

            // Otsing ja sorteerimine tehakse mälus: SQLite ei oska decimal-i järgi sorteerida
            // ning LIKE ei tee täpitähtedel (õ, ä, ö, ü) suur/väiketähe vahet.
            IEnumerable<Toode> tooted = kat.Tooded;
            if (otsing.Length > 0)
                tooted = tooted.Where(t => t.Toodenimetus.Contains(otsing, StringComparison.CurrentCultureIgnoreCase));

            tooted = sort switch
            {
                1 => tooted.OrderBy(t => t.Hind).ThenBy(t => t.Toodenimetus),
                2 => tooted.OrderByDescending(t => t.Hind).ThenBy(t => t.Toodenimetus),
                _ => tooted.OrderBy(t => t.Toodenimetus)
            };

            int i = 0;
            foreach (var toode in tooted)
            {
                var kaart = LooTootekaart(toode);
                // 5 kaarti reas: veerg = i % 5, rida = i / 5
                kaart.Location = new Point(10 + (i % 5) * 175, 10 + (i / 5) * 250);
                leht.Controls.Add(kaart);
                i++;
            }

            if (i > 0 && esimeneTulemusega < 0) esimeneTulemusega = tabPood.TabPages.Count;
            tabPood.TabPages.Add(leht);
        }

        if (tabPood.TabPages.Count == 0) return;

        int uus = (varemValitud >= 0 && varemValitud < tabPood.TabPages.Count) ? varemValitud : 0;
        // Otsingu ajal hüppa esimesele vahelehele, kus midagi leiti
        if (otsing.Length > 0 && tabPood.TabPages[uus].Controls.Count == 0 && esimeneTulemusega >= 0)
            uus = esimeneTulemusega;
        tabPood.SelectedIndex = uus;
    }

    private Panel LooTootekaart(Toode t)
    {
        var kaart = new Panel { Size = new Size(160, 235), BorderStyle = BorderStyle.FixedSingle };

        var pilt = new PictureBox
        {
            Bounds = new Rectangle(5, 5, 148, 140),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = PiltMallist(t.Pilt)
        };

        var silt = new Label
        {
            Bounds = new Rectangle(5, 150, 148, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Text = $"{t.Toodenimetus}\n{t.Hind:F2} €"
        };

        var kogus = new NumericUpDown
        {
            Bounds = new Rectangle(5, 197, 60, 27),
            Minimum = 1,
            Maximum = Math.Max(1, t.Kogus),   // rohkem kui laos ei saa valida
            Value = 1
        };

        var nupp = new Button
        {
            Bounds = new Rectangle(70, 195, 83, 30),
            Text = "Lisa korvi",
            Enabled = t.Kogus > 0             // laos pole = nupp hall
        };

        nupp.Click += (s, e) =>
        {
            int jubaKorvis = Ostukorv.KorvisKogus(t.Id);
            if (!Ostukorv.Lisa(t, (int)kogus.Value))
            {
                MessageBox.Show($"Laos on {t.Kogus} tk, korvis juba {jubaKorvis} tk.\nRohkem lisada ei saa.");
                return;
            }
            MessageBox.Show($"{t.Toodenimetus} ({kogus.Value} tk) lisati ostukorvi.");
        };

        pilt.Click += (s, e) =>
            MessageBox.Show($"{t.Toodenimetus}\nHind: {t.Hind:F2} €\nLaos: {t.Kogus} tk");

        kaart.Controls.AddRange(new Control[] { pilt, silt, kogus, nupp });
        return kaart;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        foreach (var pilt in _pildid.Values) pilt?.Dispose();
        _db.Dispose();
        base.OnFormClosed(e);
    }
}

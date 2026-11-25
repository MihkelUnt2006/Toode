using System.Net;
using System.Net.Mail;

namespace Tooded_EFCore;

public partial class ArveForm : Form
{
    private readonly ListBox listOstukorv = new();
    private readonly Label lblKokku = new();
    private readonly TextBox txtEmail = new();
    private readonly NumericUpDown numKogus = new();
    private readonly Button btnEemalda = new(), btnMuudaKogust = new(), btnKinnita = new(),
                            btnGenPDF = new(), btnSaadaEmail = new(), btnEmailSeaded = new();

    private Ost? viimaneOst;      // viimati kinnitatud ost
    private string? viimanePdf;   // viimati loodud arve täistee

    public ArveForm()
    {
        InitializeComponent();
        Text = "Ostukorv ja arve";
        ClientSize = new Size(600, 380);

        Ostukorv.UuendaLaoseis();   // värske hind ja laoseis

        listOstukorv.SetBounds(20, 20, 330, 300);
        listOstukorv.SelectedIndexChanged += (s, e) => UuendaKogusevali();
        lblKokku.SetBounds(20, 330, 330, 30);
        lblKokku.Font = new Font(Font, FontStyle.Bold);

        txtEmail.SetBounds(370, 20, 200, 27);
        txtEmail.PlaceholderText = "Saaja e-post";

        numKogus.SetBounds(370, 119, 60, 27);
        numKogus.Minimum = 1;
        numKogus.Maximum = 1;
        numKogus.Value = 1;

        SeaNupp(btnEemalda, "Eemalda valitud rida", 70, btnEemalda_Click);
        btnMuudaKogust.SetBounds(440, 115, 130, 35);
        btnMuudaKogust.Text = "Muuda kogust";
        btnMuudaKogust.Click += btnMuudaKogust_Click;
        SeaNupp(btnKinnita, "Kinnita ost", 160, btnKinnita_Click);
        SeaNupp(btnGenPDF, "Genereeri PDF", 205, btnGenPDF_Click);
        SeaNupp(btnSaadaEmail, "Saada e-postiga", 250, btnSaadaEmail_Click);
        SeaNupp(btnEmailSeaded, "E-posti seaded", 295, btnEmailSeaded_Click);
        btnEmailSeaded.Enabled = AktiivneKasutaja.OnAdmin;   // seadistab administraator

        Controls.AddRange(new Control[] { listOstukorv, lblKokku, txtEmail, numKogus,
            btnEemalda, btnMuudaKogust, btnKinnita, btnGenPDF, btnSaadaEmail, btnEmailSeaded });

        NaitaKorvi();
    }

    private void SeaNupp(Button nupp, string tekst, int y, EventHandler kasitleja)
    {
        nupp.Text = tekst;
        nupp.SetBounds(370, y, 200, 35);
        nupp.Click += kasitleja;
    }

    private void NaitaKorvi(int valitud = -1)
    {
        listOstukorv.Items.Clear();
        foreach (var rida in Ostukorv.Ridad)
            listOstukorv.Items.Add(rida);   // ListBox kuvab OstukorviRida.ToString()

        if (valitud >= 0 && valitud < listOstukorv.Items.Count)
            listOstukorv.SelectedIndex = valitud;

        lblKokku.Text = $"Kokku: {Ostukorv.Kokku:F2} €";
    }

    // Kui rida valitakse, piiratakse kogus laoseisuga
    private void UuendaKogusevali()
    {
        if (listOstukorv.SelectedItem is not OstukorviRida rida) return;
        numKogus.Maximum = Math.Max(1, rida.Toode.Kogus);
        numKogus.Value = Math.Min(Math.Max(1, rida.Kogus), numKogus.Maximum);
    }

    private void btnEemalda_Click(object? sender, EventArgs e)
    {
        if (listOstukorv.SelectedItem is OstukorviRida rida)
        {
            Ostukorv.Ridad.Remove(rida);
            NaitaKorvi();
        }
    }

    private void btnMuudaKogust_Click(object? sender, EventArgs e)
    {
        if (listOstukorv.SelectedItem is not OstukorviRida rida)
        {
            MessageBox.Show("Vali korvist rida.");
            return;
        }
        int indeks = listOstukorv.SelectedIndex;

        if (!Ostukorv.MuudaKogust(rida, (int)numKogus.Value))
        {
            MessageBox.Show($"Laos on ainult {rida.Toode.Kogus} tk.");
            return;
        }
        NaitaKorvi(indeks);
    }

    // ---------- Ostu kinnitamine ----------
    private void btnKinnita_Click(object? sender, EventArgs e)
    {
        if (Ostukorv.Ridad.Count == 0)
        {
            MessageBox.Show("Ostukorv on tühi.");
            return;
        }

        if (MessageBox.Show($"Kinnitan ostu kogusummas {Ostukorv.Kokku:F2} €?", "Kinnitus",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        var ost = Ostukorv.Kinnita(AktiivneKasutaja.Id, out string? viga);
        if (ost == null)
        {
            MessageBox.Show(viga ?? "Ostu ei õnnestunud salvestada.");
            Ostukorv.UuendaLaoseis();   // näita õiget laoseisu
            NaitaKorvi();
            return;
        }

        viimaneOst = ost;
        viimanePdf = null;
        NaitaKorvi();
        MessageBox.Show($"Ost nr {ost.Id} on salvestatud. Nüüd saab luua PDF-arve.");
    }

    // ---------- PDF ----------
    private void btnGenPDF_Click(object? sender, EventArgs e)
    {
        if (viimaneOst == null)
        {
            MessageBox.Show("Kinnita enne ost.");
            return;
        }

        try
        {
            viimanePdf = ArvePdf.Loo(viimaneOst, AktiivneKasutaja.Nimi);
            MessageBox.Show($"PDF on loodud:\n{viimanePdf}");
        }
        catch (Exception ex)
        {
            MessageBox.Show("PDF-i loomine ebaõnnestus: " + ex.Message);
        }
    }

    // ---------- E-post ----------
    private void btnSaadaEmail_Click(object? sender, EventArgs e)
    {
        if (viimanePdf == null || !File.Exists(viimanePdf))
        {
            MessageBox.Show("Genereeri enne PDF.");
            return;
        }

        string saaja = txtEmail.Text.Trim();
        if (!MailAddress.TryCreate(saaja, out _))
        {
            MessageBox.Show("Sisesta korrektne e-posti aadress.");
            return;
        }

        var seaded = EmailSeaded.Lae();   // parool pärineb krüpteeritud failist, mitte koodist
        if (seaded == null)
        {
            MessageBox.Show("E-posti seaded puuduvad. Administraator saab need sisestada nupust \"E-posti seaded\".");
            return;
        }

        try
        {
            // using vabastab manuse, muidu jääb PDF-fail lukku
            using var kiri = new MailMessage(seaded.Saatja, saaja, "Arve", "Arve on manuses.");
            kiri.Attachments.Add(new Attachment(viimanePdf));

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(seaded.Saatja, seaded.Parool),
                EnableSsl = true,
                Timeout = 20000
            };
            smtp.Send(kiri);

            MessageBox.Show("Arve saadetud!");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Viga: " + ex.Message);
        }
    }

    private void btnEmailSeaded_Click(object? sender, EventArgs e)
    {
        using var vorm = new EmailSeadedForm();
        vorm.ShowDialog();
    }
}

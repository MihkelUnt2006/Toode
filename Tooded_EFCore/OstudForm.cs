using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public partial class OstudForm : Form
{
    private readonly ToodedContext _db = new();
    private readonly DataGridView dataGridViewOstud = new();
    private readonly ListBox listRead = new();
    private readonly Label lblKokku = new();
    private readonly Button btnPdf = new();

    private List<Ost> _ostud = new();

    public OstudForm()
    {
        InitializeComponent();
        Text = AktiivneKasutaja.OnAdmin ? "Ostude ajalugu (kõik ostud)" : "Ostude ajalugu (minu ostud)";
        ClientSize = new Size(700, 480);

        dataGridViewOstud.SetBounds(12, 12, 676, 250);
        dataGridViewOstud.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewOstud.MultiSelect = false;
        dataGridViewOstud.ReadOnly = true;
        dataGridViewOstud.AllowUserToAddRows = false;
        dataGridViewOstud.SelectionChanged += (s, e) => NaitaRead();

        listRead.SetBounds(12, 272, 676, 150);

        lblKokku.SetBounds(12, 435, 450, 25);
        lblKokku.Font = new Font(Font, FontStyle.Bold);

        btnPdf.SetBounds(488, 430, 200, 35);
        btnPdf.Text = "Loo PDF-arve";
        btnPdf.Click += btnPdf_Click;

        Controls.AddRange(new Control[] { dataGridViewOstud, listRead, lblKokku, btnPdf });

        LaeOstud();
    }

    private void LaeOstud()
    {
        IQueryable<Ost> paring = _db.Ostud.Include(o => o.Read).Include(o => o.Kasutaja);

        // Tavakasutaja näeb ainult enda oste, administraator kõiki
        if (!AktiivneKasutaja.OnAdmin)
        {
            int minuId = AktiivneKasutaja.Id;
            paring = paring.Where(o => o.KasutajaId == minuId);
        }

        _ostud = paring.ToList().OrderByDescending(o => o.Kuupaev).ToList();

        dataGridViewOstud.DataSource = _ostud.Select(o => new
        {
            o.Id,
            Kuupaev = o.Kuupaev.ToString("dd.MM.yyyy HH:mm"),
            Klient = o.Kasutaja.Nimi,
            Kokku = o.Kokku.ToString("F2") + " €"
        }).ToList();

        dataGridViewOstud.Columns["Id"]!.HeaderText = "Ost nr";
        dataGridViewOstud.Columns["Kuupaev"]!.HeaderText = "Kuupäev";
        dataGridViewOstud.Columns["Klient"]!.Visible = AktiivneKasutaja.OnAdmin;

        NaitaRead();
    }

    private Ost? ValitudOst()
    {
        if (dataGridViewOstud.SelectedRows.Count == 0) return null;
        int id = (int)dataGridViewOstud.SelectedRows[0].Cells["Id"].Value;
        return _ostud.FirstOrDefault(o => o.Id == id);
    }

    private void NaitaRead()
    {
        listRead.Items.Clear();
        var ost = ValitudOst();
        if (ost == null)
        {
            lblKokku.Text = _ostud.Count == 0 ? "Oste pole." : "";
            return;
        }

        foreach (var rida in ost.Read.OrderBy(r => r.Id))
            listRead.Items.Add(rida);   // kuvab OstuRida.ToString()

        lblKokku.Text = $"Kokku: {ost.Kokku:F2} €";
    }

    private void btnPdf_Click(object? sender, EventArgs e)
    {
        var ost = ValitudOst();
        if (ost == null)
        {
            MessageBox.Show("Vali tabelist ost.");
            return;
        }

        try
        {
            string tee = ArvePdf.Loo(ost, ost.Kasutaja.Nimi);
            MessageBox.Show($"PDF on loodud:\n{tee}");
        }
        catch (Exception ex)
        {
            MessageBox.Show("PDF-i loomine ebaõnnestus: " + ex.Message);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _db.Dispose();
        base.OnFormClosed(e);
    }
}

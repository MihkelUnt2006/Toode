using System.Net.Mail;

namespace Tooded_EFCore;

public partial class EmailSeadedForm : Form
{
    private readonly TextBox txtSaatja = new(), txtParool = new();
    private readonly Label lblInfo = new();
    private readonly Button btnSalvesta = new();

    public EmailSeadedForm()
    {
        InitializeComponent();
        Text = "E-posti seaded";
        ClientSize = new Size(340, 230);

        txtSaatja.SetBounds(30, 30, 280, 27);
        txtSaatja.PlaceholderText = "Saatja Gmaili aadress";
        txtSaatja.Text = EmailSeaded.LaeSaatja();

        txtParool.SetBounds(30, 70, 280, 27);
        txtParool.PlaceholderText = "Rakenduse parool (16 märki)";
        txtParool.UseSystemPasswordChar = true;

        lblInfo.SetBounds(30, 105, 280, 60);
        lblInfo.Text = "Kasuta Google'i rakenduse parooli, mitte tavalist parooli. " +
                       "Parool salvestatakse krüpteeritult (Windows DPAPI).";

        btnSalvesta.SetBounds(30, 175, 280, 35);
        btnSalvesta.Text = "Salvesta";
        btnSalvesta.Click += btnSalvesta_Click;

        AcceptButton = btnSalvesta;
        Controls.AddRange(new Control[] { txtSaatja, txtParool, lblInfo, btnSalvesta });
    }

    private void btnSalvesta_Click(object? sender, EventArgs e)
    {
        if (!MailAddress.TryCreate(txtSaatja.Text.Trim(), out _))
        {
            MessageBox.Show("Sisesta korrektne e-posti aadress.");
            return;
        }
        if (string.IsNullOrWhiteSpace(txtParool.Text))
        {
            MessageBox.Show("Sisesta rakenduse parool.");
            return;
        }

        try
        {
            EmailSeaded.Salvesta(txtSaatja.Text, txtParool.Text);
            MessageBox.Show("Seaded salvestatud.");
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Viga: " + ex.Message);
        }
    }
}

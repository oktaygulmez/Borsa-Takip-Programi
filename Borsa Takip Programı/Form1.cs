using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Net;
using System.Data.SqlClient;


namespace Borsa_Takip_Programı
{
    public partial class Form1 : Form
    {
        public string html;
        public Uri url;
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                baglanti.Open();
                SqlCommand komut = new SqlCommand("UPDATE PotansiyelListesi Set AlisFiyati=" + textBox2.Text + ", SatisFiyati=" + textBox3.Text + " Where HisseAdi='" + label4.Text + "';", baglanti);
                komut.ExecuteNonQuery();               
                baglanti.Close();
            }
            catch
            {
                MessageBox.Show("Veriler Güncellenirken Bir Hata Oluştu");
            }
            potansiyeltabosubutonu.PerformClick();
            portföytablosubutonu.PerformClick();
        }
        SqlConnection baglanti = new SqlConnection("Server=DESKTOP-GRK9EOF\\SQLEXPRESS; Database=BorsaVeriTabanı;Integrated Security=true");
        SqlDataAdapter da;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                url = new Uri("http://bigpara.hurriyet.com.tr/borsa/hisse-senetleri/");
            }
            catch (UriFormatException)
            {
                if (MessageBox.Show("Hatalı Url", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                {

                }
            }
            catch (ArgumentException)
            {
                if (MessageBox.Show("Hatalı Url", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                {

                }
            }
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            try
            {
                html = client.DownloadString(url);
            }
            catch (WebException)
            {
                if (MessageBox.Show("Hatalı Url", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                {

                }
            }
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            try
            {
                bistlabel.Text = "Bist 100 : " + doc.DocumentNode.SelectSingleNode("//*[@id='content']/div[2]/div[3]/div[1]/div/div[2]/ul[1]/li[2]").InnerText;
                dolarlabel.Text = "Dolar : " + doc.DocumentNode.SelectSingleNode("//*[@id='content']/div[2]/div[3]/div[1]/div/div[2]/ul[2]/li[2]").InnerText;
                eurolabel.Text = "Euro : " + doc.DocumentNode.SelectSingleNode("//*[@id='content']/div[2]/div[3]/div[1]/div/div[2]/ul[3]/li[2]").InnerText;
            }
            catch (NullReferenceException)
            {
                if (MessageBox.Show("Hatalı xPath", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                {

                }
            } 
            //BİGPARA'DAN GÜNCEL VERİLERİ ÇEKME
            int toplamhissesayisi = 0;
            //8 WEB SAYFASI İÇİN 8'LİK DÖNGÜ
            for (int sayfa = 1; sayfa <= 8; sayfa++)
            {
                try
                {
                    url = new Uri("http://bigpara.hurriyet.com.tr/borsa/hisse-fiyatlari/" + sayfa + "/");
                }
                catch (UriFormatException)
                {
                    if (MessageBox.Show("Hatalı Url", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                    {

                    }
                }
                catch (ArgumentException)
                {
                    if (MessageBox.Show("Hatalı Url", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                    {

                    }
                }
                WebClient client2 = new WebClient();
                client2.Encoding = Encoding.UTF8;
                try
                {
                    html = client2.DownloadString(url);
                }
                catch (WebException)
                {
                    if (MessageBox.Show("Hatalı Url", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                    {

                    }
                }
                HtmlAgilityPack.HtmlDocument doc2 = new HtmlAgilityPack.HtmlDocument();
                doc2.LoadHtml(html);
                //45 SATIR İÇİN 45'LİK DÖNGÜ
                try
                {
                    string HisseAdi;
                    string GüncelFiyat;
                    for (int say = 1; say <= 45; say++)
                    {
                        toplamhissesayisi++;
                        try
                        {
                            HisseAdi = (doc2.DocumentNode.SelectSingleNode("//*[@id='content']/div[2]/div[6]/div/div/div[2]/ul[" + say + "]/li[1]").InnerText);
                            GüncelFiyat = ((doc2.DocumentNode.SelectSingleNode("//*[@id='content']/div[2]/div[6]/div/div/div[2]/ul[" + say + "]/li[2]").InnerText));
                            baglanti.Open();
                        if (Convert.ToDouble(GüncelFiyat) < 1000)
                        {
                            SqlCommand komut = new SqlCommand("UPDATE PotansiyelListesi Set GüncelFiyat=" + GüncelFiyat.ToString().Replace(',', '.') + " Where HisseAdi='" + HisseAdi + "';", baglanti);
                            komut.ExecuteNonQuery();
                        }
                            baglanti.Close();                           
                        }
                        catch
                        {
                            //Son sayfada 45 satır yok. Hata verirse hiçbir şey yapma!!}  
                        }
                    }
                }             
                catch (NullReferenceException)
                {
                    if (MessageBox.Show("Hatalı xPath", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                    {

                    }
                }
            }
            //PotansiyelListesi Tablosunu Listele
            potansiyeltabosubutonu.PerformClick();
            //Portföy Tablosunu Listele
            portföytablosubutonu.PerformClick();
            //Bağlantıyı Aç
            baglanti.Open();
            //Toplam Kar-Zarar ı Listele
            SqlCommand karzarar = new SqlCommand("Select Sum((pl.GüncelFiyat*p.Lot)-(p.AlisFiyati*p.Lot)) as ToplamKarZarar from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
            label14.Text = karzarar.ExecuteScalar().ToString();
            //Toplam Kar-Zarar ın yüzdesi
            SqlCommand karzararyüzde = new SqlCommand("Select Convert (Decimal(16,2),((Sum((pl.GüncelFiyat*p.Lot)-(p.AlisFiyati*p.Lot)))/(Sum(p.AlisFiyati*p.Lot)))*100) as ToplamKarZararyüzde from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
            label50.Text = "%" + karzararyüzde.ExecuteScalar().ToString();
            //ToplamAlisFiyati
            SqlCommand toplamalis = new SqlCommand("Select Convert (Decimal(16,2),Sum(AlisFiyati*Lot)) As ToplamAlis from Portföy;", baglanti);
            label20.Text = toplamalis.ExecuteScalar().ToString();
            //ToplamSatisFiyati
            SqlCommand toplamsatis = new SqlCommand("Select Convert (Decimal(16,2),Sum(pl.SatisFiyati*p.Lot)) As ToplamSatis from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
            label22.Text = toplamsatis.ExecuteScalar().ToString();
            //Beklenen Toplam Kar 
            SqlCommand karmiktari = new SqlCommand("Select Sum((pl.SatisFiyati*p.Lot)-(p.AlisFiyati*p.Lot)) As ToplamKar from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
            label17.Text = karmiktari.ExecuteScalar().ToString();
            //Beklenen Toplam Kar Yüzdesi
            SqlCommand karyüzdesi = new SqlCommand("Select Convert (Decimal(16,2),(Sum(pl.SatisFiyati*p.Lot)/Sum(p.AlisFiyati*p.Lot)-1)*100) As ToplamKarYüzdesi from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
            label18.Text = karyüzdesi.ExecuteScalar().ToString();
            //Beklenen Gün Ortalaması
            SqlCommand ortgun = new SqlCommand("SELECT Avg(DATEDIFF(day, AlisTarihi, GETDATE())) as tarihfark FROM Portföy;", baglanti);
            label46.Text = ortgun.ExecuteScalar().ToString();
            //Baglantıyı Kapat
            baglanti.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Timer Her Tetiklendiğinde Verileri Otomatik Olarak Günceller
            progressBar1.Value += 1;
            if(progressBar1.Value==100)
            {                
                this.button1.PerformClick();
                progressBar1.Value = 0;
            }           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
                //Program Başlangıcında Çalışır
            try
            {
                //Tarih ve Saati Yazdır
                tarihlabel.Text = "Tarih : " + DateTime.Now.ToLongDateString() + " Günü";
                //Potansiyel Listesi Tablosunu Yazdır
                potansiyeltabosubutonu.PerformClick();
                //Portföy Tablosunu Yazdır
                portföytablosubutonu.PerformClick();
                //Portföy Geçmişi Tablosunu Yazdır
                portföygeçmişibutonu.PerformClick();

                //Bağlantıyı Aç
                baglanti.Open();
                //Toplam Kar-Zarar ı Listele
                SqlCommand karzarar = new SqlCommand("Select Sum((pl.GüncelFiyat*p.Lot)-(p.AlisFiyati*p.Lot)) as ToplamKarZarar from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
                label14.Text = karzarar.ExecuteScalar().ToString();
                //Toplam Kar-Zarar ın yüzdesi
                SqlCommand karzararyüzde = new SqlCommand("Select Convert (Decimal(16,2),((Sum((pl.GüncelFiyat*p.Lot)-(p.AlisFiyati*p.Lot)))/(Sum(p.AlisFiyati*p.Lot)))*100) as ToplamKarZararyüzde from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
                label50.Text = "%" + karzararyüzde.ExecuteScalar().ToString();
                //ToplamAlisFiyati
                SqlCommand toplamalis = new SqlCommand("Select Convert (Decimal(16,2),Sum(AlisFiyati*Lot)) As ToplamAlis from Portföy;", baglanti);
                label20.Text = toplamalis.ExecuteScalar().ToString();
                //ToplamSatisFiyati
                SqlCommand toplamsatis = new SqlCommand("Select Convert (Decimal(16,2),Sum(pl.SatisFiyati*p.Lot)) As ToplamSatis from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
                label22.Text = toplamsatis.ExecuteScalar().ToString();
                //Beklenen Toplam Kar 
                SqlCommand karmiktari = new SqlCommand("Select Sum((pl.SatisFiyati*p.Lot)-(p.AlisFiyati*p.Lot)) As ToplamKar from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
                label17.Text = karmiktari.ExecuteScalar().ToString();
                //Beklenen Toplam Kar Yüzdesi
                SqlCommand karyüzdesi = new SqlCommand("Select Convert (Decimal(16,2),(Sum(pl.SatisFiyati*p.Lot)/Sum(p.AlisFiyati*p.Lot)-1)*100) As ToplamKarYüzdesi from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
                label18.Text = karyüzdesi.ExecuteScalar().ToString();
                //Beklenen Gün Ortalaması
                SqlCommand ortgun = new SqlCommand("SELECT Avg(DATEDIFF(day, AlisTarihi, GETDATE())) as tarihfark FROM Portföy;", baglanti);
                label46.Text = ortgun.ExecuteScalar().ToString();
                //Baglantıyı Kapat
                baglanti.Close();
            }
            catch { MessageBox.Show("Program Açılışı Sırasında Veriler Getirilirken Bir Hata Oluştu."); }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //Aranacak Hisse Kodu Textbox'ında Her Veri DEğişikliğinde Çalışır
            try
            {
                if (textBox1.Text != "")
                {
                    baglanti.Open();
                    da = new SqlDataAdapter("SELECT HisseAdi, GüncelFiyat, AlisFiyati,CONVERT(Decimal(6,0),(AlisFiyati/GüncelFiyat-1)*100) AS Yakinlik, SatisFiyati, CONVERT(Decimal(6,0),(SatisFiyati/AlisFiyati-1)*100) AS KarMarji FROM PotansiyelListesi Where HisseAdi Like '%" + textBox1.Text + "%' ORDER BY Yakinlik DESC;", baglanti);
                    DataTable tablo = new DataTable();
                    da.Fill(tablo);
                    dataGridView1.DataSource = tablo;
                    baglanti.Close();
                }
                else
                {
                    //Textboox Boş İse Hiçbir İşlem Yapma
                }
            }
            catch
            {
                MessageBox.Show("Aranacak Hisse Kodunu Doğru Girdiğinizden Emin Olun");
                textBox1.Clear();
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
                //DataGrid İçinde Seçili Satırın HisseAdi'nı Alır
            try
            {
                label4.Text = dataGridView1.CurrentRow.Cells["HisseAdi"].Value.ToString();
                label5.Text = dataGridView1.CurrentRow.Cells["GüncelFiyat"].Value.ToString().Replace(',', '.');
                textBox2.Text = dataGridView1.CurrentRow.Cells["AlisFiyati"].Value.ToString();
                textBox3.Text = dataGridView1.CurrentRow.Cells["SatisFiyati"].Value.ToString();
                label10.Text = Convert.ToString(Math.Round(Convert.ToDouble((Convert.ToDouble(textBox2.Text) / Convert.ToDouble(label5.Text) - 1 ) * 100),0));
                label11.Text = Convert.ToString(Math.Round(Convert.ToDouble((Convert.ToDouble(textBox3.Text) / Convert.ToDouble(textBox2.Text) - 1) * 100),0));
                linkLabel1.Text = "http://bigpara.hurriyet.com.tr/borsa/hisse-fiyatlari/" + dataGridView1.CurrentRow.Cells["HisseAdi"].Value.ToString().Trim() + "-detay/genel/1yil/";
            }
            catch
            {
                MessageBox.Show("Veriler Alınırken Bir Hata Oluştu");
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            //SatisFiyati textbox'ı her değiştiğinde Yakinlik ve Kar Marjı Değerleri Yeniden Hesaplanır
            try
            {              
                textBox3.Text = textBox3.Text.ToString().Replace(',', '.');
                label10.Text = Convert.ToString(Math.Round(Convert.ToDouble((Convert.ToDouble(textBox2.Text.Replace('.', ',')) / Convert.ToDouble(label5.Text) - 1) * 100), 0));
                label11.Text = Convert.ToString(Math.Round(Convert.ToDouble((Convert.ToDouble(textBox3.Text.Replace('.', ',')) / Convert.ToDouble(textBox2.Text.Replace('.', ',')) - 1) * 100), 0));
            }
            catch
            {
                label10.Text = "!!Hata";
                label11.Text = "!!Hata";
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //AlisFiyati textbox'ı her değiştiğinde Yakinlik ve Kar Marjı Değerleri Yeniden Hesaplanır
            try
            {
                textBox2.Text = textBox2.Text.ToString().Replace(',', '.');
                label10.Text = Convert.ToString(Math.Round(Convert.ToDouble((Convert.ToDouble(textBox2.Text.Replace('.', ',')) / Convert.ToDouble(label5.Text) - 1) * 100), 0));
                label11.Text = Convert.ToString(Math.Round(Convert.ToDouble((Convert.ToDouble(textBox3.Text.Replace('.', ',')) / Convert.ToDouble(textBox2.Text.Replace('.', ',')) - 1) * 100), 0));
            }
            catch
            {
                label10.Text = "!!Hata";
                label11.Text = "!!Hata";
            }
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //Potansiyel Listesi Tablosunun Satır Sayısı Değiştiğinde Alta Toplam Kayıt Miktarını Getirir
            label12.Text = "Listelenen Kayıt Miktarı : " + Convert.ToString(dataGridView1.RowCount-1);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //Potansiyel Listesinden Seçili Hissenin Ayrıntılı Bilgilerini İçeren Web Sayfasına Götürür
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //Programın Otomatik Olarak Verileri Yenilemesini Sağlar
            if (checkBox1.Checked == false)
            {
                timer1.Stop();
            }
            else
            {
                timer1.Start();
            }
        }

        private void dataGridView2_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //DataGrid İçinde Seçili Satırın HisseAdi'nı Alır
            try
            {
                linkLabel1.Text = "http://bigpara.hurriyet.com.tr/borsa/hisse-fiyatlari/" + dataGridView2.CurrentRow.Cells["HisseAdi"].Value.ToString().Trim() + "-detay/genel/1yil/";
            }
            catch
            {
                MessageBox.Show("Veriler Alınırken Bir Hata Oluştu");
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            potansiyeltabosubutonu.PerformClick();
        }

        private void potansiyeltabosubutonu_Click(object sender, EventArgs e)
        {
            //Y1 Potanbiyel Listesi Tablosunu Listeler
            if (checkBox2.Checked == false)
            {
                baglanti.Close();
                baglanti.Open();
                da = new SqlDataAdapter("SELECT HisseAdi, GüncelFiyat, AlisFiyati,CONVERT(Decimal(16,0),(AlisFiyati/GüncelFiyat-1)*100) AS Yakinlik, SatisFiyati, CONVERT(Decimal(16,0),(SatisFiyati/AlisFiyati-1)*100) AS KarMarji FROM PotansiyelListesi Where Grup='Y1' ORDER BY Yakinlik DESC;", baglanti);
                DataTable tablo = new DataTable();
                da.Fill(tablo);
                dataGridView1.DataSource = tablo;
                baglanti.Close();
            }
            else
            {
                baglanti.Close();
                baglanti.Open();
                da = new SqlDataAdapter("SELECT pl.HisseAdi, pl.GüncelFiyat, pl.AlisFiyati,CONVERT(Decimal(16,0),(pl.AlisFiyati/pl.GüncelFiyat-1)*100) AS Yakinlik, pl.SatisFiyati, CONVERT(Decimal(16,0),(pl.SatisFiyati/pl.AlisFiyati-1)*100) AS KarMarji FROM PotansiyelListesi pl, Portföy p Where pl.HisseAdi=p.HisseAdi Order BY Yakinlik DESC;", baglanti);
                DataTable tablo = new DataTable();
                da.Fill(tablo);
                dataGridView1.DataSource = tablo;
                baglanti.Close();
            }
        }

        private void portföytablosubutonu_Click(object sender, EventArgs e)
        {
            //Portföy Tablosunu Listeler
            da = new SqlDataAdapter("SELECT p.HisseAdi, pl.GüncelFiyat, CONVERT(Decimal(16,0),((pl.SatisFiyati/pl.GüncelFiyat-1)*100)) as Yakinlik,  p.AlisFiyati as LotMaliyet, ((p.Lot*pl.GüncelFiyat)-(p.Lot*p.AlisFiyati)) As KarZarar, pl.SatisFiyati, DATEDIFF(Day, AlisTarihi, GETDATE()) as Gün, CONVERT(Decimal(16,0),((pl.GüncelFiyat*lot)/(p.AlisFiyati*p.Lot)-1)*100) as Yüzde, pl.Grup FROM Portföy as p, PotansiyelListesi as pl Where p.HisseAdi = pl.HisseAdi Order By Yakinlik;", baglanti);
            DataTable tablo2 = new DataTable();
            da.Fill(tablo2);
            dataGridView2.DataSource = tablo2;
            baglanti.Close();
        }

        private void dataGridView2_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //Portföy Tablosunun Satır Sayısı Değiştiğinde Alta Toplam Kayıt Miktarını Getirir
            label23.Text = "Listelenen Kayıt Miktarı : " + Convert.ToString(dataGridView2.RowCount - 1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //PortföyGeçmişi TAblosunu Listeler
            baglanti.Open();
            da = new SqlDataAdapter("Select HisseAdi, AlisTarihi, AlisFiyati, Lot, (AlisFiyati*Lot) as AlışMaliyeti, SatisTarihi, SatisFiyati, (SatisFiyati*Lot) as SatışMaliyeti, DATEDIFF(Day, AlisTarihi, SatisTarihi) as Gün, (SatisFiyati-AlisFiyati)*Lot as KarZarar, CONVERT(Decimal(16,0),((SatisFiyati/AlisFiyati-1)*100)) as Yüzde from PortföyGeçmişi ORDER BY SatisTarihi ASC;", baglanti);
            DataTable tablo = new DataTable();
            da.Fill(tablo);
            dataGridView3.DataSource = tablo;         
            //Toplam Kar-Zarar ı Listele         
            SqlCommand karzarar = new SqlCommand("SELECT Sum((SatisFiyati*Lot) - (AlisFiyati*Lot)) AS KarZarar FROM PortföyGeçmişi;", baglanti);
            label32.Text = "Toplam Kar-Zarar : " + karzarar.ExecuteScalar().ToString();
            //
            SqlCommand toplamalis = new SqlCommand("SELECT Sum(AlisFiyati*Lot) AS ToplamAlis FROM PortföyGeçmişi;", baglanti);
            label33.Text = "Toplam Alış Maliyeti :  " + toplamalis.ExecuteScalar().ToString();
            //
            SqlCommand toplamsatis = new SqlCommand("SELECT Sum(SatisFiyati*Lot) AS ToplamSatis FROM PortföyGeçmişi;", baglanti);
            label34.Text = "Toplam Satış Maliyeti :  " + toplamsatis.ExecuteScalar().ToString();
            baglanti.Close();


        }

        private void button4_Click(object sender, EventArgs e)
        {
            baglanti.Open();
            string sorgu = "Insert Into PortföyGeçmişi(HisseAdi,AlisTarihi,AlisFiyati,Lot,SatisTarihi,SatisFiyati) values(@hisseadi,@at,@af,@lot,@st,@sf)";
            SqlCommand komut = new SqlCommand(sorgu, baglanti);
            komut.Parameters.AddWithValue("@hisseadi", textBox4.Text);
            komut.Parameters.AddWithValue("@at", dateTimePicker1.Value);
            komut.Parameters.AddWithValue("@af", textBox5.Text);
            komut.Parameters.AddWithValue("@lot", textBox6.Text);
            komut.Parameters.AddWithValue("@st", dateTimePicker2.Value);
            komut.Parameters.AddWithValue("@sf", textBox8.Text);
            komut.ExecuteNonQuery();
            baglanti.Close();
            portföygeçmişibutonu.PerformClick();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            portföygeçmişibutonu.PerformClick();
          /*  dateTimePicker1.Value = Convert.ToDateTime("17.01.2019");
            dateTimePicker2.Value = Convert.ToDateTime("17.01.2019");*/
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            try
            {
                label31.Text = Math.Round(Convert.ToDouble((Convert.ToDouble(textBox5.Text) * Convert.ToInt16(textBox6.Text))),2).ToString();
            }
            catch { }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            textBox5.Text = textBox5.Text.ToString().Replace(',', '.');
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            textBox8.Text = textBox8.Text.ToString().Replace(',', '.');
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            baglanti.Open();

            //Değişkenlerin Tanımlanması
            TimeSpan ts = (Convert.ToDateTime(DateTime.Now.ToShortDateString()) - Convert.ToDateTime("07.09.2016"));
            int gün = Convert.ToInt16(ts.Days);
            double kesinkarzarar, güncelkarzarar;

            //Borsadaki Toplam Gün Sayısı Yazdırma
            label35.Text += ts.Days.ToString();

            //Kesinleşmiş Toplam Kar
            SqlCommand karzarar = new SqlCommand("SELECT Sum((SatisFiyati*Lot) - (AlisFiyati*Lot)) AS KarZarar FROM PortföyGeçmişi;", baglanti);
            kesinkarzarar = Convert.ToInt16(karzarar.ExecuteScalar());
            label36.Text += kesinkarzarar.ToString();

            //Güncel Kar Zarar
            SqlCommand gkarzarar = new SqlCommand("Select Sum((pl.GüncelFiyat*p.Lot)-(p.AlisFiyati*p.Lot)) as ToplamKarZarar from Portföy as p, PotansiyelListesi as pl where p.HisseAdi=pl.HisseAdi;", baglanti);
            güncelkarzarar= Convert.ToDouble(gkarzarar.ExecuteScalar());
            label37.Text += güncelkarzarar.ToString();

            //Genel Kar Zarar
            label38.Text += (kesinkarzarar + güncelkarzarar).ToString();

            //Günlük - Aylık - Yıllık Kazanç
            label47.Text += Math.Round(Convert.ToDouble(Convert.ToDouble(kesinkarzarar) / gün), 2).ToString();
            label48.Text += Math.Round(Convert.ToDouble((Convert.ToDouble(kesinkarzarar) / gün)*30), 2).ToString();
            label49.Text += Math.Round(Convert.ToDouble((Convert.ToDouble(kesinkarzarar) / gün)*360), 2).ToString();

            //Toplam İşlem Sayısı
            SqlCommand tislem = new SqlCommand("SELECT count(*) FROM PortföyGeçmişi;", baglanti);
            label39.Text += tislem.ExecuteScalar().ToString();

            //Karlı İşlem Sayısı
            SqlCommand Kislem = new SqlCommand("SELECT count(*) FROM PortföyGeçmişi WHERE SatisFiyati>AlisFiyati;", baglanti);
            label40.Text += Kislem.ExecuteScalar().ToString();

            //Notr İşlem Sayısı
            SqlCommand Nislem = new SqlCommand("SELECT count(*) FROM PortföyGeçmişi WHERE SatisFiyati=AlisFiyati;", baglanti);
            label41.Text += Nislem.ExecuteScalar().ToString();

            //Zararına İşlem Sayısı
            SqlCommand Zislem = new SqlCommand("SELECT count(*) FROM PortföyGeçmişi WHERE SatisFiyati<AlisFiyati;", baglanti);
            label42.Text += Zislem.ExecuteScalar().ToString();

            //Ortalama Gün Sayısı
            SqlCommand Ogün = new SqlCommand("SELECT avg(DATEDIFF(day, AlisTarihi, SatisTarihi)) as tarihfark FROM PortföyGeçmişi;", baglanti);
            label43.Text += Ogün.ExecuteScalar().ToString();

            //Kazanç Yüzdesi
            SqlCommand Oyüzde = new SqlCommand("SELECT CONVERT(Decimal(16,2),(((SUM(SatisFiyati*Lot)/(SUM(AlisFiyati*Lot))-1)*100))) as yuzde FROM PortföyGeçmişi;", baglanti);
            label44.Text += Oyüzde.ExecuteScalar().ToString();

            //Kazanç Yüzdesi Aylık

            label53.Text += Math.Round((Convert.ToDouble(Oyüzde.ExecuteScalar()) / Convert.ToDouble(Ogün.ExecuteScalar()) * 30),2);

            //Kazanç Yüzdesi Yıllık

            label54.Text += Math.Round((Convert.ToDouble(Oyüzde.ExecuteScalar()) / Convert.ToDouble(Ogün.ExecuteScalar()) * 360),2);

            baglanti.Close();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if((label4.Text == ".")||(label4.Text == ""))
            {
                MessageBox.Show("Silmek İstediğiniz Hisseyi Seçin");
            }
            else
            {
                DialogResult Soru;

                Soru = MessageBox.Show(label4.Text + " Hissesi portföyden çıkartılacak. Onaylıyor Musun?", "Uyarı",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (Soru == DialogResult.Yes)
                {
                    try
                    {
                          baglanti.Open();
                          SqlCommand komut = new SqlCommand("Delete from Portföy Where HisseAdi='" + label4.Text + "';", baglanti);
                          komut.ExecuteNonQuery();
                          baglanti.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Veri Silinirken Bir Hata Oluştu");
                    }
                    potansiyeltabosubutonu.PerformClick();
                    portföytablosubutonu.PerformClick();
                }         
            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult Soru;

            Soru = MessageBox.Show(label4.Text + " Hissesi " + textBox2.Text + " Alış Fiyatı ile ve " + textBox7.Text + " Lot Sayısı ile " + dateTimePicker3.Value + " Tarihinde Portföye Eklenecek. Onaylıyor Musun? ", "Uyarı",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (Soru == DialogResult.Yes)
            {
                try
                {
                    baglanti.Open();
                    SqlCommand komut = new SqlCommand("INSERT INTO Portföy(HisseAdi, AlisFiyati, Lot, AlisTarihi) Values('" + label4.Text + "'," + textBox2.Text + "," + textBox7.Text + ",'" + dateTimePicker3.Value + "');", baglanti);

                    // SqlCommand komut = new SqlCommand("UPDATE PotansiyelListesi Set AlisFiyati=" + textBox2.Text + ", SatisFiyati=" + textBox3.Text + " Where HisseAdi='" + label4.Text + "';", baglanti);
                    komut.ExecuteNonQuery();
                    baglanti.Close();
                }
                catch
                {
                    MessageBox.Show("Veriler Eklenirken Bir Hata Oluştu");
                }
                potansiyeltabosubutonu.PerformClick();
                portföytablosubutonu.PerformClick();
            }
            else
            {
                
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
           
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Y2 Potanbiyel Listesi Tablosunu Listeler
            if (checkBox2.Checked == false)
            {
                baglanti.Close();
                baglanti.Open();
                da = new SqlDataAdapter("SELECT HisseAdi, GüncelFiyat, AlisFiyati,CONVERT(Decimal(16,0),(AlisFiyati/GüncelFiyat-1)*100) AS Yakinlik, SatisFiyati, CONVERT(Decimal(16,0),(SatisFiyati/AlisFiyati-1)*100) AS KarMarji FROM PotansiyelListesi Where Grup='Y2' ORDER BY Yakinlik DESC;", baglanti);
                DataTable tablo = new DataTable();
                da.Fill(tablo);
                dataGridView1.DataSource = tablo;
                baglanti.Close();
            }
            else
            {
                baglanti.Close();
                baglanti.Open();
                da = new SqlDataAdapter("SELECT pl.HisseAdi, pl.GüncelFiyat, pl.AlisFiyati,CONVERT(Decimal(16,0),(pl.AlisFiyati/pl.GüncelFiyat-1)*100) AS Yakinlik, pl.SatisFiyati, CONVERT(Decimal(16,0),(pl.SatisFiyati/pl.AlisFiyati-1)*100) AS KarMarji FROM PotansiyelListesi pl, Portföy p Where pl.HisseAdi=p.HisseAdi Order BY Yakinlik DESC;", baglanti);
                DataTable tablo = new DataTable();
                da.Fill(tablo);
                dataGridView1.DataSource = tablo;
                baglanti.Close();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //A1 Potanbiyel Listesi Tablosunu Listeler
            if (checkBox2.Checked == false)
            {
                baglanti.Close();
                baglanti.Open();
                da = new SqlDataAdapter("SELECT HisseAdi, GüncelFiyat, AlisFiyati,CONVERT(Decimal(16,0),(AlisFiyati/GüncelFiyat-1)*100) AS Yakinlik, SatisFiyati, CONVERT(Decimal(16,0),(SatisFiyati/AlisFiyati-1)*100) AS KarMarji FROM PotansiyelListesi Where Grup='A1' ORDER BY Yakinlik DESC;", baglanti);
                DataTable tablo = new DataTable();
                da.Fill(tablo);
                dataGridView1.DataSource = tablo;
                baglanti.Close();
            }
            else
            {
                baglanti.Close();
                baglanti.Open();
                da = new SqlDataAdapter("SELECT pl.HisseAdi, pl.GüncelFiyat, pl.AlisFiyati,CONVERT(Decimal(16,0),(pl.AlisFiyati/pl.GüncelFiyat-1)*100) AS Yakinlik, pl.SatisFiyati, CONVERT(Decimal(16,0),(pl.SatisFiyati/pl.AlisFiyati-1)*100) AS KarMarji FROM PotansiyelListesi pl, Portföy p Where pl.HisseAdi=p.HisseAdi Order BY Yakinlik DESC;", baglanti);
                DataTable tablo = new DataTable();
                da.Fill(tablo);
                dataGridView1.DataSource = tablo;
                baglanti.Close();
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //A2 Potanbiyel Listesi Tablosunu Listeler
            if (checkBox2.Checked == false)
            {
                baglanti.Close();
                baglanti.Open();
                da = new SqlDataAdapter("SELECT HisseAdi, GüncelFiyat, AlisFiyati,CONVERT(Decimal(16,0),(AlisFiyati/GüncelFiyat-1)*100) AS Yakinlik, SatisFiyati, CONVERT(Decimal(16,0),(SatisFiyati/AlisFiyati-1)*100) AS KarMarji FROM PotansiyelListesi Where Grup='A2' ORDER BY Yakinlik DESC;", baglanti);
                DataTable tablo = new DataTable();
                da.Fill(tablo);
                dataGridView1.DataSource = tablo;
                baglanti.Close();
            }
            else
            {
                baglanti.Close();
                baglanti.Open();
                da = new SqlDataAdapter("SELECT pl.HisseAdi, pl.GüncelFiyat, pl.AlisFiyati,CONVERT(Decimal(16,0),(pl.AlisFiyati/pl.GüncelFiyat-1)*100) AS Yakinlik, pl.SatisFiyati, CONVERT(Decimal(16,0),(pl.SatisFiyati/pl.AlisFiyati-1)*100) AS KarMarji FROM PotansiyelListesi pl, Portföy p Where pl.HisseAdi=p.HisseAdi Order BY Yakinlik DESC;", baglanti);
                DataTable tablo = new DataTable();
                da.Fill(tablo);
                dataGridView1.DataSource = tablo;
                baglanti.Close();
            }
        }
    }
}

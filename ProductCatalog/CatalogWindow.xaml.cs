using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing;

namespace ProductCatalog
{
    /// <summary>
    /// Interaction logic for CatalogWindow.xaml
    /// </summary>
    public partial class CatalogWindow : Window
    {
        

        public CatalogWindow()
        {
           
            InitializeComponent();

            // Ladataan datagrid tietokannasta kun ikkuna avataan.

            updateGrid();   // Methodi löytyy sivun loppupäästä. päätin tehdä sen vasta lopuksi, kun totesin että se säästää varmaan n. 100-200 riviä kun ei tarvii kopioida tota samaa joka paikkaan.

        }

        private void btn_addUser_Click(object sender, RoutedEventArgs e)            // Add User buttonin logiikka
        {
            if (tb_password1.Password == tb_password2.Password && tb_password1.Password != "" && tb_password2.Password != "" && tb_username.Text != "") // Tarkistetaan että kentät on täytetty oikein.
            {
                
                SHA512 sha512 = new SHA512Managed();                    // SHA512 cryptaus pitäisi riittää.
                UTF8Encoding utf8 = new UTF8Encoding();                 // SHA512 ottaa byte arrayn joten string täytyy muuttaa ennen kryptausta.
                String password = BitConverter.ToString(sha512.ComputeHash(utf8.GetBytes(tb_password1.Password)));  // Aika monimutkainen rivi joka muuttaa string byte arrayksi ja kryptaa sen 


                string dbConnectionString = @"Data Source=database.db;Version=3;";               // Määritellään sqlite tietokannan nimeksi database.db ja versioksi 3.
                SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);           // Tehdään yhteys olio sqliteCon jota voidaan käyttää myöhemmin.
                try
                {
                    sqliteCon.Open();                                                            // Avataan sqlite yhteys
                    string Query = "INSERT INTO users (username, password) VALUES('" + this.tb_username.Text + "','" + password + "') ";    // Syötetään uuden käyttäjän tiedot tietokantaan. Huom. kryptattu salasana.
                    SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);           // Tehdään SQlite komento query ja sqlitecon argumenteilla.
                    createCommand.ExecuteNonQuery();                                             // Suoritetaan query.
                    sqliteCon.Close();                                                           // Suljetaan db yhteys.   

                    tb_username.Text = "";                                                       // Tyhjennetään kentät ettei vahingossa upita samoja tietoja uudestaan
                    tb_password1.Password = "";
                    tb_password2.Password = "";
                    MessageBox.Show("User added successfully!");                                 // Ilmoitus käyttäjälle onnistumisesta.
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);                                                 // Tai epäonnistumisesta.
                }               
            }
            else
            {
                MessageBox.Show("Oops, check the fields.");                                      // Kaikki kentät pitää täyttää, ja vieläpä oikein.
            }
        }

        private void btn_switch_user_Click(object sender, RoutedEventArgs e)                    // Vaihda käyttäjää nappi.
        {
            MainWindow main = new MainWindow();                                                 // Avataan uusi ikkuna..oudolta nimeltä MainWindow vaikka kyse onkin loginista.
            main.Show();
            this.Close();                                                                       // Suljetaan tämä window.
        }

                                                                                                
        private void btn_quit_Click(object sender, RoutedEventArgs e)                           // Exit buttonin koodit
        {
            this.Close();                                                                       // Eipä ollu kauheesti. Sulkee tämän ikkunan ja samalla koko ohjelman.
        }


        private void btn_add_prod_Click(object sender, RoutedEventArgs e)                       // Tuotteen lisäys nappulan koodit
        {
            
            Random rnd = new Random();                                                                          // Random num generaattori
            string random_barcode = rnd.Next(10000000, 99999999).ToString(); // Tehdään barcode random numerolla
            
            // Luodaan barcode 
           Barcode128 code128 = new Barcode128();
            code128.CodeType = Barcode.CODE128_UCC;
            code128.Code = random_barcode;
            
            // Generoidaan barcode png image /barcodes hakemistoon
            System.Drawing.Bitmap barcode = new System.Drawing.Bitmap(code128.CreateDrawingImage(System.Drawing.Color.Black, System.Drawing.Color.White));  // Mustaa valkoiselle
            barcode.Save("barcodes/"+random_barcode+".png");    // Tallennetaan /barcodes hakemistoon
     
            // Ihan samat SQL Connectionit kun aina.
            string dbConnectionString = @"Data Source=database.db;Version=3;";
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);
            if (tb_name_update.Text != "" && tb_brand_update.Text != "" && tb_qty_update.Text != "")    // Jos nimi, brändi ja määrä kentät EIvät ole tyhjiä niin eteenpäin. (Muuten tulee vähän huono tuotemerkintä.)
            {
                try
                {
                    sqliteCon.Open();
                    string Query = "INSERT INTO products (name, brand, price, qty, desc, barcode) VALUES('" + this.tb_name_update.Text + "','" + this.tb_brand_update.Text + "','" + this.tb_price_update.Text + "','" + this.tb_qty_update.Text + "','" + this.tb_desc_update.Text + "','"+random_barcode+"') ";

                    SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);
                    createCommand.ExecuteNonQuery();
                    
                    sqliteCon.Close();                                                  // Sinne vaan pusketaan tiedot taas tietokantaan ja näytetään käyttäjälle ilmoitus ja suljetaan db-yhteys.
                    MessageBox.Show("Product added successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                                                                                        // Kun tiedot on uploadattu niin päivitetäänpä samalla vaivalla datagrid niin ei pysyy käyttäjäkin ajan tasalla.
                updateGrid();

            }
        }

        private void btn_remove_Click(object sender, RoutedEventArgs e)                                 // NO TAAS IHAN SAMAT JUTUT, ERI SQL QUERY. NYT DELETOIDAAN.
        {
            string dbConnectionString = @"Data Source=database.db;Version=3;";
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);
            if (tb_name_update.Text != "" && tb_brand_update.Text != "" && tb_qty_update.Text != "")
            {
                try
                {
                    sqliteCon.Open();
                    string Query = "DELETE FROM products WHERE prod_num='" + this.prod_num_lbl.Content + "' ";
                    SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);
                    createCommand.ExecuteNonQuery();

                    sqliteCon.Close();
                    MessageBox.Show("Product removed successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

              /*  if (File.Exists(@"barcode/"+ this.prod_num_lbl.Content +".png")) {        / nääh, tarvittais tietenki barcoden numero eikä prod_num joten unohdetaan tällä erää tämä,
                    File.Delete(@"barcode/" + this.prod_num_lbl.Content + ".png");          / kun ei nyt just mahdu uusia labeleita sun muuta. maybe later..
                } */
                                                                                                    // JA SITTE PÄIVITETÄÄN TAAS SE GRID NIIN NÄKEE KÄYTTÄJÄKIN MUUTOKSET
                updateGrid();
            }
        }

        private void btn_update_Click(object sender, RoutedEventArgs e)                                          // UPDATE SQL QUERY, EI MUUTA EROA
        {

            string dbConnectionString = @"Data Source=database.db;Version=3;";
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);
            if (tb_name_update.Text != "" && tb_brand_update.Text != "" && tb_qty_update.Text != "")
            {
                try
                {
                    sqliteCon.Open();
                    string Query = "UPDATE products SET name='"+this.tb_name_update.Text+"', brand='"+this.tb_brand_update.Text+"', price='"+this.tb_price_update.Text+"', qty='"+this.tb_qty_update.Text+"', desc='"+this.tb_desc_update.Text+"' WHERE prod_num='" + this.prod_num_lbl.Content + "' ";
                    SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);
                    createCommand.ExecuteNonQuery();

                    sqliteCon.Close();
                    MessageBox.Show("Product updated successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                updateGrid();                          // JA SITTE PÄIVITETÄÄN TAAS SE GRIDI

            }

        }

        private void btn_search_Click(object sender, RoutedEventArgs e)
        {
            string cat = category_combobox.Text;                                                // otetaan comboboxin teksti
            string dbConnectionString = @"Data Source=database.db;Version=3;";
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);
            try
            {
                sqliteCon.Open();
                string Query = "SELECT * FROM products WHERE "+cat+"='"+this.tb_cat.Text+"' ";      // käytetään comboboxin tekstiä queryssä
                SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);
                createCommand.ExecuteNonQuery();

                SQLiteDataAdapter dataAdp = new SQLiteDataAdapter(createCommand);
                DataTable dt = new DataTable("products"); 
                dataAdp.Fill(dt);
                data_grid2.ItemsSource = dt.DefaultView;
                dataAdp.Update(dt);

                sqliteCon.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        
        private void btn_pdf_Click(object sender, RoutedEventArgs e)                    // PDF buttoni. Tässä onki jo onneks vähän eroa. Paitsi tässä alussa kun yhdistetään databaseen.
        {
            string dbConnectionString = @"Data Source=database.db;Version=3;";
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);

            try
            {
                sqliteCon.Open();
                string Query = "SELECT * FROM products ";
                SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);
                createCommand.ExecuteNonQuery();

                SQLiteDataAdapter dataAdp = new SQLiteDataAdapter(createCommand);           // Tää on tätä samaa touhua. Tässä vaiheessa alkaa mietityttämään että oisko EHKÄ pitäny tehä joku methodi ettei tarttis samaa kommentoida kokoajan samoja juttuja
                DataTable dt = new DataTable("products"); 
                dataAdp.Fill(dt);
                dataAdp.Update(dt);

                iTextSharp.text.Font fnt = FontFactory.GetFont("Times New Roman", 12);

                Random rand = new Random();
                string random_num = rand.Next(1, 100).ToString();
                string date = (DateTime.Today.ToString("dd_MM_yyyy"));
                string pdf_filename = "Catalog_"+date+"_"+random_num+"";


                Document doc = new Document(iTextSharp.text.PageSize.LETTER, 10, 10, 42, 35);                   // Määritellään dokumentti ja dokumentille koko. Tää pitäis olla about A4 mikäli mua ei oo huijattu internetissä.
                PdfWriter wri = PdfWriter.GetInstance(doc, new FileStream(""+pdf_filename+".pdf", FileMode.Create));     // Tehään pdfwriter olio wri filestream ja tiedolle nimi "Catalog.pdf". Tallentuu nyt suoraan juureen koska ei annettu mitään muuta osoitetta tiedostolle.
                doc.Open();                                                                                     // Avataan tiedosto että voidaan alkaa kirjottelemaan sinne.
                PdfPTable PdfTable = new PdfPTable(dt.Columns.Count);                                           // PDF taulu jossa tietokannan mukainen määrä columneja.
                PdfPCell PdfPCell = null;                                                                       

                                                                                                                // Ihan perus for loopilla rullataan rivit ja columnit läpi.
                for (int rows = 0; rows < dt.Rows.Count; rows++)
                {
                    if (rows == 0)
                    {
                        for (int column = 0; column < dt.Columns.Count; column++)
                        {
                            PdfPCell = new PdfPCell(new Phrase(new Chunk(dt.Columns[column].ColumnName.ToString(), fnt)));      // Kirjotellaan tauluun otsikot times new romanilla.
                            PdfTable.AddCell(PdfPCell);                                                                         // Sitte lisätään otsikkosolut paikoilleen.
                        }
                    }
                    for (int column = 0; column < dt.Columns.Count; column++)                                                   // Tässä laitetaan sitten tiedot otsikoiden alle.
                    {
                        PdfPCell = new PdfPCell(new Phrase(new Chunk(dt.Rows[rows][column].ToString(), fnt)));                  // Rivien kirjottelua.    
                        PdfTable.AddCell(PdfPCell);                                                                             // Lisätään taas solut paikoilleen.
                    } 
                }

                doc.Add(PdfTable); // Lämätään tää hieno table mikä tässä rakennettiin niin siihen pdf dokumenttiin.
                doc.Close();    // Suljetaan tiedosto.
                MessageBox.Show("" + pdf_filename + ".pdf created!");            // JEs! Onnistu. Siinä on PDF mustaa valkosella.
                System.Diagnostics.Process.Start(""+pdf_filename+".pdf");  // Avataan tiedosto, koska kukaan ei muuten tajuu minne se on tallentunu. Paitsi että se tallentuu samaan hakemistoon. Mut ei sieltä tajuu kukaan kattoa kuitenkaan.
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // Errorit otetaan kiinni ja näytetään käyttäjälle. Yleinen ongelma minkä huomasin on se, että tiedosto on käytössä (esim acrobat readrin) ja eli sulje se.
            }
        }

        private void btn_excel_Click(object sender, RoutedEventArgs e)
        {
            string dbConnectionString = @"Data Source=database.db;Version=3;";
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);
            sqliteCon.Open();
            string Query = "SELECT * FROM products ";
            SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);
            createCommand.ExecuteNonQuery();

            SQLiteDataAdapter dataAdp = new SQLiteDataAdapter(createCommand);
            DataTable dt = new DataTable("products");
            dataAdp.Fill(dt);
            dataAdp.Update(dt);
            
            // Tehdään uusi tiedosto. Päätin nyt käyttää tota päivämäärä + randomnumero yhdistelmää, niin ei overwritee ainakaan joka kerta. Ei ehkä ihan paras ratkaisu, mutta aika simppeli.
            Random rand = new Random();
            string random_num = rand.Next(1, 100).ToString();
            string date = (DateTime.Today.ToString("dd_MM_yyyy"));
            string csv_filename = "Catalog_" + date + "_" + random_num + "";    // Tää on aika puinen ratkasu, mutta kyllä sillä nyt yhen päivän aikana aika monta pdf:ää (todennäkösesti) saa ennen kun menee päällekkäin
                                                                                // Ois tietenki voinu myös tehä jonku tsekin on saman niminen tiedosto jo olemassa ja lisätä vaikka +1 edellisen perään, mutta ei päästetä käyttäjää nyt liian helpolla.
            int cols;
            StreamWriter wr = new StreamWriter("csv/"+csv_filename+".csv");         // Avataan uus tiedosto
            cols = dt.Columns.Count;
            for (int i = 0; i < cols; i++)
            {
                wr.Write(dt.Columns[i].ColumnName.ToString().ToUpper());
                if (cols - i == 1) { break; } // Ei haluta pilkkua enää rivin viimeisen cellin jälkeen, vaan uutta riviä vaan
                wr.Write(",");
                
            }
            wr.WriteLine();

            // Kirjotetaan CSV rivit tiedostoon
            for (int i = 0; i < (dt.Rows.Count); i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (dt.Rows[i][j] != null)
                    {
                        wr.Write(dt.Rows[i][j].ToString());
                        if (cols - j == 1) { break; }
                        wr.Write(",");
                    }
                    else 
                    {
                        wr.Write(",");                    
                    }
                }
                wr.WriteLine();
            }
            wr.Close();
            MessageBox.Show("CSV file created to /csv folder!");    // ilmoitetaan käyttäjälle että csv tiedosto tehtiin onnistuneesti /csv hakemistoon
        }

        private void sell_btn_Click(object sender, RoutedEventArgs e)
        {
            // Tässä on tämmönen sell button joka vähentää updatella valitun tuotteen quantityyn sen määrän mikä textboxiin on laitettu

            // Ihan samanlainen sql query kun noi kaikki muutkin, eli eiköhän ne nyt tullu jo selviks
            string dbConnectionString = @"Data Source=database.db;Version=3;";
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);

            try
            {
                sqliteCon.Open();
                string Query = "UPDATE products SET `qty` = `qty` - " + this.tb_sell.Text + "  WHERE prod_num='" + this.prod_num_lbl.Content + "' ";
                SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);
                createCommand.ExecuteNonQuery();

                sqliteCon.Close();
                MessageBox.Show("Sold " + this.tb_sell.Text + " items successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            updateGrid();
        }

        private void buy_btn_Click(object sender, RoutedEventArgs e)
        {
            // Tässä on tämmönen buy button joka lisää updatella valitun tuotteen quantityyn sen määrän mikä textboxiin on laitettu

            string dbConnectionString = @"Data Source=database.db;Version=3;";
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);
    
                try
                {
                    sqliteCon.Open();
                    string Query = "UPDATE products SET `qty` = `qty` + " + this.tb_buy.Text + "  WHERE prod_num='" + this.prod_num_lbl.Content + "' ";
                    SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);
                    createCommand.ExecuteNonQuery();

                    sqliteCon.Close();
                    MessageBox.Show("Bought "+this.tb_buy.Text+" items successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            updateGrid();

        }

        // Tässä updateGrid method jota on käytetty varmaan joka kohdassa. Päivittää datagridin kutsuttaessa

        private void updateGrid() {

            string dbConnectionString = @"Data Source=database.db;Version=3;";              // Määritellään sqlite tietokannan nimeksi database.db ja versioksi 3.
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);          // Tehdään yhteys olio sqliteCon jota voidaan käyttää myöhemmin.

            try
            {
                sqliteCon.Open();                                                           // Avataan tietokantayhteys.
                string Query = "SELECT * FROM products ";                                   // Tehdään Query joka tulostaa kaiken tietokannasta.
                SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);          // Tehdään SQlite komento query ja sqlitecon argumenteilla.
                createCommand.ExecuteNonQuery();                                            // Käynnistetään äsken tehty komento.

                SQLiteDataAdapter dataAdp = new SQLiteDataAdapter(createCommand);           // Tehdään sqlite adapteri olio dataAdp
                DataTable dt = new DataTable("products");                                   // Tehdään datataulu-olio johon käytetään tietokannan 'products' taulua
                dataAdp.Fill(dt);                                                           // Täytetään data-adapteri products taululla.
                data_grid1.ItemsSource = dt.DefaultView;                                    // Täytetään datagrid
                dataAdp.Update(dt);                                                         // Päivitetään data-adapteri

                sqliteCon.Close();                                                          // Suljetaan tietokantayhteys.
            }
            catch (Exception ex)                                                            // Napataan virheet.
            {
                MessageBox.Show(ex.Message);                                                // Ja tulostetaan messageboxiin.
            }

        
        }
        
    }
}



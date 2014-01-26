using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Security.Cryptography;


namespace ProductCatalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string dbConnectionString = @"Data Source=database.db;Version=3;";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            // Tehdään 512 bittinen SHA2 cryptaus salasanalle. Ongelma on se, että ComputeHash() ottaa byte arrayn ja textbox on string joten täytyy muuttaa se utf8 encodella.
            SHA512 sha512 = new SHA512Managed();
            UTF8Encoding utf8 = new UTF8Encoding();
            String password = BitConverter.ToString(sha512.ComputeHash(utf8.GetBytes(tb_password.Password)));

            // SQL query
            SQLiteConnection sqliteCon = new SQLiteConnection(dbConnectionString);
            try
            {
                sqliteCon.Open();
                string Query = "SELECT * FROM users WHERE username='" + this.tb_username.Text + "' AND password='" + password + "' ";
                SQLiteCommand createCommand = new SQLiteCommand(Query, sqliteCon);
                createCommand.ExecuteNonQuery();
                
                SQLiteDataReader dr = createCommand.ExecuteReader();

                int count = 0;
                while(dr.Read())
                {
                    count++;
                }

                if (count == 1)         // Jos sql query username & password palauttaa tasan yhden tuloksen niin tunnukset ovat oikein
                {
                    CatalogWindow cw = new CatalogWindow();     
                    cw.Show();              // Avataan katalogi ikkuna
                    this.Close();           // Suljetaan tämä ikkuna
                }
                else 
                { 
                    MessageBox.Show("Wrong username or password!");     // muussa tapauksessa ei päästetä eteenpäin
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                
            }

            sqliteCon.Close();

        }

        private void tb_username_GotFocus(object sender, RoutedEventArgs e)
        {
            tb_username.Text = string.Empty;        // Kun textikenttää klikataan niin se tyhjenee
        }

        private void tb_password_GotFocus(object sender, RoutedEventArgs e)
        {
            tb_password.Password = string.Empty;    // Sama password kentässä
        }











    }
}

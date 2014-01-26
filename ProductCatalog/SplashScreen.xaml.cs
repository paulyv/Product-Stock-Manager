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
using System.Windows.Shapes;

namespace ProductCatalog
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();      // Tarvitaan tietenkin timer ottamaan aikaa
        MainWindow mw = new MainWindow(); // Seuravaan ikkunan olio joka käynnistetään kun progressbar on valmis

        public SplashScreen()
        {
            InitializeComponent();

            // Luodaan dispatcher timer joka kutsuu dispatcherTimer_Tick methodia 20 millisekunnin välein.

            
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);    // 20 millisecuntia
            dispatcherTimer.Start();       // käynnistetään timer
        }


        // Methodi jota dispatcher kutsuu. Elikkää lisätään progressbariin +1 ja muutetaan loading teksti kunnes 100% on täynnä.

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            progressbar1.Value += 1;    // prgressbariin lisätään 1
            lbl_loading.Content = "Loading: " + progressbar1.Value + "%";   //Loading texti progressbar valuen mukaan ja vielä prosentti perään
            if (progressbar1.Value == 100) // Kun progressbar on edennyt 100% asti suljetaan tämä ikkuna ja avataan MainWindow.
            {
                dispatcherTimer.Stop(); 
                mw.Show();
                this.Close();
            }
        }
    }
}

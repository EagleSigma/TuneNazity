using System;
using System.Windows;
using TuneNazity.ViewModel;

namespace TuneNazity
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();    
            }
            catch(Exception ex)
            {
                MessageBox.Show(("Error: " + ex.Message));
            }

            Closing += (s, e) => ViewModelLocator.Cleanup();
        }
        
    }
}

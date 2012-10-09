using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Xml.Linq;
using System.IO;

namespace LicenseConsumerProofOfConcept
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenLicenseFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
                                    {
                                        Title = "License File",
                                        Filter = "EverBlu license file(*.ebl)|*.ebl",
                                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                                    };

            if (openFileDialog.ShowDialog(this) ?? false)
            {
                XDocument xDoc;
                if (!LicenseFileVerifier.TryOpenLicenseFile(new FileInfo(openFileDialog.FileName), out xDoc))
                {
                    MessageBox.Show(this, "Invalid License !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (xDoc != null)
                {
                    DebugTextBox.Text = xDoc.ToString();
                }
            }
        }
    }
}

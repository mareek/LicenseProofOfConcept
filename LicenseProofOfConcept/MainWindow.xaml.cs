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
using System.Xml.Linq;
using System.Management;
using System.Security.Cryptography;
using Microsoft.Win32;
using System.IO;

namespace LicenseProofOfConcept
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

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            int maxUsers;
            var xDoc = new XDocument(
                           new XElement("License",
                               new XElement("MachineKey", MachineKeyTextBox.Text),
                               new XElement("MaxUsers", int.TryParse(MaxTextBox.Text, out maxUsers) ? maxUsers : (int?)null),
                               new XElement("ExpirationDate", (TrialCheckBox.IsChecked ?? false) ? DateTime.Now.AddMonths(2).Date : (DateTime?)null)));
            debugTextBox.Text = xDoc.ToString();

            var saveFileDialog = new SaveFileDialog
                                     {
                                         Title = "License File",
                                         Filter = "EverBlu license file(*.ebl)|*.ebl",
                                         InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                                     };
            if (saveFileDialog.ShowDialog(this) ?? false)
            {
                LicenseFileGenerator.WriteLicenseFile(new FileInfo(saveFileDialog.FileName), xDoc);
            }
        }

        private byte[] GetMachineKey()
        {
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(UnicodeEncoding.UTF8.GetBytes(GetMotherBoardSerialNumber()));
            }
        }

        private string GetMotherBoardSerialNumber()
        {
            var searcher = new ManagementObjectSearcher("select * from Win32_BaseBoard ");

            var resultBuilder = new StringBuilder();
            return searcher.Get().Cast<ManagementBaseObject>().First()["SerialNumber"].ToString();
        }

    }
}

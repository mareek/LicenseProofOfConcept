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
using System.Xml;

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
                FileInfo keyFile;
                if (string.IsNullOrWhiteSpace(this.KeyFileTextBox.Text))
                {
                    keyFile = null;
                }
                else
                {
                    keyFile = new FileInfo(this.KeyFileTextBox.Text);
                }


                XDocument xDoc;
                if (!LicenseFileVerifier.TryOpenLicenseFile(new FileInfo(openFileDialog.FileName), out xDoc, keyFile))
                {
                    SetMessage("Corrupted License file", true);
                }
                else
                {
                    /*
                        <License>
                          <MachineKey>garbage</MachineKey>
                          <MaxUsers>55</MaxUsers>
                          <ExpirationDate>2012-12-10T00:00:00+01:00</ExpirationDate>
                        </License>
                     */

                    var machineKey = xDoc.Root.Element("MachineKey").Value;
                    var maxUsers = XmlConvert.ToInt32(xDoc.Root.Element("MaxUsers").Value);
                    var expirationDateString = xDoc.Root.Element("ExpirationDate").Value;
                    var expirationDate = string.IsNullOrWhiteSpace(expirationDateString) ? (DateTime?)null : XmlConvert.ToDateTime(expirationDateString);
                    double daysLeft = ((expirationDate ?? DateTime.MaxValue).Date - DateTime.Now.Date).TotalDays;


                    if (machineKey != MachineKeyHelper.GetMachineKey())
                    {
                        SetMessage("Wrong machine", null);

                    }
                    else if (expirationDate.HasValue && daysLeft <= 0)
                    {
                        SetMessage("Trial expired", null);
                    }
                    else
                    {
                        var report = string.Format("{0} users max\r\n", maxUsers);

                        if (!expirationDate.HasValue)
                        {
                            report += "Commercial version";
                        }
                        else
                        {
                            report += string.Format("Trial version ({0} day(s) left)", daysLeft);
                        }

                        SetMessage(report, false);
                    }
                }
            }
        }

        private void SetMessage(string message, bool? inError)
        {
            DebugTextBox.Text = message;
            if (!inError.HasValue)
            {
                DebugTextBox.Background = Brushes.Orange;
                DebugTextBox.Foreground = Brushes.Black;
            }
            else if (inError.Value)
            {
                DebugTextBox.Background = Brushes.OrangeRed;
                DebugTextBox.Foreground = Brushes.White;
            }
            else
            {
                DebugTextBox.Background = Brushes.LawnGreen;
                DebugTextBox.Foreground = Brushes.Black;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MachineKeyTextBox.Text = MachineKeyHelper.GetMachineKey();
        }

        private void KeyFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Public key file",
                Filter = "Public key file (*.puk)|*.puk",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (openFileDialog.ShowDialog(this) ?? false)
            {
                KeyFileTextBox.Text = openFileDialog.FileName;
            }
        }
    }
}

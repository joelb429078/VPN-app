using System.Windows;
using System.Windows.Controls;

namespace VPNProject.Windows
{
    public partial class VpnFormWindow : Window
    {
        public VpnFormWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

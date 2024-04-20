
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using DispoAdmin.ViewModels;

namespace DispoAdmin.Views

{
    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {}
        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {}
    }
}


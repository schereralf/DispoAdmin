
using System.Windows;
//using Model3DFarm;
using ModelSQLLiteFarm;
using DispoAdmin.ViewModels;

namespace DispoAdmin.Views
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
            public OrderWindow(Order order)
        {
            InitializeComponent();

            this.DataContext = new OrderWindowViewModel(order);
        }
    }
}


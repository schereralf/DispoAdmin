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
using Model3DFarm;
using DispoAdmin.ViewModels;

namespace DispoAdmin.Views
{
    /// <summary>
    /// Interaktionslogik für OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
            public OrderWindow(Order order)
        {
            InitializeComponent();

            this.DataContext = new OrderWindowViewModel(order);
        }

        private OrderWindowViewModel ViewModel
        {
            get { return (OrderWindowViewModel)this.DataContext; }
        }
    }
}


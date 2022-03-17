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
using DispoAdmin.Scheduler;
using System.Reflection;

namespace DispoAdmin.Views
{
    /// <summary>
    /// Interaktionslogik für DispoWindow.xaml
    /// </summary>
    public partial class DispoWindow : Window
    {
        public DispoWindow(int scheduleWeek)
        {
            InitializeComponent();
            this.DataContext = new DispoWindowViewModel(scheduleWeek);        
        }

        private DispoWindowViewModel ViewModel
        {
            get { return (DispoWindowViewModel)this.DataContext; }
        }

    }
}

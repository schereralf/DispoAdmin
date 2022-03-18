using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispoBaseLib;
using Model3DFarm;
using DispoAdmin.Models;
using DispoAdmin.Views;
using System.Windows.Input;
using System.IO;

namespace DispoAdmin.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        //Setup Observable Collections and Items Lists  for each tab

        private ObservableCollection<Order> _listOrders;
        private ObservableCollection<Printer> _listPrinters;
        private ObservableCollection<ServiceLogEvent> _listServices;
        private ObservableCollection<Material> _listMaterials;

        public IList<Order> ListOrders => _listOrders;
        public IList<Printer> ListPrinters => _listPrinters;
        public IList<ServiceLogEvent> ListServices => _listServices;
        public IList<Material> ListMaterials => _listMaterials;

        private Order _selectedOrder;
        private Printer _selectedPrinter;
        private ServiceLogEvent _selectedService;
        private Material _selectedMaterial;
        private int _scheduleWeek;

        //Setup line selection and button actions

        // Order tab contains button to open jobs listing in "OrderWindow" window for selected order
        public Order SelectedOrder
        {    // for binding
            get { return _selectedOrder; }
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();               
                _cmdViewOrder.RaiseCanExecuteChanged();
                _cmdAddOrder.RaiseCanExecuteChanged();
                _cmdRemoveOrder.RaiseCanExecuteChanged();
                _cmdSaveStuff.RaiseCanExecuteChanged();
            }
        }
        public Printer SelectedPrinter
        {    // for binding
            get { return _selectedPrinter; }
            set
            {
                _selectedPrinter = value;
                OnPropertyChanged();
                _cmdAddPrinter.RaiseCanExecuteChanged();
                _cmdRemovePrinter.RaiseCanExecuteChanged();
                _cmdSaveStuff.RaiseCanExecuteChanged();
            }
        }
        // Scheduler tab needs only field to specify week and the window start button 
        public int scheduleWeek
        {   
            get { return _scheduleWeek; }
            set
            {
                _scheduleWeek = value;
                OnPropertyChanged();
                _cmdRegSchedule.RaiseCanExecuteChanged();
            }
        }

        public ServiceLogEvent SelectedService
        {   
            get { return _selectedService; }
            set
            {
                _selectedService = value;
                OnPropertyChanged();
                _cmdAddService.RaiseCanExecuteChanged();
                _cmdRemoveService.RaiseCanExecuteChanged();
                _cmdSaveStuff.RaiseCanExecuteChanged();
            }
        }

        public Material SelectedMaterial
        {    // für Binding von Auswahl in Liste
            get { return _selectedMaterial; }
            set
            {
                _selectedMaterial = value;
                OnPropertyChanged();
                _cmdAddMaterial.RaiseCanExecuteChanged();
                _cmdRemoveMaterial.RaiseCanExecuteChanged();
                _cmdSaveStuff.RaiseCanExecuteChanged();
            }
        }

        RelayCommand _cmdViewOrder;
        RelayCommand _cmdAddOrder;
        RelayCommand _cmdRemoveOrder;
        RelayCommand _cmdAddPrinter;
        RelayCommand _cmdRemovePrinter;
        RelayCommand _cmdAddService;
        RelayCommand _cmdRemoveService;
        RelayCommand _cmdRegSchedule;
        RelayCommand _cmdAddMaterial;
        RelayCommand _cmdRemoveMaterial;
        RelayCommand _cmdSaveStuff;

        public ICommand CmdViewOrder { get { return _cmdViewOrder; } }
        public ICommand CmdAddOrder { get { return _cmdAddOrder; } }
        public ICommand CmdRemoveOrder { get { return _cmdRemoveOrder; } }
        public ICommand CmdAddPrinter { get { return _cmdAddPrinter; } }
        public ICommand CmdRemovePrinter { get { return _cmdRemovePrinter; } }
        public ICommand CmdAddService { get { return _cmdAddService; } }
        public ICommand CmdRemoveService { get { return _cmdRemoveService; } }
        public ICommand CmdRegSchedule { get { return _cmdRegSchedule; } }
        public ICommand CmdAddMaterial { get { return _cmdAddMaterial; } }
        public ICommand CmdRemoveMaterial { get { return _cmdRemoveMaterial; } }
        public ICommand CmdSaveStuff { get { return _cmdSaveStuff; } }

        public MainWindowViewModel()
        {
            //using statement for a db context holding all content for all tabs -- does this at all make sense ??

            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                _listOrders = new ObservableCollection<Order>();
                _listPrinters = new ObservableCollection<Printer>();
                _listServices = new ObservableCollection<ServiceLogEvent>();
                _listMaterials = new ObservableCollection<Material>();

                _cmdViewOrder = new RelayCommand(ViewOrder, () => SelectedOrder != null);
                _cmdAddOrder = new RelayCommand(AddOrder, () => SelectedOrder != null);
                _cmdRemoveOrder = new RelayCommand(RemoveOrder, () => SelectedOrder != null);
                _cmdAddPrinter = new RelayCommand(AddPrinter, () => SelectedPrinter != null);
                _cmdRemovePrinter = new RelayCommand(RemovePrinter, () => SelectedPrinter != null);
                _cmdAddService = new RelayCommand(AddService, () => SelectedService != null);
                _cmdRemoveService = new RelayCommand(RemoveService, () => SelectedService != null);
                _cmdRegSchedule = new RelayCommand(RegSchedule);
                _cmdAddMaterial = new RelayCommand(AddMaterial, () => SelectedMaterial != null);
                _cmdRemoveMaterial = new RelayCommand(RemoveMaterial, () => SelectedMaterial != null);
                _cmdSaveStuff = new RelayCommand(SaveStuff);

                foreach (Order k in context.Orders) _listOrders.Add(k);
                //LoadOrder();
                foreach (Printer k in context.Printers) _listPrinters.Add(k);
                //LoadPrinter();
                foreach (ServiceLogEvent k in context.ServiceLogEvents) _listServices.Add(k);
                //LoadService();
                foreach (Material k in context.Materials) _listMaterials.Add(k);
                //LoadMaterial();

                context.SaveChanges();

                // A Saving button should be redundant when using ObservableCollection ??
                void SaveStuff()
                {
                    //context.SaveChanges();
                }
            }
        }

        public void AddOrder()
        {
            ListOrders.Add(SelectedOrder);
        }

        public void RemoveOrder()
        {
            ListOrders.Remove(SelectedOrder);
        }

        public void AddPrinter()
        {
            ListPrinters.Add(SelectedPrinter);
        }

        public void RemovePrinter()
        {
            ListPrinters.Remove(SelectedPrinter);
        }

        public void AddService()
        {
            ListServices.Add(SelectedService);
        }

        public void RemoveService()
        {
            ListServices.Remove(SelectedService);
        }

        public void AddMaterial()
        {
            ListMaterials.Add(SelectedMaterial);
        }

        public void RemoveMaterial()
        {
            ListMaterials.Remove(SelectedMaterial);
        }
        //Pass over to view orders window
        public void ViewOrder()
        {           
            OrderWindow orderView = new OrderWindow(SelectedOrder);
            orderView.ShowDialog();  
        }

        // Pass over to schedule window
        public void RegSchedule()
        {           
            DispoWindow scheduleView = new DispoWindow(scheduleWeek);
            scheduleView.ShowDialog();  
        }
    }
}

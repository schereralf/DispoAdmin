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

        public Order SelectedOrder
        {    // für Binding von Auswahl in Liste
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
        {    // für Binding von Auswahl in Liste
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

        public int scheduleWeek
        {    // für Binding von Auswahl in Liste
            get { return _scheduleWeek; }
            set
            {
                _scheduleWeek = value;
                OnPropertyChanged();
                _cmdRegSchedule.RaiseCanExecuteChanged();
                //_cmdOptSchedule.RaiseCanExecuteChanged();
            }
        }

        public ServiceLogEvent SelectedService
        {    // für Binding von Auswahl in Liste
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
        //RelayCommand _cmdOptSchedule;
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
        //public ICommand CmdOptSchedule { get { return _cmdOptSchedule; } }
        public ICommand CmdAddMaterial { get { return _cmdAddMaterial; } }
        public ICommand CmdRemoveMaterial { get { return _cmdRemoveMaterial; } }
        public ICommand CmdSaveStuff { get { return _cmdSaveStuff; } }

        public MainWindowViewModel()
        {
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
                //_cmdOptSchedule = new RelayCommand(OptSchedule);
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

                void SaveStuff()
                {
                    //context.SaveChanges();
                }
            }
        }

        /*public void LoadOrder()
        {
            _listOrders.Clear();  
            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                //var result = context.Orders.Include(k => k.PrintJobs);
                foreach (Order k in context.Orders) _listOrders.Add(k);
            } 
        }
        public void LoadPrinter()
        {
            _listPrinters.Clear();    
            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                //var result = from k in context.Printers.Local orderby k.PrinterType select k;
                foreach (Printer k in context.Printers) _listPrinters.Add(k);
            } 
        }
        public void LoadService()
        {
            _listServices.Clear();   
            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                //var result = from k in context.ServiceLogEvents.Local orderby k.EventCategory select k;
                foreach (ServiceLogEvent k in context.ServiceLogEvents) _listServices.Add(k);
            } 
        }

        public void LoadMaterial()
        {
            _listMaterials.Clear();    // clear old data
            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                //var result = from k in context.Materials.Local orderby k.MaterialPrice select k;
                foreach (Material k in context.Materials) _listMaterials.Add(k);
            } 
        }

        public void SaveStuff()
        {
            context.SaveChanges();
        }*/


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

        public void ViewOrder()
        {            // Neues Fenster anlegen
            // aktuell ausgewählten Kunden mitgeben
            OrderWindow orderView = new OrderWindow(SelectedOrder);
            orderView.ShowDialog();  // ShowDialog() => modales Dialog Fenster, MainWindow ist blockiert
        }

        public void RegSchedule()
        {            // Neues Fenster anlegen            // aktuell ausgewählten Kunden mitgeben
            DispoWindow scheduleView = new DispoWindow(scheduleWeek);
            scheduleView.ShowDialog();  // ShowDialog() => modales Dialog Fenster, MainWindow ist blockiert
        }

        /*public void OptSchedule()
        {            // Neues Fenster anlegen            // aktuell ausgewählten Kunden mitgeben
            DispoWindow scheduleView = new DispoWindow(scheduleWeek);
            scheduleView.ShowDialog();  // ShowDialog() => modales Dialog Fenster, MainWindow ist blockiert
        }*/
    }
}

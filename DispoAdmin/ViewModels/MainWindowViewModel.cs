
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DispoBaseLib;
using ModelSQLLiteFarm;
using DispoAdmin.Models;
using DispoAdmin.Views;
using System.Windows.Input;
using System.Windows;
using System.Linq;
using Microsoft.IdentityModel.Tokens;


namespace DispoAdmin.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        //Setup Observable Collections and Items Lists for each tab

        private readonly ObservableCollection<Order> _listOrders;
        private readonly ObservableCollection<Printer> _listPrinters;
        private readonly ObservableCollection<ServiceLogEvent> _listServices;
        private readonly ObservableCollection<Material> _listMaterials;

        public IList<Order> ListOrders => _listOrders;
        public IList<Printer> ListPrinters => _listPrinters;
        public IList<ServiceLogEvent> ListServices => _listServices;
        public IList<Material> ListMaterials => _listMaterials;

        private Order _selectedOrder;
        private Printer _selectedPrinter;
        private ServiceLogEvent _selectedService;
        private Material _selectedMaterial;
        private int _scheduleWeek;
        private static readonly IList<string> _availablePrinterModels =
        [
            "Prusa i3",
            "Prusa Mini",
            "Ender 3",
            "Ender 5",
            "Ultimaker 2",
            "Resin Printer"
        ];
        private static readonly IList<string> _availableMaterials =
[
            "PLA",
            "ABS",
            "PET-G",
            "UV Resin",
];
        private double? _costsTotal;
        private int _countOrders;
        private double _revenuesTotal;
        private int? _countPrintJobs;
        //private List<double?> _countMaterials;

        public static IList<string> AvailablePrinterModels => _availablePrinterModels;
        public static IList<string> AvailableMaterials => _availableMaterials;

        public double? CostsTotal
        {    // for binding
            get { return _costsTotal; }
            set
            {
                _costsTotal = value;
                OnPropertyChanged();
            }
        }
        public int CountOrders
        {    // for binding
            get { return _countOrders; }
            set
            {
                _countOrders = value;
                OnPropertyChanged();
            }
        }
        public double RevenuesTotal
        {    // for binding
            get { return _revenuesTotal; }
            set
            {
                _revenuesTotal = value;
                OnPropertyChanged();
            }
        }
        public int? CountPrintJobs
        {    // for binding
            get { return _countPrintJobs; }
            set
            {
                _countPrintJobs = value;
                OnPropertyChanged();
            }
        }

        /*public List<double?> CountMaterials
        {
            get
            {
                return _countMaterials;
            }
            set
            {
                _countMaterials = value;
                OnPropertyChanged();
            }
        }*/

        //Setup line selection and button actions
        //Order tab contains button to open jobs listing in "OrderWindow" window for selected order

        public Order SelectedOrder
        {    // for binding
            get
            {
                if (_selectedOrder!=null&&(_selectedOrder.OrderName.IsNullOrEmpty()
                     || _selectedOrder.CustomerName.IsNullOrEmpty()
                     || _selectedOrder.OrderPrice == 0
                     || !_selectedOrder.DateIn.HasValue
                     || !_selectedOrder.DateDue.HasValue))
                    MessageBox.Show("Warning ! Please ensure that there are proper values in all of the fields !");
                return _selectedOrder;
            }

            set
            {
                _selectedOrder = value;
                {
                    OnPropertyChanged();
                    _cmdViewOrder.RaiseCanExecuteChanged();
                    _cmdAddOrder.RaiseCanExecuteChanged();
                    _cmdRemoveOrder.RaiseCanExecuteChanged();
                    _cmdSaveStuff.RaiseCanExecuteChanged();
                }
            }
        }
        public Printer SelectedPrinter
        {    // for binding
            get {
                if (_selectedPrinter!=null&&(_selectedPrinter.PrinterType.IsNullOrEmpty()
                   || !_selectedPrinter.PrinterPurchDate.HasValue
                   || _selectedPrinter.PrinterPurchPrice==0))
                   MessageBox.Show("Warning!! Please ensure that there are proper values in all of the fields !!");
                return _selectedPrinter; }
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
        public int ScheduleWeek
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
        {
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

        readonly RelayCommand _cmdViewOrder;
        readonly RelayCommand _cmdAddOrder;
        readonly RelayCommand _cmdRemoveOrder;
        readonly RelayCommand _cmdAddPrinter;
        readonly RelayCommand _cmdRemovePrinter;
        readonly RelayCommand _cmdAddService;
        readonly RelayCommand _cmdRemoveService;
        readonly RelayCommand _cmdRegSchedule;
        readonly RelayCommand _cmdAddMaterial;
        readonly RelayCommand _cmdRemoveMaterial;
        readonly RelayCommand _cmdSaveStuff;

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
            //using statement for a db context holding all content for all tabs
            PrinterfarmContext printerfarmContext = DispoAdminModel.Default.GetDBContext();
            using PrinterfarmContext context = printerfarmContext;

            _listOrders = [];
            _listPrinters = [];
            _listServices = [];
            _listMaterials = [];

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
            foreach (Printer k in context.Printers) _listPrinters.Add(k);
            foreach (ServiceLogEvent k in context.ServiceLogEvents) _listServices.Add(k);
            foreach (Material k in context.Materials) _listMaterials.Add(k);

            _revenuesTotal = _listOrders.Select(o => o.OrderPrice).ToList().Sum();
            _countOrders = _listOrders.Count;
            _costsTotal = _listOrders.Select(o => o.PrintJobsCost).ToList().Sum();
            _countPrintJobs = _listOrders.Select(o => o.PrintJobsCount).ToList().Sum();

            /*
            foreach (string i in AvailableMaterials)
            {
                double? countlisti = 0;
                foreach (Order o in  _listOrders) 
                { 
                foreach (PrintJob p in o.PrintJobs)
                    {
                        if (p.Material == i)
                        {
                            countlisti += p.WeightMaterial;
                        }
                    }
                }
                _countMaterials.Add(countlisti);
            }
            */
        }

        public void SaveStuff()
        {
            PrinterfarmContext printerfarmContext = DispoAdminModel.Default.GetDBContext();
            using PrinterfarmContext updatedWorkSetup = printerfarmContext;

            foreach (Order k in updatedWorkSetup.Orders) updatedWorkSetup.Orders.Remove(k);
            foreach (Printer k in updatedWorkSetup.Printers) updatedWorkSetup.Printers.Remove(k);
            foreach (ServiceLogEvent k in updatedWorkSetup.ServiceLogEvents) updatedWorkSetup.ServiceLogEvents.Remove(k);
            foreach (Material k in updatedWorkSetup.Materials) updatedWorkSetup.Materials.Remove(k);

            foreach (Order k in ListOrders) updatedWorkSetup.Orders.Add(k);
            foreach (Printer k in ListPrinters) updatedWorkSetup.Printers.Add(k);
            foreach (ServiceLogEvent k in ListServices) updatedWorkSetup.ServiceLogEvents.Add(k);
            foreach (Material k in ListMaterials) updatedWorkSetup.Materials.Add(k);

            updatedWorkSetup.SaveChanges();
            MessageBox.Show("All saved, please hit OK  here and move to the next window\n or just close this window and revisit your printer park later !");
        }

        public void AddOrder()
        {
            // TODO: include exception for when order due dates erroneoisly are <= the file dates !
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
            if (ListMaterials.IsNullOrEmpty() || ListPrinters.IsNullOrEmpty())
            {
                MessageBox.Show("Please ensure that you have defined printing materials and/or printers that you can use to print with before you create printjobs !");
            }
            else
            {
                OrderWindow orderView = new(SelectedOrder);
                orderView.ShowDialog();
            }
        }

        // Pass over to schedule window
        public void RegSchedule()
        {
            DispoWindow scheduleView = new(ScheduleWeek)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            scheduleView.ShowDialog();
        }
    }
}

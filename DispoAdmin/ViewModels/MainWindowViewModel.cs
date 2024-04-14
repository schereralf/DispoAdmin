
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
using System;
using System.Text.Json;
using System.IO;
using System.ComponentModel.Design;


namespace DispoAdmin.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        //Setup Observable Collections and Items Lists for each tab

        private readonly ObservableCollection<Order> _listOrders;
        private readonly ObservableCollection<Printer> _listPrinters;
        private readonly ObservableCollection<ServiceLogEvent> _listServices;
        private readonly ObservableCollection<Material> _listMaterials;

        readonly string saveJsonPath = @"./savedParametersList.json";

        public IList<Order> ListOrders => _listOrders;
        public IList<Printer> ListPrinters => _listPrinters;
        public IList<ServiceLogEvent> ListServices => _listServices;
        public IList<Material> ListMaterials => _listMaterials;

        private Order _selectedOrder;
        private Printer _selectedPrinter;
        private ServiceLogEvent _selectedService;
        private Material _selectedMaterial;
        private int _scheduleWeek;
        private int _scheduleYear;
        private int _targetRateOfReturn;
        private int _depreciationTime;
        private int _workHoursPerWeek;
        private int _laborHourlyRate;
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

        public static IList<string> AvailablePrinterModels => _availablePrinterModels;
        public static IList<string> AvailableMaterials => _availableMaterials;

        public int ScheduleYear
        {    // for binding
            get {
                if (_scheduleYear>DateTime.Today.Year || _scheduleYear< DateTime.Today.Year-10)
                    MessageBox.Show("Warning ! This year is off limits, please select either the current year or any of the last 10 years !");

                return _scheduleYear; }
            set
            {
                _scheduleYear = value;
                OnPropertyChanged();
            }
        }
        public int TargetRateOfReturn
        {
            get
            {
                if (_targetRateOfReturn > 50 || _targetRateOfReturn < 0)
                    MessageBox.Show("Warning ! Your IRR is either negative or too large (max is 50) !");
                return _targetRateOfReturn;
            }
            set
            {
                _targetRateOfReturn = value;
                OnPropertyChanged();
            }
        }
        public int DepreciationTime
        {
            get 
            {
                if (_depreciationTime > 20 || _depreciationTime < 0)
                    MessageBox.Show("Warning ! Your depreciation time is either negative or too large (max is 20) !");
                return _depreciationTime; 
            }
            set 
            { 
                _depreciationTime=value;
                OnPropertyChanged();
            }
        }

        public int WorkHoursPerWeek
        {
            get
            {
                if (_workHoursPerWeek > 40 || _workHoursPerWeek < 5)
                    MessageBox.Show("Warning ! Your weekly work hours are either too few (min is 5) or too many (max is 40) !");
                return _workHoursPerWeek;
            }
            set
            {
                _workHoursPerWeek = value;
                OnPropertyChanged();
            }
        }

        public int LaborHourlyRate
        {
            get
            {
                if (_laborHourlyRate > 1000 || _laborHourlyRate < 1)
                    MessageBox.Show("Warning ! Your labor rate is either too small (min is 1) or too large (max is 1000) !");
                return _laborHourlyRate;
            }
            set
            {
                _laborHourlyRate = value;
                OnPropertyChanged();
            }
        }

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
                _cmdRemoveMaterial.RaiseCanExecuteChanged();
                _cmdSaveStuff.RaiseCanExecuteChanged();
            }
        }

        readonly RelayCommand _cmdViewOrder;
        readonly RelayCommand _cmdRemoveOrder;
        readonly RelayCommand _cmdRemovePrinter;
        readonly RelayCommand _cmdRemoveService;
        readonly RelayCommand _cmdRegSchedule;
        readonly RelayCommand _cmdRemoveMaterial;
        readonly RelayCommand _cmdSaveStuff;

        public ICommand CmdViewOrder { get { return _cmdViewOrder; } }
        public ICommand CmdRemoveOrder { get { return _cmdRemoveOrder; } }
        public ICommand CmdRemovePrinter { get { return _cmdRemovePrinter; } }
        public ICommand CmdRemoveService { get { return _cmdRemoveService; } }
        public ICommand CmdRegSchedule { get { return _cmdRegSchedule; } }
        public ICommand CmdRemoveMaterial { get { return _cmdRemoveMaterial; } }
        public ICommand CmdSaveStuff { get { return _cmdSaveStuff; } }

        public MainWindowViewModel()
        {
            //using statement for a db context holding all content for all tabs
            PrinterfarmContext printerfarmContext = DispoAdminModel.Default.GetDBContext();
            using PrinterfarmContext context = printerfarmContext;

            var _framework = GetFramework();
            _scheduleYear= _framework[0];
            _targetRateOfReturn= _framework[1];
            _depreciationTime= _framework[2];
            _workHoursPerWeek= _framework[3];
            _laborHourlyRate= _framework[4];

            _listOrders = [];
            _listPrinters = [];
            _listServices = [];
            _listMaterials = [];

            _cmdViewOrder = new RelayCommand(ViewOrder, () => SelectedOrder != null);
            _cmdRemoveOrder = new RelayCommand(RemoveOrder, () => SelectedOrder != null);
            _cmdRemovePrinter = new RelayCommand(RemovePrinter, () => SelectedPrinter != null);
            _cmdRemoveService = new RelayCommand(RemoveService, () => SelectedService != null);
            _cmdRegSchedule = new RelayCommand(RegSchedule);
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

            SaveFramework(_scheduleYear, _targetRateOfReturn, _depreciationTime, _workHoursPerWeek, _laborHourlyRate);
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
            SaveFramework(_scheduleYear, _targetRateOfReturn, _depreciationTime, _workHoursPerWeek, _laborHourlyRate);
        }

        public void AddOrder()
        {
            // TODO: include exception for when order due dates erroneously are <= the file dates !
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
                OrderWindow orderView = new(SelectedOrder, ScheduleYear, TargetRateOfReturn, DepreciationTime, WorkHoursPerWeek, LaborHourlyRate);
                orderView.ShowDialog();
            }
        }

        // Pass over to schedule window
        public void RegSchedule()
        {
            DispoWindow scheduleView = new(ScheduleWeek, ScheduleYear)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            scheduleView.ShowDialog();
        }


        public void SaveFramework(int selectedyear, int targetrateofreturn, int depreciation, int workhours, int hourlyrate)
        {
            List<string> frameworkAsString =
            [
                selectedyear.ToString(),
                targetrateofreturn.ToString(),
                depreciation.ToString(),
                workhours.ToString(),
                hourlyrate.ToString(),
            ];

            var recipesListAsJson = JsonSerializer.Serialize(frameworkAsString);

            // Saving json copy of the recipe names, ingredients and ids in a local directory           
            File.WriteAllText(saveJsonPath, recipesListAsJson);
        }
        
        public List<int> GetFramework()
        {
            List<int> myFramework = new() { 0, 0,0,0 ,0};
            if (File.Exists(saveJsonPath))
            {
                string serializedFramework = File.ReadAllText(saveJsonPath);
                List<string>? workingFramework = JsonSerializer.Deserialize<List<string>>(serializedFramework);
                if (workingFramework != null)
                    for (int i=0; i<5; i++) { myFramework[i] = int.Parse(workingFramework[i]); }
                else Console.WriteLine("Note that we have no analytical framework data entered yet !");
            }
            return myFramework;
        }

    }
}

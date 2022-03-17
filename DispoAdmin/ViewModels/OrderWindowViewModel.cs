using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model3DFarm;
using DispoBaseLib;
using System.Windows.Input;
using DispoAdmin.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Win32;

namespace DispoAdmin.ViewModels
{
    class OrderWindowViewModel : BaseViewModel
    {
        private Order _order;

        public Order Order
        {    // für Binding von Auswahl in Liste
            get { return _order; }
            set
            {
                _order = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<PrintJob> _listPrintJobs;
        public IList<PrintJob> ListPrintJobs => _listPrintJobs;

        private PrintJob _selectedPrintJob;
        private string gcodeString;
        public PrintJob SelectedPrintJob
        {
            get { return _selectedPrintJob; }
            set
            {
                _selectedPrintJob = value;
                OnPropertyChanged();
                _cmdAddPrintJob.RaiseCanExecuteChanged();
                _cmdRemovePrintJob.RaiseCanExecuteChanged();
                _cmdParsePrintJob.RaiseCanExecuteChanged();
            }
        }

        RelayCommand _cmdAddPrintJob;
        RelayCommand _cmdRemovePrintJob;
        RelayCommand _cmdParsePrintJob;

        public ICommand CmdAddPrintJob { get { return _cmdAddPrintJob; } }
        public ICommand CmdRemovePrintJob { get { return _cmdRemovePrintJob; } }
        public ICommand CmdParsePrintJob { get { return _cmdParsePrintJob; } }
        // Konstruktor mit Parameter
        public OrderWindowViewModel(Order order)
        {
            this.Order = order;

            _listPrintJobs = new ObservableCollection<PrintJob>();

            _cmdAddPrintJob = new RelayCommand(AddPrintJob, () => SelectedPrintJob != null);
            _cmdRemovePrintJob = new RelayCommand(RemovePrintJob, () => SelectedPrintJob != null);
            _cmdParsePrintJob = new RelayCommand(ParsePrintJob, () => SelectedPrintJob != null);

            LoadPrintJob();
        }


        public void LoadPrintJob()
        {
            //_listPrintJobs.Clear();    // clear old data
            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                var result = from k in context.PrintJobs.Include(k => k.Order) orderby k.JobName select k;
                foreach (PrintJob k in result)
                {
                    _listPrintJobs.Add(k);
                }
            }
        }
        public void AddPrintJob()
        {
            ListPrintJobs.Add(SelectedPrintJob);
        }

        public void RemovePrintJob()
        {
            ListPrintJobs.Remove(SelectedPrintJob);
        }
        public void ParsePrintJob()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\Alf\Wifi_Coursework\GcodeFiles";
            openFileDialog.Filter = "Gcode files (*.gcode)|*.gcode|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            gcodeString = File.ReadAllText(openFileDialog.FileName);

            SelectedPrintJob.GcodeAdresse=openFileDialog.FileName;


        }        
    }
}




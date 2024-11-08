﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ModelSQLLiteFarm;
using DispoBaseLib;
using System.Windows.Input;
using DispoAdmin.Models;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using Microsoft.IdentityModel.Tokens;

namespace DispoAdmin.ViewModels
{
    class OrderWindowViewModel : BaseViewModel
    {
        private Order _order;

        public Order Order
        {
            get { return _order;}
            set {_order = value;}
        }
        private readonly ObservableCollection<PrintJob> _listPrintJobs;
        private readonly IList<Schedule> _listSchedules;
        public double materialPrice = new();
        public double printerHourlyRate = new();
        public IList<PrintJob> ListPrintJobs => _listPrintJobs;
        public IList<Schedule> ListSchedules => _listSchedules;
        public static IList<string> AvailableMaterials => MainWindowViewModel.AvailableMaterials;

        public List<Material> Materials = [];
        public List<Printer> Printers = [];

        private PrintJob _selectedPrintJob;

        readonly DateTime dayDateStart;

        private string gcodeText;
        private int layerheight;
        private int prusastart;
        private int gcodestart;
        public PrintJob SelectedPrintJob
        {
            //in addition to add, remove and save print jobs in an order, a fourth option called
            //ParsePrintJob contains code for extracting data from Gcode files

            get { return _selectedPrintJob; }
            set
            {
                _selectedPrintJob = value;
                OnPropertyChanged();
                _cmdRemovePrintJob.RaiseCanExecuteChanged();
                _cmdParsePrintJob.RaiseCanExecuteChanged();
                _cmdSavePrintJobs.RaiseCanExecuteChanged();
            }
        }
        
        private readonly RelayCommand _cmdRemovePrintJob;
        private readonly RelayCommand _cmdParsePrintJob;
        private readonly RelayCommand _cmdSavePrintJobs;

        public ICommand CmdRemovePrintJob { get { return _cmdRemovePrintJob; } }
        public ICommand CmdParsePrintJob { get { return _cmdParsePrintJob; } }
        public ICommand CmdSavePrintJobs { get { return _cmdSavePrintJobs; } }

        // Constructor with order as parameter
        public OrderWindowViewModel(Order order, int year, int rateOfReturn, int depreciationTime, int workHoursPerWeek, int laborHourlyRate)
        {
            Order = order;
            dayDateStart = new(year, 1, 1);
            while (dayDateStart.DayOfWeek != DayOfWeek.Monday) { dayDateStart=dayDateStart.AddDays(1); }

            _listPrintJobs = [];
            _listSchedules = [];

            _cmdRemovePrintJob = new RelayCommand(RemovePrintJob, () => SelectedPrintJob != null);
            _cmdParsePrintJob = new RelayCommand(ParsePrintJob, () => SelectedPrintJob != null);
            _cmdSavePrintJobs = new RelayCommand(SavePrintJobs);

            using PrinterfarmContext initialContext = DispoAdminModel.Default.GetDBContext();
            {
                var initialPrintJobsList = from printJob in initialContext.PrintJobs
                                           where printJob.Order == Order
                                           orderby printJob.JobName
                                           select printJob;

                foreach (Material m in initialContext.Materials) Materials.Add(m);
                foreach (Printer p in initialContext.Printers) Printers.Add(p);

                foreach (PrintJob printJob in initialPrintJobsList)
                {
                    printJob.OrderID = order.OrderID;
                    Schedule schedule = new()
                    {
                        JobID = printJob.JobID,
                        RO_Time = printJob.PrintTime,
                        TimeStart = order.DateIn,
                        TimeEnd = order.DateDue,
                        ScheduleWeek = (order.DateIn - dayDateStart).Value.Days / 7,
                        PrinterID = printJob.PrinterType
                    };

                    Printer printJobPrinter = new();
                    Material printJobMaterial = Materials.FirstOrDefault(m => m.MaterialName.Trim() == printJob.Material.Trim());

                    if (printJob.Material == "UV Resin")
                    {
                        printJobPrinter = Printers.FirstOrDefault(p => p.PrinterType == "Resin Printer");
                        MessageBox.Show("When scheduling resin printers, please enter schedule-relevant data manually and don't try to use the gcode data extraction button !");
                        printJob.GcodeAdress = "This PrintJob does not use Gcode";
                    }
                    else printJobPrinter = Printers.FirstOrDefault(p => p.PrinterID == printJob.PrinterType);

                    //The economic calculations are very rudimentary.  We assume:
                    //  - the amortization period for all printers is provided by the operator, as is the IRR
                    //  - average loading of available total hours per year (includes weekends, holidays and 24 hrs a day) is 24%
                    //  - only machine time is counted, which includes MR and run time (no proper maintenance can be scheduled here yet)

                    materialPrice = (double)printJobMaterial.MaterialPrice;
                    printerHourlyRate = printJobPrinter.PrinterPurchPrice / depreciationTime / workHoursPerWeek / 52*(1+rateOfReturn/100);
                    
                    printJob.PrinterType=printJobPrinter.PrinterID;

                    schedule.MR_Time = printJobPrinter.MRTimeEst;
                    printJob.Costs = materialPrice * printJob.WeightMaterial / 1000 + (schedule.MR_Time + schedule.RO_Time) * printerHourlyRate + schedule.MR_Time * laborHourlyRate;

                    if (printJob.OrderID != null)
                    {
                        _listPrintJobs.Add(printJob);
                        _listSchedules.Add(schedule);
                    }
                }
                order.PrintJobsCost = _listPrintJobs.Select(c=>c.Costs).Sum();
                order.PrintJobsCount = _listPrintJobs.Count;

                initialContext.SaveChanges();
            }
        }

        public void SavePrintJobs()
        {
            using PrinterfarmContext updatedContext = DispoAdminModel.Default.GetDBContext();

            var initialPrintJobsList = from printJob in updatedContext.PrintJobs
                          where printJob.Order == Order
                          orderby printJob.JobName
                          select printJob;
            var initialSchedulesList = from schedule in updatedContext.Schedules
                          where schedule.PrintJob.Order == Order
                          select schedule;

            foreach (PrintJob printJob in initialPrintJobsList) updatedContext.PrintJobs.Remove(printJob);
            foreach (Schedule schedule in initialSchedulesList) updatedContext.Schedules.Remove(schedule);

            var updatedPrintJobsList = from printJob in ListPrintJobs
                                       orderby printJob.JobName
                                       select printJob;

            foreach (PrintJob printJob in updatedPrintJobsList)
            {
                if (!printJob.JobName.IsNullOrEmpty()&&!printJob.Material.IsNullOrEmpty())
                {
                    printJob.OrderID = Order.OrderID;
                    updatedContext.PrintJobs.Add(printJob);
                }
                else
                {
                    MessageBox.Show("Please give this job a name and/or a material before closing the window, then hit the save button again !");
                }
            }

            var updatedSchedulesList = from schedule in _listSchedules
                           orderby schedule.ScheduleWeek
                           select schedule;

            foreach (Schedule schedule in updatedSchedulesList)
            {
                if (schedule.ScheduleWeek != null)
                {
                    schedule.PrintJob = updatedPrintJobsList.Where(p => p.JobID == schedule.JobID).FirstOrDefault();
                    schedule.JobScheduleID = schedule.JobID; 
                    updatedContext.Schedules.Add(schedule);
                }
            }
            updatedContext.SaveChanges();
            MessageBox.Show("All saved, please hit OK here and then close the printjobs window for this order!");
      }

        public void RemovePrintJob()
        {
            Schedule s = _listSchedules.FirstOrDefault(s => s.JobID == SelectedPrintJob.JobID);
            ListPrintJobs.Remove(SelectedPrintJob);
            _listSchedules.Remove(s);
        }

        public void ParsePrintJob()
        {
            // Window for selecting gcode files for data extraction
            // Three slicer+printer combinations possible now
            // 1. Cura and Ultimaker2
            // 2. PrusaSlicer and Prusa Mini
            // 3. Cura and Ender 3

            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = @"C:\",
                Filter = "Gcode files (*.gcode)|*.gcode|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
                _selectedPrintJob.GcodeAdress = openFileDialog.FileName;
            try
            {
                string[] gcodeLines = File.ReadAllLines(openFileDialog.FileName);
                gcodeText = File.ReadAllText(openFileDialog.FileName);
                ParseGcode(gcodeLines);
            }
            catch 
            {
                MessageBox.Show("The filename you submitted here is null or empty");
            }
            // in case someone closes the gcode files window prematurely, set up a try catch exception handler here..
            SavePrintJobs();
        }

        private void ParseGcode(string[] gcodeLines)
        {
            if (gcodeText.Contains("UltiGCode") && gcodeText.Contains("Cura_SteamEngine"))
            {
                _selectedPrintJob.PrinterType = Printers.Where(p=>p.PrinterType=="Ultimaker 2").FirstOrDefault().PrinterID;
                Read_Ultimaker(gcodeLines);
            }
            else if (gcodeText.Contains("PrusaSlicer") && gcodeText.Contains("MINI"))
            {
                _selectedPrintJob.PrinterType = Printers.Where(p => p.PrinterType == "Prusa Mini").FirstOrDefault().PrinterID;
                Read_PrusaMini(gcodeLines);
            }
            else if (gcodeText.Contains("FLAVOR:Marlin") && (gcodeText.Contains("Cura_SteamEngine")))
            {
                _selectedPrintJob.PrinterType = Printers.Where(p => p.PrinterType == "Ender 3").FirstOrDefault().PrinterID;
                Read_Ender3(gcodeLines);
            }
            else MessageBox.Show ("This printer has not yet been included");
        }

        // first combination - Gcode for a Ultimaker job created with the Cura slicer
        private void Read_Ultimaker(string[] gcodeLines)
        {
            string printtimetxt = gcodeLines[1][(gcodeLines[1].IndexOf(':') + 1)..];
            float printtime = (float)int.Parse(printtimetxt) / 3600;
            _selectedPrintJob.PrintTime = (double)printtime;

            string nozzletxt = gcodeLines[4][(gcodeLines[4].IndexOf(':') + 1)..];
            float nozzle = float.Parse(nozzletxt, System.Globalization.CultureInfo.InvariantCulture);
            _selectedPrintJob.NozzleDiam_mm = (double)nozzle;

            string weightmaterialtxt = gcodeLines[2][(gcodeLines[2].IndexOf(':') + 1)..];
            float weightmaterial = (float)int.Parse(weightmaterialtxt) / 1000;
            _selectedPrintJob.WeightMaterial = (double)weightmaterial;

            switch (nozzle) { case (float)0.15: { layerheight = 10; break; } case (float)0.4: { layerheight = 20; break; } default: { layerheight = 30; break; } }
            _selectedPrintJob.LayerHeight = layerheight;

            string volXtxt = gcodeLines[8][(gcodeLines[8].IndexOf(':') + 1)..];
            float volX = float.Parse(volXtxt, System.Globalization.CultureInfo.InvariantCulture);
            int volx = (int)volX;
            _selectedPrintJob.VolX = volx;

            string volYtxt = gcodeLines[9][(gcodeLines[9].IndexOf(':') + 1)..];
            float volY = float.Parse(volYtxt, System.Globalization.CultureInfo.InvariantCulture);
            int voly = (int)volY;
            _selectedPrintJob.VolY = voly;

            string volZtxt = gcodeLines[10][(gcodeLines[10].IndexOf(':') + 1)..];
            float volZ = float.Parse(volZtxt, System.Globalization.CultureInfo.InvariantCulture);
            int volz = (int)volZ;
            _selectedPrintJob.VolZ = volz;
        }

        // Second combination - Gcode for a Prusa Mini job created with the PrusaSlicer slicer 
        private void Read_PrusaMini(string[] gcodeLines)
        {
            for (int i = 0; i < gcodeLines.Length; i++) if (gcodeLines[i].Contains("filament used [mm]")) prusastart = i;
                else if (gcodeLines[i].Contains("layer_gcode")) gcodestart = i;

            string printtimetxt = gcodeLines[prusastart + 6][(gcodeLines[prusastart + 6].IndexOf("= ") + 1)..];
            string hourstxt = printtimetxt[..printtimetxt.IndexOf('h')];
            string minstxt = printtimetxt.Substring(printtimetxt.IndexOf('h') + 1, printtimetxt.IndexOf('m') - printtimetxt.IndexOf('h') - 1);
            string secstxt = printtimetxt.Substring(printtimetxt.IndexOf('m') + 1, printtimetxt.IndexOf('s') - printtimetxt.IndexOf('m') - 1);
            _selectedPrintJob.PrintTime = (double)(int.Parse(hourstxt) + (float)int.Parse(minstxt) / 60 + (float)int.Parse(secstxt) / 3600);

            //TODO:Check this one+update
            string nozzletxt = gcodeLines[prusastart + 58][(gcodeLines[prusastart + 58].IndexOf("= ") + 1)..];
            float nozzle = float.Parse(nozzletxt, System.Globalization.CultureInfo.InvariantCulture);
            _selectedPrintJob.NozzleDiam_mm = (double)nozzle;

            string weightmaterialtxt = gcodeLines[prusastart + 2][(gcodeLines[prusastart + 2].IndexOf("= ") + 1)..];
            float weightmaterial = float.Parse(weightmaterialtxt.Trim(), System.Globalization.CultureInfo.InvariantCulture);
            _selectedPrintJob.WeightMaterial = (double)weightmaterial;

            string layertxt = gcodeLines[gcodestart + 1][(gcodeLines[gcodestart + 1].IndexOf("= ") + 2)..];
            float layerheight = 100 * float.Parse(layertxt, System.Globalization.CultureInfo.InvariantCulture);
            _selectedPrintJob.LayerHeight = (int)layerheight;

            int volx = 180;
            _selectedPrintJob.VolX = volx;

            int voly = 180;
            _selectedPrintJob.VolY = voly;

            int volz = 180;
            _selectedPrintJob.VolZ = volz;
        }

        // Third combination - Gcode for a Ender-3 job created with the Cura slicer 
        private void Read_Ender3(string[] gcodeLines)
        {
            string printtimetxt = gcodeLines[1][(gcodeLines[1].IndexOf(':') + 1)..];
            float printtime = (float)int.Parse(printtimetxt) / 3600;
            _selectedPrintJob.PrintTime = (double)printtime;

            _selectedPrintJob.NozzleDiam_mm = 0.4;

            string weightmaterialtxt = gcodeLines[2][(gcodeLines[2].IndexOf(':') + 1)..];
            string weighttxt2 = weightmaterialtxt[1..weightmaterialtxt.IndexOf('.')];
            float weightmaterial = int.Parse(weighttxt2);
            _selectedPrintJob.WeightMaterial = (double)weightmaterial;

            string layertxt = gcodeLines[3][(gcodeLines[2].IndexOf(':') + 1)..];
            float layerheight = float.Parse(layertxt, System.Globalization.CultureInfo.InvariantCulture) * 100;
            _selectedPrintJob.LayerHeight = (int)layerheight;

            string volXtxt = gcodeLines[7][(gcodeLines[7].IndexOf(':') + 1)..];
            float volX = float.Parse(volXtxt, System.Globalization.CultureInfo.InvariantCulture);
            int volx = (int)volX;
            _selectedPrintJob.VolX = volx;

            string volYtxt = gcodeLines[8][(gcodeLines[8].IndexOf(':') + 1)..];
            float volY = float.Parse(volYtxt, System.Globalization.CultureInfo.InvariantCulture);
            int voly = (int)volY;
            _selectedPrintJob.VolY = voly;

            string volZtxt = gcodeLines[9][(gcodeLines[9].IndexOf(':') + 1)..];
            float volZ = float.Parse(volZtxt, System.Globalization.CultureInfo.InvariantCulture);
            int volz = (int)volZ;
            _selectedPrintJob.VolZ = volz;
        }
    }
}

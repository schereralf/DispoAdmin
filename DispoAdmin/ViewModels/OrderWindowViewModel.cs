using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Model3DFarm;
using DispoBaseLib;
using System.Windows.Input;
using DispoAdmin.Models;
using System.IO;
using Microsoft.Win32;

namespace DispoAdmin.ViewModels
{
    class OrderWindowViewModel : BaseViewModel
    {
        private Order _order;

        public Order Order
        {    // 
            get { return _order; }
            set
            {
                _order = value;
            }
        }
        private readonly ObservableCollection<PrintJob> _listPrintJobs;
		private readonly List<Schedule> _listSchedules;
		public IList<PrintJob> ListPrintJobs => _listPrintJobs;
		//public IList<Schedule> ListSchedules => _listSchedules;

		private PrintJob _selectedPrintJob;

		DateTime dayDateStart = new (2022, 1, 3);

		private string gcodeText;
        private int layerheight;
        private int prusastart;
        public PrintJob SelectedPrintJob
        {
			//in addition to add, remove and save print jobs in an order, a fourth option called
			//ParsePrintJob contains code for extracting data from Gcode files

            get { return _selectedPrintJob; }
            set
            {
                _selectedPrintJob = value;
                OnPropertyChanged();
                _cmdAddPrintJob.RaiseCanExecuteChanged();
                _cmdRemovePrintJob.RaiseCanExecuteChanged();
                _cmdParsePrintJob.RaiseCanExecuteChanged();
				_cmdSavePrintJobs.RaiseCanExecuteChanged();
            }
        }

        private readonly RelayCommand _cmdAddPrintJob;
        private readonly RelayCommand _cmdRemovePrintJob;
        private readonly RelayCommand _cmdParsePrintJob;
		private readonly RelayCommand _cmdSavePrintJobs;

		public ICommand CmdAddPrintJob { get { return _cmdAddPrintJob; } }
        public ICommand CmdRemovePrintJob { get { return _cmdRemovePrintJob; } }
        public ICommand CmdParsePrintJob { get { return _cmdParsePrintJob; } }
		public ICommand CmdSavePrintJobs { get { return _cmdSavePrintJobs; } }

		// Konstruktor mit Parameter
		public OrderWindowViewModel(Order order)
		{
			this.Order = order;

			_listPrintJobs = new ObservableCollection<PrintJob>();
			_listSchedules = new List<Schedule>();

			_cmdAddPrintJob = new RelayCommand(AddPrintJob, () => SelectedPrintJob != null);
			_cmdRemovePrintJob = new RelayCommand(RemovePrintJob, () => SelectedPrintJob != null);
			_cmdParsePrintJob = new RelayCommand(ParsePrintJob, () => SelectedPrintJob != null);
			_cmdSavePrintJobs = new RelayCommand(SavePrintJobs);

			using PrinterfarmContext context = DispoAdminModel.Default.GetDBContext();
			{
				var result = from k in context.PrintJobs
							 where k.Order == this.Order
							 orderby k.JobName
							 select k;

				foreach (PrintJob k in result)
				{
					k.OrderID = order.OrderID;
					Schedule s = new Schedule
					{
						PrintJobID = k.JobID,
						RO_Time = k.PrintTime,
						TimeStart = order.DateIn,
						TimeEnd = order.DateDue,
						ScheduleWeek = (order.DateIn - dayDateStart).Value.Days / 7,
						MR_Time = 0.5,
						PrinterID = k.PrinterType
					};

					_listPrintJobs.Add(k);
					_listSchedules.Add(s);
				}
			}
		}

		public void SavePrintJobs()
        {
			using PrinterfarmContext context2 = DispoAdminModel.Default.GetDBContext();

			var result1 = from k in context2.PrintJobs
						 where k.Order == this.Order
						 orderby k.JobName
						 select k;
			var termine = from l in context2.Schedules
						  where l.PrintJob.Order == this.Order
						  select l;

			foreach (PrintJob k in result1)
			{
				context2.PrintJobs.Remove(k);
			}

			foreach (Schedule l in termine)
			{
				context2.Schedules.Remove(l);
			}

			var result2 = from k in ListPrintJobs
						 orderby k.JobName
						 select k;
			foreach (PrintJob k in result2)
			{
				if (k.JobName != null)
				{
					k.OrderID = this.Order.OrderID;
					Schedule s = new Schedule
					{
						PrintJobID = k.JobID,
						RO_Time = k.PrintTime,
						TimeStart = this.Order.DateIn,
						TimeEnd = this.Order.DateDue,
						ScheduleWeek = (this.Order.DateIn - dayDateStart).Value.Days / 7,
						MR_Time = 0.5,
						PrinterID = k.PrinterType
					};

					context2.PrintJobs.Add(k);
					context2.Schedules.Add(s);

				}
			}

			context2.SaveChanges();
		}

        public void AddPrintJob()
        {
			Schedule s = _listSchedules.FirstOrDefault(s => s.PrintJobID == SelectedPrintJob.JobID);
			ListPrintJobs.Add(SelectedPrintJob);
			_listSchedules.Add(s);
		}

        public void RemovePrintJob()
        {
			Schedule s = _listSchedules.FirstOrDefault(s => s.PrintJobID == SelectedPrintJob.JobID);
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

				OpenFileDialog openFileDialog = new();
				openFileDialog.InitialDirectory = @"C:\Alf\Wifi_Coursework\GcodeFiles";
				openFileDialog.Filter = "Gcode files (*.gcode)|*.gcode|All files (*.*)|*.*";

				if (openFileDialog.ShowDialog() == true)
					SelectedPrintJob.GcodeAdresse = openFileDialog.FileName[..25];

				string[] gcodeLines = File.ReadAllLines(openFileDialog.FileName);

				gcodeText = File.ReadAllText(openFileDialog.FileName);

				// first combination - Gcode for a Ultimaker job created with the Cura slicer

				if (gcodeText.Contains("UltiGCode") && gcodeText.Contains("Cura_SteamEngine"))
				{
					string printtimetxt = gcodeLines[1][(gcodeLines[1].IndexOf(":") + 1)..];
					float printtime = (float)Int32.Parse(printtimetxt) / 3600;
					SelectedPrintJob.PrintTime = (double)printtime;

					string nozzletxt = gcodeLines[4][(gcodeLines[4].IndexOf(":") + 1)..];
					float nozzle = float.Parse(nozzletxt, System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.NozzleDiam_mm = (double)nozzle;

					string weightmaterialtxt = gcodeLines[2][(gcodeLines[2].IndexOf(":") + 1)..];
					float weightmaterial = (float)Int32.Parse(weightmaterialtxt) / 1000;
					SelectedPrintJob.WeightMaterial = (double)weightmaterial;

					switch (nozzle) { case (float)0.4: { layerheight = 15; break; } case (float)0.15: { layerheight = 6; break; } default: { layerheight = 15; break; } }
					SelectedPrintJob.LayerHeight = layerheight;

					string volXtxt = gcodeLines[8].Substring(gcodeLines[8].IndexOf(":") + 1);
					float volX = float.Parse(volXtxt, System.Globalization.CultureInfo.InvariantCulture);
					int volx = (int)volX;
					SelectedPrintJob.VolX = volx;

					string volYtxt = gcodeLines[9].Substring(gcodeLines[9].IndexOf(":") + 1);
					float volY = float.Parse(volYtxt, System.Globalization.CultureInfo.InvariantCulture);
					int voly = (int)volY;
					SelectedPrintJob.VolY = voly;

					string volZtxt = gcodeLines[10].Substring(gcodeLines[10].IndexOf(":") + 1);
					float volZ = float.Parse(volZtxt, System.Globalization.CultureInfo.InvariantCulture);
					int volz = (int)volZ;
					SelectedPrintJob.VolZ = volz;

					SelectedPrintJob.PrinterType = 7;
				}

				// Second combination - Gcode for a Prusa Mini job created with the PrusaSlicer slicer 

				else if (gcodeText.Contains("PrusaSlicer") && gcodeText.Contains("MINI"))
				{
					for (int i = 0; i < gcodeLines.Length; i++) if (gcodeLines[i].Contains("filament used [mm]")) prusastart = i;

					string printtimetxt = gcodeLines[prusastart + 6].Substring(gcodeLines[prusastart + 6].IndexOf("= ") + 1);
					string hourstxt = printtimetxt.Substring(0, printtimetxt.IndexOf("h"));
					string minstxt = printtimetxt.Substring(printtimetxt.IndexOf("h")+1, printtimetxt.IndexOf("m") - printtimetxt.IndexOf("h")-1);
					string secstxt = printtimetxt.Substring(printtimetxt.IndexOf("m")+1, printtimetxt.IndexOf("s") - printtimetxt.IndexOf("m")-1);
					SelectedPrintJob.PrintTime = (double)(Int32.Parse(hourstxt) + (float)Int32.Parse(minstxt) / 60 + (float)Int32.Parse(secstxt) / 3600);

					string nozzletxt = gcodeLines[prusastart + 58].Substring(gcodeLines[prusastart + 58].IndexOf("= ") + 1);
					float nozzle = float.Parse(nozzletxt, System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.NozzleDiam_mm = (double)nozzle;

					string weightmaterialtxt = gcodeLines[prusastart + 2].Substring(gcodeLines[prusastart + 2].IndexOf("= ") + 1);
					float weightmaterial = float.Parse(weightmaterialtxt.Trim(), System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.WeightMaterial = (double)weightmaterial;

					string layertxt = gcodeLines[prusastart + 124].Substring(gcodeLines[prusastart + 124].IndexOf("= ") + 1);
					float layerheight = 100*float.Parse(layertxt, System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.LayerHeight = (int)layerheight;

					int volx = 180;
					SelectedPrintJob.VolX = volx;

					int voly = 180;
					SelectedPrintJob.VolY = voly;

					int volz = 180;
					SelectedPrintJob.VolZ = volz;

					SelectedPrintJob.PrinterType = 2;
			}

				// Third combination - Gcode for a Ender-3 job created with the Cura slicer 

				else if (gcodeText.Contains("Marlin") && (gcodeText.Contains("MAXZ:40.8") ||gcodeText.Contains("MAXZ:17")))
				{
					string printtimetxt = gcodeLines[1][(gcodeLines[1].IndexOf(":") + 1)..];
					float printtime = (float)Int32.Parse(printtimetxt) / 3600;
					SelectedPrintJob.PrintTime = (double)printtime;

					SelectedPrintJob.NozzleDiam_mm = 0.4;

					string weightmaterialtxt = gcodeLines[2][(gcodeLines[2].IndexOf(":") + 1)..];
					string weighttxt2 = weightmaterialtxt.Substring(1, weightmaterialtxt.IndexOf(".")-1);
					float weightmaterial = (float)Int32.Parse(weighttxt2);
					SelectedPrintJob.WeightMaterial = (double)weightmaterial;

					string layertxt = gcodeLines[3][(gcodeLines[2].IndexOf(":") + 1)..];
					float layerheight = float.Parse(layertxt, System.Globalization.CultureInfo.InvariantCulture)*100;
					SelectedPrintJob.LayerHeight = (int)layerheight;

					string volXtxt = gcodeLines[7].Substring(gcodeLines[7].IndexOf(":") + 1);
					float volX = float.Parse(volXtxt, System.Globalization.CultureInfo.InvariantCulture);
					int volx = (int)volX;
					SelectedPrintJob.VolX = volx;

					string volYtxt = gcodeLines[8].Substring(gcodeLines[8].IndexOf(":") + 1);
					float volY = float.Parse(volYtxt, System.Globalization.CultureInfo.InvariantCulture);
					int voly = (int)volY;
					SelectedPrintJob.VolY = voly;

					string volZtxt = gcodeLines[9].Substring(gcodeLines[9].IndexOf(":") + 1);
					float volZ = float.Parse(volZtxt, System.Globalization.CultureInfo.InvariantCulture);
					int volz = (int)volZ;
					SelectedPrintJob.VolZ = volz;

				if (gcodeText.Contains("MAXZ:17"))
					SelectedPrintJob.PrinterType = 3;
				else SelectedPrintJob.PrinterType = 4;
			}
				else throw new Exception("This printer has not yet been included");
			}        
    }
}




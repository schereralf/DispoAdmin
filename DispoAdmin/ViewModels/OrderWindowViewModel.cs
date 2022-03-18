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
        {    // 
            get { return _order; }
            set
            {
                _order = value;
            }
        }
        private ObservableCollection<PrintJob> _listPrintJobs;
        public IList<PrintJob> ListPrintJobs => _listPrintJobs;

        private PrintJob _selectedPrintJob;
        private string gcodeText;
        private string gcodeName;
        private int layerheight;
        private int prusastart;
        public PrintJob SelectedPrintJob
        {

			//ParsePrintJob contains code for extracting data from Gcode files

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
			using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
			{
				
				// Window for selecting gcode files for data extraction
				// Three slicer+printer combinations possible now
				// 1. Cura+Ultimaker2
				// 2. PrusaSlicer and Prusa Mini
				// 3. PrusaSlicer and Ender 3

				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.InitialDirectory = @"C:\Alf\Wifi_Coursework\GcodeFiles";
				openFileDialog.Filter = "Gcode files (*.gcode)|*.gcode|All files (*.*)|*.*";

				if (openFileDialog.ShowDialog() == true)
					SelectedPrintJob.GcodeAdresse = openFileDialog.FileName;

				string[] gcodeLines = File.ReadAllLines(openFileDialog.FileName);

				gcodeText = File.ReadAllText(openFileDialog.FileName);
				gcodeName = openFileDialog.FileName;


				// first combination

				if (gcodeText.Contains("UltiGCode") && gcodeText.Contains("Cura_SteamEngine"))
				{
					string printtimetxt = gcodeLines[1].Substring(gcodeLines[1].IndexOf(":") + 1);
					float printtime = (float)Int32.Parse(printtimetxt) / 3600;
					SelectedPrintJob.PrintTime = (double)printtime;

					string nozzletxt = gcodeLines[4].Substring(gcodeLines[4].IndexOf(":") + 1);
					float nozzle = float.Parse(nozzletxt, System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.NozzleDiam_mm = (int)nozzle;

					string weightmaterialtxt = gcodeLines[2].Substring(gcodeLines[2].IndexOf(":") + 1);
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
				}

				// Second combination

				if (gcodeText.Contains("PrusaSlicer") && gcodeText.Contains("MINI"))
				{
					for (int i = 0; i < gcodeLines.Length; i++) if (gcodeLines[i].Contains("filament used [mm]")) prusastart = i;

					string printtimetxt = gcodeLines[prusastart + 6].Substring(gcodeLines[prusastart + 6].IndexOf("= ") + 1);
					string hourstxt = printtimetxt.Substring(0, printtimetxt.IndexOf("h"));
					string minstxt = printtimetxt.Substring(printtimetxt.IndexOf("h"), printtimetxt.IndexOf("m") - printtimetxt.IndexOf("h"));
					string secstxt = printtimetxt.Substring(printtimetxt.IndexOf("m"), printtimetxt.IndexOf("s") - printtimetxt.IndexOf("m"));
					SelectedPrintJob.PrintTime = (double)(Int32.Parse(hourstxt) + (float)Int32.Parse(minstxt) / 60 + (float)Int32.Parse(minstxt) / 3600);

					string nozzletxt = gcodeLines[prusastart + 58].Substring(gcodeLines[prusastart + 58].IndexOf("= ") + 1);
					float nozzle = float.Parse(nozzletxt, System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.NozzleDiam_mm = (int)nozzle;

					string weightmaterialtxt = gcodeLines[prusastart + 2].Substring(gcodeLines[prusastart + 2].IndexOf("= ") + 1);
					float weightmaterial = float.Parse(weightmaterialtxt.Trim(), System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.WeightMaterial = (double)weightmaterial;

					string layertxt = gcodeLines[prusastart + 124].Substring(gcodeLines[prusastart + 124].IndexOf("= ") + 1);
					float layerheight = float.Parse(layertxt, System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.LayerHeight = (int)layerheight;

					int volx = 180;
					SelectedPrintJob.VolX = volx;

					int voly = 180;
					SelectedPrintJob.VolY = voly;

					int volz = 180;
					SelectedPrintJob.VolZ = volz;
				}

				// Third combination

				if (gcodeText.Contains("PrusaSlicer") && gcodeText.Contains("ENDER3"))
				{
					for (int i = 0; i < gcodeLines.Length; i++) if (gcodeLines[i].Contains("filament used [mm]")) prusastart = i;

					string printtimetxt = gcodeLines[prusastart + 6].Substring(gcodeLines[prusastart + 6].IndexOf("= ") + 1);
					string hourstxt = printtimetxt.Substring(0, printtimetxt.IndexOf("h"));
					string minstxt = printtimetxt.Substring(printtimetxt.IndexOf("h"), printtimetxt.IndexOf("m") - printtimetxt.IndexOf("h"));
					string secstxt = printtimetxt.Substring(printtimetxt.IndexOf("m"), printtimetxt.IndexOf("s") - printtimetxt.IndexOf("m"));
					SelectedPrintJob.PrintTime = (double)(Int32.Parse(hourstxt) + (float)Int32.Parse(minstxt) / 60 + (float)Int32.Parse(minstxt) / 3600);

					string nozzletxt = gcodeLines[prusastart + 58].Substring(gcodeLines[prusastart + 58].IndexOf("= ") + 1);
					float nozzle = float.Parse(nozzletxt, System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.NozzleDiam_mm = (int)nozzle;

					string weightmaterialtxt = gcodeLines[prusastart + 2].Substring(gcodeLines[prusastart + 2].IndexOf("= ") + 1);
					float weightmaterial = float.Parse(weightmaterialtxt.Trim(), System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.WeightMaterial = (double)weightmaterial;

					string layertxt = gcodeLines[prusastart + 124].Substring(gcodeLines[prusastart + 124].IndexOf("= ") + 1);
					float layerheight = float.Parse(layertxt, System.Globalization.CultureInfo.InvariantCulture);
					SelectedPrintJob.LayerHeight = (int)layerheight;

					int volx = 228;
					SelectedPrintJob.VolX = volx;

					int voly = 228;
					SelectedPrintJob.VolY = voly;

					int volz = 228;
					SelectedPrintJob.VolZ = volz;
				}
			}
        }        
    }
}




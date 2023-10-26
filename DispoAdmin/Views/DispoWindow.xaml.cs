using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DispoAdmin.Models;
using System.Windows.Media;
using Model3DFarm;
using System.Reflection;
using System.Collections.ObjectModel;

namespace DispoAdmin.Views
{
    /// <summary>
    /// Interaction logic für DispoWindow.xaml
    /// </summary>
    public partial class DispoWindow : Window
    {
        public int ScheduleWeek;

        private ObservableCollection<Schedule> _listSchedules;
        public IList<Schedule> ListSchedules => _listSchedules;

        List<Printer> ListPrinters= new ();

        private Schedule _selectedSchedule;

        public Schedule SelectedSchedule
        {
            get { return _selectedSchedule; }
            set
            {
                _selectedSchedule = value;
            }
        }

        List<Button> Controls = new ();
        private int jobPosition;
        private int jobPositionWeek;
        private int jobCanStart;
        private int jobDeadline;

        public DispoWindow(int scheduleWeek)
        {
            InitializeComponent();

            this.ScheduleWeek = scheduleWeek;
            DateTime dayDateStart = new (2022, 1, 3);

            int NewDate = (scheduleWeek - 1) * 7;

            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                ListPrinters.Clear();

                _listSchedules = new ObservableCollection<Schedule>();

                foreach (Printer k in context.Printers) ListPrinters.Add(k);

                bool [,] annSchedule = new bool[8736, ListPrinters.Count];
                for (int i = 0; i < 8736; i++) { for (int j = 0; j < ListPrinters.Count;j++) { annSchedule[i, j] = true; } }

                for (int i = 0; i < ListPrinters.Count; i++)
                {
                    TextBox t = new ();
                    t.Text = ListPrinters[i].PrinterType;
                    Grid.SetRow(t, i * 2+1);
                    Grid.SetColumnSpan(t, 167);
                    t.Background = PickBrush(3);
                    t.HorizontalAlignment = HorizontalAlignment.Left;
                    t.Width = 2000;
                    Dispogrid.Children.Add(t);
                }

                    var result = from k in context.Schedules
                             where k.ScheduleWeek == this.ScheduleWeek
                             orderby k.TimeStart
                             select k;

                foreach (Schedule k in result)
                {
                    ListSchedules.Add(k);
                }

                Random rnd = new Random();
                jobPosition = 0;

                for (int i = 0; i < ListSchedules.Count; i++)
                {
                    ExtendedButton b = new();
                    // unique MessageBox used here operates with ExtendedButton

                    b._myval = ListSchedules[i].PrintJob.JobName.ToString() + " " + ListSchedules[i].PrintJob.Order.OrderName.ToString() + " "
                        + ListSchedules[i].PrintJob.Material.ToString();
                    //this assigns some job details to the extended button

                    b.Click += new RoutedEventHandler(OnButtonClick);

                    List<int> availableUnits=new();
                    int UnitUsed = 0;
                    foreach (Printer p in ListPrinters) { UnitUsed++; if (p.PrinterID == ListSchedules[i].PrintJob.PrinterType) availableUnits.Add(UnitUsed); } 

                    //Start Test Section
                    int jobRun = (int)Math.Ceiling((decimal)ListSchedules[i].MR_Time + (decimal)ListSchedules[i].RO_Time);
                    TimeSpan startTime = (TimeSpan)(ListSchedules[i].TimeStart - new DateTime(2022, 1, 1));
                    TimeSpan endTime = (TimeSpan)(ListSchedules[i].TimeEnd - new DateTime(2022, 1, 1));

                    int mindays = (int)startTime.TotalDays - 1;
                    int maxdays = (int)endTime.TotalDays - 1;

                    DateTime schedHour = (DateTime)ListSchedules[i].TimeStart;
                    int minhours = (int)schedHour.TimeOfDay.TotalHours;

                    jobCanStart = mindays * 24 + minhours;
                    jobPosition = jobCanStart;

                    DateTime deadlineHour = (DateTime)ListSchedules[i].TimeEnd;
                    int maxhours = (int)deadlineHour.TimeOfDay.TotalHours;
                    jobDeadline = maxdays * 24 + maxhours;
                    int printerRow = 0;

                    bool isfree = false;

                    while ((printerRow<availableUnits.Count)&&(!isfree))
                    {
                        if (!isfree)
                        {
                            isfree = CheckCapacity(jobRun, jobPosition, availableUnits);
                            jobPosition++;
                        }
                        if(jobPosition+jobRun>jobDeadline)
                        { 
                            jobPosition = jobCanStart;
                            printerRow++;
                        }
                    }

                // After capacity has been located, mark this capacity as booked in the annual schedule table for the year.
                // Booking row is id number of the assigned unit in the available units.

                    if (isfree) 
                    {
                        int spec = availableUnits[printerRow];
                        for (int m = 0; m < jobRun; m++) annSchedule[jobPosition + m, spec] = false;
                    }

                //TODO:  add the exception for running out of the total available printer capacity of the required printer category in here

                    bool CheckCapacity(int jobRun, int jobBegin, List<int> WorkingRows)
                    {
                        bool check= true;
                        UnitUsed = WorkingRows[printerRow];
                        for (int l = 1; l < jobRun; l++)
                        {
                            bool bot = annSchedule[jobBegin + l, UnitUsed];
                            if (!bot) check= false;
                        }
                        return check;
                    }

                    //Bedore displaying on a weekly schedule, translate the job position (in hours) into the hour count for the selected week
                    jobPositionWeek = jobPosition - (int)ListSchedules[i].ScheduleWeek * 7 * 24;

                    this.Controls.Add(b);
                    DeButnPlace(b, i, UnitUsed*2, jobRun, jobPositionWeek);
                    Dispogrid.Children.Add(b);
                }

                context.SaveChanges();

                void DeButnPlace(Button thisButton, int j, int row, int jobRun, int jobPosition)
                {
                    Grid.SetRow(thisButton, row);
                    Grid.SetColumn(thisButton, jobPosition);
                    Grid.SetColumnSpan(thisButton, jobRun);
                    thisButton.Background = PickBrush(rnd.Next(20));
                    thisButton.Content = ListSchedules[j].PrintJob.JobName;
                }

                void OnButtonClick(object sender, EventArgs e)
                {
                    string jobName = ((ExtendedButton)sender)._myval;
                    MessageBox.Show(jobName);
                }

                Brush PickBrush(int random)
                {
                    Brush result = Brushes.Transparent;
                    Type brushesType = typeof(Brushes);
                    PropertyInfo[] properties = brushesType.GetProperties();
                    result = (Brush)properties[random].GetValue(null, null);
                    return result;
                }
            }
    }
        class ExtendedButton : Button //this class inherits from button
        {
            //constructor
            public ExtendedButton()
            { }

            private string myval;
            public string _myval
            {
                get { return myval; }
                set { myval = value; }
            }
        }
    }
}

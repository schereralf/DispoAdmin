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
using System.Globalization;

namespace DispoAdmin.Views
{
    // Interaction logic für DispoWindow.xaml
    public partial class DispoWindow : Window
    {
        public int ScheduleWeek;

        private readonly ObservableCollection<Schedule> _listSchedules;
        public IList<Schedule> ListSchedules => _listSchedules;

        readonly List<Printer> ListPrinters= [];

        private Schedule _selectedSchedule;

        public Schedule SelectedSchedule
        {
            get { return _selectedSchedule; }
            set
            {
                _selectedSchedule = value;
            }
        }

        public string [] weekdays = new string[7];

        readonly List<Button> Controls = [];
        private readonly int jobPosition;
        private readonly int jobPositionWeek;
        private readonly int jobCanStart;
        private readonly int jobDeadline;

        public DispoWindow(int scheduleWeek)
        {
            InitializeComponent();

            this.ScheduleWeek = scheduleWeek;
            DateTime dayDateStart = new (2022, 1, 3);

            int NewDate = (scheduleWeek - 1) * 7;

            weekdays[0] = "Monday " + dayDateStart.AddDays(NewDate).ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
            weekdays[1] = "Tuesday " + dayDateStart.AddDays(NewDate + 1).ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
            weekdays[2] = "Wednesday " + dayDateStart.AddDays(NewDate + 2).ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
            weekdays[3] = "Thursday " + dayDateStart.AddDays(NewDate + 3).ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
            weekdays[4] = "Friday " + dayDateStart.AddDays(NewDate + 4).ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
            weekdays[5] = "Saturday " + dayDateStart.AddDays(NewDate + 5).ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
            weekdays[6] = "Sunday " + dayDateStart.AddDays(NewDate + 6).ToString("dd/M/yyyy", CultureInfo.InvariantCulture);

            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                ListPrinters.Clear();

                _listSchedules = [];

                foreach (Printer k in context.Printers) ListPrinters.Add(k);

                bool [,] annSchedule = new bool[8736, ListPrinters.Count];

                for (int i = 0; i < 8736; i++) { for (int j = 0; j < ListPrinters.Count;j++) { annSchedule[i, j] = true; } }
                
                for (int i = 0; i < 7; i++)
                {
                    TextBox t = new()
                    {
                        Text = weekdays[i]
                    };
                    Grid.SetRow(t,0);
                    Grid.SetColumn(t, 24 * i);
                    Grid.SetColumnSpan(t, 24);
                    t.Background = PickBrush(4);
                    t.HorizontalAlignment = HorizontalAlignment.Left;
                    t.Width = 400;
                    Dispogrid.Children.Add(t);
                }

                for (int i = 0; i < ListPrinters.Count; i++)
                {
                    TextBox t = new()
                    {
                        Text = ListPrinters[i].PrinterType
                    };
                    Grid.SetRow(t, i * 2+1);
                    Grid.SetColumnSpan(t, 168);
                    t.Background = PickBrush(3);
                    t.HorizontalAlignment = HorizontalAlignment.Left;
                    t.Width = 2100;
                    Dispogrid.Children.Add(t);
                }

                    var result = from k in context.Schedules
                             where k.TimeStart>dayDateStart
                             orderby k.TimeStart
                             select k;

                foreach (Schedule k in result)
                {
                    ListSchedules.Add(k);
                }

                Random rnd = new();
                jobPosition = 0;

                for (int i = 0; i < ListSchedules.Count; i++)
                {
                    ExtendedButton b = new();

                    // to spawn the job elements on the scheduling grid, a  unique MessageBox used here operates with ExtendedButton

                    int jobRun = (int)Math.Ceiling((decimal)ListSchedules[i].MR_Time + (decimal)ListSchedules[i].RO_Time);
                    b.Myval = "Name of this job : " + ListSchedules[i].PrintJob.JobName.ToString() + "\nName of the order: " + ListSchedules[i].PrintJob.Order.OrderName.ToString() + "\nMaterial used: "
                        + ListSchedules[i].PrintJob.Material.ToString() + "\nPrinting time: " + jobRun.ToString() + " hours";

                    //this assigns some job details to the extended button

                    b.Click += new RoutedEventHandler(OnButtonClick);

                    List<int> availableUnits=[];
                    int UnitUsed = 0;
                    foreach (Printer p in ListPrinters) 
                    {
                        UnitUsed++;
                        if (p.PrinterType == ListSchedules[i].Printer.PrinterType) 
                            availableUnits.Add(UnitUsed); 
                    } 

                    //Start Test Section

                    TimeSpan startTime = (TimeSpan)(ListSchedules[i].TimeStart - dayDateStart);
                    TimeSpan endTime = (TimeSpan)(ListSchedules[i].TimeEnd - dayDateStart);

                    int mindays = (int)startTime.TotalDays+1;
                    int maxdays = (int)endTime.TotalDays;

                    DateTime schedHour = (DateTime)ListSchedules[i].TimeStart;
                    int minhours = (int)schedHour.TimeOfDay.TotalHours;

                    jobCanStart = mindays * 24 + minhours+1;
                    jobPosition = jobCanStart;

                    DateTime deadlineHour = (DateTime)ListSchedules[i].TimeEnd;
                    int maxhours = (int)deadlineHour.TimeOfDay.TotalHours;
                    jobDeadline = maxdays * 24 + maxhours;
                    int printerRow = 0;

                    // Go through the schedule on your first choice printer of the required type for the job, check for capacity.
                    // If none is found, move on to the next available printer of the required type, else give an error message
                    // something like "ALERT: job X cannot be scheduled"

                    bool isfree = false;

                    while ((printerRow<availableUnits.Count)&&(!isfree)&&(jobCanStart<jobDeadline))
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

                    //Before displaying on a weekly schedule, translate the job position (in hours) as an hour count inside the selected week.
                    //Need to first check for the holdover parts of runs that started during the previous week.


                    int startofthisweek = (ScheduleWeek-1) * 7 * 24;
                    jobPositionWeek = jobPosition - startofthisweek;
                    if (jobPositionWeek + jobRun > 0 && jobPositionWeek < 0)
                    {
                        jobRun += jobPositionWeek;
                        jobPositionWeek = 0;
                        this.Controls.Add(b);
                        DeButnPlace(b, i, UnitUsed * 2, jobRun, jobPositionWeek);
                        Dispogrid.Children.Add(b);
                    }
                    else if (jobPositionWeek > 0 && jobPositionWeek < 168 && isfree)
                    {
                        this.Controls.Add(b);
                        DeButnPlace(b, i, UnitUsed * 2, jobRun, jobPositionWeek);
                        Dispogrid.Children.Add(b);
                    }
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
                    string jobName = ((ExtendedButton)sender).Myval;
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
            public ExtendedButton()
            { }

            private string myval;
            public string Myval
            {
                get { return myval; }
                set { myval = value; }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DispoAdmin.Models;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Model3DFarm;
using DispoAdmin.ViewModels;
using DispoAdmin.Scheduler;
using System.Reflection;
using System.Collections.ObjectModel;

namespace DispoAdmin.Views
{
    /// <summary>
    /// Interaktionslogik für DispoWindow.xaml
    /// </summary>
    public partial class DispoWindow : Window
    {
        public int ScheduleWeek;

        private ObservableCollection<Schedule> _listSchedules;
        public IList<Schedule> ListSchedules => _listSchedules;

        private Schedule _selectedSchedule;

        public Schedule SelectedSchedule
        {
            get { return _selectedSchedule; }
            set
            {
                _selectedSchedule = value;
            }
        }

        List<Button> Controls = new List<Button>();
        private int jobPosition;

        public DispoWindow(int scheduleWeek)
        {
            InitializeComponent();

            this.ScheduleWeek = scheduleWeek;

            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                _listSchedules = new ObservableCollection<Schedule>();

                var result = from k in context.Schedules//.Include(k => k.ScheduleWeek)
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
                    ExtendedButton b = new ExtendedButton();
                    // unique MessageBox used here operates with ExtendedButton
                    b._myval = ListSchedules[i].PrintJob.JobName.ToString() + " " + ListSchedules[i].PrintJob.JobOrder.ToString() + " " + ListSchedules[i].PrintJob.Material.ToString();
                    //this assigns some job details to the extended button

                    b.Click += new RoutedEventHandler(OnButtonClick);
                    this.Controls.Add(b);
                    DeButnPlace(b, i, 2);
                    Dispogrid.Children.Add(b);
                }

                context.SaveChanges();

                void DeButnPlace(Button thisButton, int j, int row)
                {
                    int jobRun = (int)Math.Ceiling((decimal)ListSchedules[j].MR_Time + (decimal)ListSchedules[j].RO_Time);

                    TimeSpan startTime = (TimeSpan)(ListSchedules[j].TimeStart - new DateTime(2022, 1, 1));
                    int days = (int)startTime.TotalDays - 1;
                    DateTime schedHour = (DateTime)ListSchedules[j].TimeStart;
                    int hours = (int)schedHour.TimeOfDay.TotalHours;

                    jobPosition = (days - ((int)ListSchedules[j].ScheduleWeek - 1) * 7) * 24 + hours;

                    Grid.SetRow(thisButton, row);
                    Grid.SetColumn(thisButton, jobPosition);
                    Grid.SetColumnSpan(thisButton, jobRun);
                    thisButton.Background = PickBrush(rnd.Next(20));
                    thisButton.Content = ListSchedules[j].PrintJob.JobName;
                    // job position is calculated by optimization routine based on length, dimensions, material, 
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

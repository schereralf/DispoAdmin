using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model3DFarm;
using DispoBaseLib;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DispoAdmin.Models;
using DispoAdmin.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows.Media;
using System.Reflection;
using System.Windows.Controls;
using System.Windows;

namespace DispoAdmin.ViewModels
{
    class DispoWindowViewModel : BaseViewModel
    {
        private int _scheduleWeek;
        public int ScheduleWeek
        { 
            get { return _scheduleWeek; }        
            set
            {
                _scheduleWeek = value;
                OnPropertyChanged();
            }                
        }

        
        private ObservableCollection<Schedule> _listSchedules;
        public IList<Schedule> ListSchedules => _listSchedules;

        private Schedule _selectedSchedule;

        public Schedule SelectedSchedule
        {
            get { return _selectedSchedule; }
            set
            {
                _selectedSchedule = value;
                OnPropertyChanged();
            }
        }

        private int jobPosition;

        List<Button> Controls = new List<Button>();

        private readonly Grid Dispogrid;

        public DispoWindowViewModel(int scheduleWeek)
        {
            this.ScheduleWeek = scheduleWeek;

            _listSchedules = new ObservableCollection<Schedule>();
            LoadSchedule();

            Random rnd = new Random();

            jobPosition = 0;

            for (int i = 0; i < ListSchedules.Count; i++)
            {
                ExtendedButton b = new ExtendedButton(); // unique MessageBox used here only works with ExtendedButton
                b._myval = ListSchedules[i].PrintJob.JobName.ToString() + " " + ListSchedules[i].PrintJob.JobOrder.ToString() + " " + ListSchedules[i].PrintJob.Material.ToString();//this assigns some job details to the extended button
                b.Click += new RoutedEventHandler(OnButtonClick);
                this.Controls.Add(b);
                DeButnPlace(b, i, 1);
                Dispogrid.Children.Add(b);
            }

            void DeButnPlace(Button thisButton, int j, int row)
            {
                int jobRun = (int)Math.Ceiling((decimal)ListSchedules[j].MR_Time + (decimal)ListSchedules[j].RO_Time);

                TimeSpan startTime = (TimeSpan)(ListSchedules[j].TimeStart - new DateTime(2022, 1, 1));
                int days = (int)startTime.TotalDays - 2;
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

        public void LoadSchedule()
        {
            ListSchedules.Clear();    // clear existing jobs schedule
            using (PrinterfarmContext context = DispoAdminModel.Default.GetDBContext())
            {
                var result = from k in context.Schedules.Include(k => k.ScheduleWeek) orderby k.TimeStart select k;
                foreach (Schedule k in result)
                {
                    ListSchedules.Add(k);
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
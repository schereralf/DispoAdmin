using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using DispoAdmin.ViewModels;
using Model3DFarm;
using DispoBaseLib;
using System.Windows.Data;

namespace DispoAdmin.Scheduler
{
    class MonToFri
    {
        //First attempt at some more scheduling logic.
        public void DateLocation(int scheduleWeek)
        {
            DateTime dayDateStart = new DateTime(2022, 1, 3);

            int NewDate = (scheduleWeek - 1) * 7;

            DateTime dayDateMon = dayDateStart.AddDays(NewDate);
            DateTime dayDateTue = dayDateStart.AddDays(NewDate + 1);
            DateTime dayDateWed = dayDateStart.AddDays(NewDate + 3);
            DateTime dayDateThu = dayDateStart.AddDays(NewDate + 4);
            DateTime dayDateFri = dayDateStart.AddDays(NewDate + 5);
            DateTime dayDateSat = dayDateStart.AddDays(NewDate + 6);
            DateTime dayDateSun = dayDateStart.AddDays(NewDate + 7);
        }
    }
}


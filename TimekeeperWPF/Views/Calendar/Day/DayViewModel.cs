using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TimekeeperDAL.EF;
using TimekeeperWPF.Calendar;

namespace TimekeeperWPF
{
    public class DayViewModel : CalendarViewModel
    {
        public override string Name => "Day View";
        public DayViewModel() : base()
        {
        }
        #region Predicates
        protected override bool CanSave => false;
        protected override bool CanSelectDay => false;
        #endregion
        #region Actions
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        protected override void SetUpCalendarObjects()
        {
            CalendarObjectsCollection = new CollectionViewSource();
            CalendarObjectsCollection.Source = new ObservableCollection<UIElement>()
            {
                new CalendarObject()
                {
                    Start = DateTime.Now.Date.AddHours(2).AddMinutes(20),
                    End = DateTime.Now.Date.AddHours(4),
                },
                new CalendarObject()
                {
                    Start = DateTime.Now.Date.AddMinutes(10),
                    End = DateTime.Now.Date.AddHours(1).AddMinutes(20),
                },
            };
            View.Filter = T =>
            {
                TimeTask task = T as TimeTask;
                return task.Start.Date == SelectedDate.Date
                    || task.End.Date == SelectedDate.Date;
            };
            OnPropertyChanged(nameof(View));
            foreach (TimeTask T in View)
            {
                //Here we want to create CalendarObjects based on the selected TimeTask
                CalendarObject CalObj = new CalendarObject();
                //edit CalObj properties
                CalendarObjectsView.AddNewItem(CalObj);
            }
            OnPropertyChanged(nameof(CalendarObjectsView));
        }
        #endregion
    }
}

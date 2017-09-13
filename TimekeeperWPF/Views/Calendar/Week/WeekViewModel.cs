using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;
using TimekeeperWPF.Calendar;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;

namespace TimekeeperWPF
{
    public class WeekViewModel : CalendarViewModel
    {
        #region Fields
        private Orientation _WeekOrientation = Orientation.Horizontal;
        private ICommand _WeekOrientationCommand;
        #endregion

        #region Properties
        public override string Name => "Week View";
        public Orientation WeekOrientation
        {
            get { return _WeekOrientation; }
            set
            {
                if (_WeekOrientation == value) return;
                _WeekOrientation = value;
                OnPropertyChanged();
            }
        }
        public override DateTime SelectedDate
        {
            get => base.SelectedDate;
            set
            {
                //Here we intercept and set the date to the first of the week
                base.SelectedDate = value.Date.AddDays(-(int)value.DayOfWeek);
            }
        }
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        protected override bool CanMax => true;
        #endregion
        #region Commands
        public ICommand WeekOrientationCommand => _WeekOrientationCommand
            ?? (_WeekOrientationCommand = new RelayCommand(ap => ToggleWeekOrientation(), pp => CanWeekOrientation));
        #endregion
        #region Predicates
        protected override bool CanOrientation => true;
        protected virtual bool CanWeekOrientation => true;
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
                //assume that CalObj start and/or end is within week
                CalendarObjectsView.AddNewItem(CalObj);
                KageBunshinNoJutsu(CalObj);
            }
            OnPropertyChanged(nameof(CalendarObjectsView));
        }

        private void KageBunshinNoJutsu(CalendarObject CalObj)
        {
            if (CalObj.Start.Date != CalObj.End.Date)
            {
                //CalObj covers more than one day, so we need to make copies and set
                //the DayOffset property to help Week panel, otherwise it will not be
                //displayed properly and only show up on one day.
                //First get number of days this CalObj covers in this week up to 7
                int startDayOfWeek = Math.Max(0, Math.Min((int)(CalObj.Start.Date - SelectedDate.WeekStart()).TotalDays, 6));
                int endDayOfWeek = Math.Max(0, Math.Min((int)(CalObj.End.Date - SelectedDate.WeekStart()).TotalDays, 6));
                int numExtraDays = endDayOfWeek - startDayOfWeek;
                CalObj.DayOffset = 0;
                for (int i = 1; i <= numExtraDays; i++)
                {
                    CalendarObject shadowClone = CalObj.ShadowClone();
                    shadowClone.DayOffset = i;
                    CalendarObjectsView.AddNewItem(shadowClone);
                }
            }
        }

        protected override void ToggleMaxScale()
        {
            //instead of maxing days, (days are permanently maxed), 
            //max the week to the viewport
        }
        protected override void ScaleUp()
        {
            //instead of scaling days, just stretch the week grid
        }
        protected override void ScaleDown()
        {

        }
        protected virtual void ToggleWeekOrientation()
        {
            if (WeekOrientation == Orientation.Horizontal)
                WeekOrientation = Orientation.Vertical;
            else
                WeekOrientation = Orientation.Horizontal;
        }
        #endregion
    }
}

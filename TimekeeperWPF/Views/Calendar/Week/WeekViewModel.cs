// Copyright 2017 (C) Cody Neuburger  All rights reserved.
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
        private int _SelectedMonthOverride = DateTime.Now.Month;
        public WeekViewModel() : base()
        {
            SelectedDate = DateTime.Now;
        }
        #region Properties
        public override string Name => "Week View";
        public override DateTime SelectedDate
        {
            get => base.SelectedDate;
            set
            {
                //Here we intercept and set the date to the first of the week
                base.SelectedDate = value.WeekStart();
                SelectedMonthOverride = value.Month;
            }
        }
        public int SelectedMonthOverride
        {
            get { return _SelectedMonthOverride; }
            set
            {
                if (_SelectedMonthOverride == value) return;
                _SelectedMonthOverride = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        public override bool CanMax => true;
        protected override bool CanOrientation => true;
        protected override bool CanSelectWeek => false;
        #endregion
        #region Actions
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        protected override void AdditionalCalObjSetup(CalendarObject CalObj)
        {
            KageBunshinNoJutsu(CalObj);
        }
        private void KageBunshinNoJutsu(CalendarObject CalObj)
        {
            if (CalObj.Start.Date != CalObj.End.Date)
            {
                //CalObj covers more than one day, so we need to make copies and set
                //the DayOffset property to help Week panel, otherwise it will not be
                //displayed properly and only show up on one day.
                int startDayOfWeek = (int)(CalObj.Start.Date - SelectedDate.WeekStart()).TotalDays.Within(0, 6);
                int endDayOfWeek = (int)(CalObj.End.Date - SelectedDate.WeekStart()).TotalDays.Within(0, 6);
                int numExtraDays = endDayOfWeek - startDayOfWeek;
                CalObj.DayOffset = 0;
                for (int i = 1; i <= numExtraDays; i++)
                {
                    CalendarObject shadowClone = CalObj.ShadowClone();
                    shadowClone.DayOffset = i;
                    CalendarObjectsView.AddNewItem(shadowClone);
                    CalendarObjectsView.CommitNew();
                }
            }
        }
        protected override void Previous()
        {
            SelectedDate = SelectedDate.AddDays(-7);
            base.Previous();
        }
        protected override void Next()
        {
            SelectedDate = SelectedDate.AddDays(7);
            base.Next();
        }
        protected override bool IsNoteRelevant(Note note)
        {
            return note.DateTime.WeekStart() == SelectedDate.WeekStart();
        }
        protected override bool IsTaskRelevant(TimeTask task)
        {
            return task.Start.WeekStart() == SelectedDate.WeekStart()
                || task.End.WeekStart() == SelectedDate.WeekStart();
        }
        #endregion
    }
}

// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using TimekeeperDAL.EF;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows.Input;
using TimekeeperWPF.Tools;
using TimekeeperWPF.Calendar;
using System.Windows.Data;
using System.Windows;
using TimekeeperDAL.Tools;

namespace TimekeeperWPF
{
    public class MonthViewModel : CalendarViewModel
    {
        public override string Name => "Month View";
        public override DateTime Start
        {
            get => base.Start;
            set { base.Start = value.MonthStart(); }
        }
        public override DateTime End
        {
            get => Start.AddMonths(1);
            set { }
        }
        protected override bool CanSave => false;
        public override bool CanMax => false;
        protected override bool CanOrientation => false;
        protected override bool CanSelectMonth => false;
        internal override void SaveAs()
        {
            throw new NotImplementedException();
        }
        protected override void AdditionalCalTaskObjSetup(CalendarTaskObject CalObj)
        {
            ShadowClone(CalObj);
        }
        private void ShadowClone(CalendarTaskObject CalObj)
        {
            if (CalObj.Start.Date != CalObj.End.Date)
            {
                //CalObj covers more than one day, so we need to make copies and set
                //the DayOffset property to help Week panel, otherwise it will not be
                //displayed properly and only show up on one day.
                int startDayOfMonth = (int)(CalObj.Start.Date - Start).TotalDays.Within(0, Start.MonthDays() - 1);
                int endDayOfMonth = (int)(CalObj.End.Date - Start).TotalDays.Within(0, Start.MonthDays() - 1);
                int numExtraDays = endDayOfMonth - startDayOfMonth;
                CalObj.DayOffset = 0;
                for (int i = 1; i <= numExtraDays; i++)
                {
                    CalendarTaskObject shadowClone = CalObj.ShadowClone();
                    shadowClone.DayOffset = i;
                    CalTaskObjsView.AddNewItem(shadowClone);
                    CalTaskObjsView.CommitNew();
                }
            }
        }
        protected override async Task PreviousAsync()
        {
            Start = Start.AddMonths(-1);
            await base.PreviousAsync();
        }
        protected override async Task NextAsync()
        {
            Start = Start.AddMonths(1);
            await base.NextAsync();
        }
    }
}

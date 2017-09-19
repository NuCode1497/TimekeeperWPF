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
            base.SetUpCalendarObjects();
            foreach (TimeTask T in View)
            {
                //Here we want to create CalendarObjects based on the selected TimeTask
                CalendarObject CalObj = new CalendarObject();
                CalObj.TimeTask = T;
                //edit CalObj properties

                //TimeTask
                //Start
                //End
                //Priority
                //RaiseOnReschedule
                //AsksForReschedule
                //AsksForCheckin
                //CanBePushed
                //CanInflate
                //CanDeflate
                //CanFill
                //CanBeEarly
                //CanBeLate
                //Dimension
                //PowerLevel
                //Labels
                //Excludes
                //Includes

                //TimePatern
                //Duration
                //ForX
                //ForNth
                //ForSkipDuration
                //Labels
                //Allocations

                //Allocation
                //min
                //max
                //Resource

                //CalendarObjectsView.AddNewItem(CalObj);
                //CalendarObjectsView.CommitNew();
            }
            OnPropertyChanged(nameof(CalendarObjectsView));
        }
        protected override bool IsTaskRelevant(TimeTask task)
        {
            return task.Start.Date == SelectedDate.Date
                || task.End.Date == SelectedDate.Date;
        }
        #endregion
    }
}

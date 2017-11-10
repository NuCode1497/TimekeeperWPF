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
        #region Fields
        #endregion
        public DayViewModel() : base()
        {
        }
        #region Properties
        public override string Name => "Day View";
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        protected override bool CanSelectDay => false;

        public override DateTime EndDate => SelectedDate.AddDays(1);
        #endregion
        #region Actions
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        protected override void Previous()
        {
            SelectedDate = SelectedDate.AddDays(-1);
            base.Previous();
        }
        protected override void Next()
        {
            SelectedDate = SelectedDate.AddDays(1);
            base.Next();
        }
        #endregion
    }
}

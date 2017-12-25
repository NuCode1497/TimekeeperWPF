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
        protected override bool CanSave => false;
        protected override bool CanSelectDay => false;
        public override DateTime End
        {
            get
            {
                return Start.AddDays(1);
            }
            set
            {
            }
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        protected override async Task PreviousAsync()
        {
            Start = Start.AddDays(-1);
            await base.PreviousAsync();
        }
        protected override async Task NextAsync()
        {
            Start = Start.AddDays(1);
            await base.NextAsync();
        }
    }
}

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

namespace TimekeeperWPF
{
    public abstract class CalendarViewModel : ViewModel<TimeTask>
    {
        #region Properties
        public ObservableCollection<CalendarObject> SetOfCalendarObjects { get; set; } = new ObservableCollection<CalendarObject>();

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TimekeeperDAL.EF;
using TimekeeperWPF.Calendar;

namespace TimekeeperWPF
{
    public class FakeDayViewModel : DayViewModel
    {
        protected override async Task GetDataAsync()
        {
            await Task.Delay(0);

            CalTaskObjsCollection = new CollectionViewSource();
            CalTaskObjsCollection.Source = new ObservableCollection<CalendarTaskObject>()
            {
                new CalendarTaskObject(),
            };
            OnPropertyChanged(nameof(CalTaskObjsView));
        }
    }
}

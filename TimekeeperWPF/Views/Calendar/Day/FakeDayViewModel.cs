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

            Context = new FakeTimeKeeperContext();
            Items.Source = Context.Notes.Local;

            CalendarObjectsCollection = new CollectionViewSource();
            CalendarObjectsCollection.Source = new ObservableCollection<CalendarObject>()
            {
                new CalendarObject(),
            };
            OnPropertyChanged(nameof(CalendarObjectsView));
        }
    }
}

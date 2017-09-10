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

namespace TimekeeperWPF
{
    public abstract class CalendarViewModel : ViewModel<TimeTask>
    {
        #region Fields
        private CalendarObject _SelectedCalendarObect;
        #endregion
        public CalendarViewModel() :base()
        {
        }
        #region Properties
        public CollectionViewSource CalendarObjectsCollection { get; set; }
        public ObservableCollection<CalendarObject> CalendarObjectsSource => CalendarObjectsCollection?.Source as ObservableCollection<CalendarObject>;
        public ListCollectionView CalendarObjectsView => CalendarObjectsCollection?.View as ListCollectionView;
        public CalendarObject SelectedCalendarObject
        {
            get
            {
                return _SelectedCalendarObect;
            }
            set
            {
                if (_SelectedCalendarObect == value) return;
                _SelectedCalendarObect = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            //get tasks 
            //read task data and create CalendarObjects
            //calculate collisions and reorganize CalendarObjects by changing their datetimes
            Context = new TimeKeeperContext();
            await Context.TimeTasks.LoadAsync();
            Items.Source = Context.TimeTasks.Local;

            CalendarObjectsCollection = new CollectionViewSource();
            CalendarObjectsCollection.Source = new ObservableCollection<CalendarObject>()
            {
                new CalendarObject()
                {
                    Start = DateTime.Now.Date.AddMinutes(10),
                    End = DateTime.Now.Date.AddHours(1).AddMinutes(20),
                },
                new CalendarObject()
                {
                    Start = DateTime.Now.Date.AddHours(2).AddMinutes(20),
                    End = DateTime.Now.Date.AddHours(4),
                }
            };
            OnPropertyChanged(nameof(CalendarObjectsView));
        }
        #endregion
    }
}
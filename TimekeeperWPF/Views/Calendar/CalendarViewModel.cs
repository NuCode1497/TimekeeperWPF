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

namespace TimekeeperWPF
{
    public abstract class CalendarViewModel : ViewModel<TimeTask>
    {
        #region Fields
        private CalendarObject _SelectedCalendarObect;
        private DateTime _SelectedDate;
        private Orientation _Orientation;
        private bool _Max;
        private bool _TextMargin;
        private int _ScaleSudoCommand;
        private ICommand _PreviousCommand;
        private ICommand _NextCommand;
        private ICommand _OrientationCommand;
        private ICommand _MaxCommand;
        private ICommand _TextMarginCommand;
        private ICommand _ScaleUpCommand;
        private ICommand _ScaleDownCommand;
        #endregion
        public CalendarViewModel() :base()
        {
            SelectedDate = DateTime.Now.Date;
            Orientation = Orientation.Vertical;
            TextMargin = true;
            Max = false;
        }
        #region Properties
        public CollectionViewSource CalendarObjectsCollection { get; set; }
        public ObservableCollection<CalendarObject> CalendarObjectsSource => CalendarObjectsCollection?.Source as ObservableCollection<CalendarObject>;
        public ListCollectionView CalendarObjectsView => CalendarObjectsCollection?.View as ListCollectionView;
        public CalendarObject SelectedCalendarObject
        {
            get { return _SelectedCalendarObect; }
            set
            {
                if (_SelectedCalendarObect == value) return;
                _SelectedCalendarObect = value;
                OnPropertyChanged();
            }
        }
        public DateTime SelectedDate
        {
            get { return _SelectedDate; }
            set
            {
                if (_SelectedDate.Date == value.Date) return;
                _SelectedDate = value.Date;
                OnPropertyChanged();
            }
        }
        public Orientation Orientation
        {
            get{ return _Orientation; }
            set
            {
                if (_Orientation == value) return;
                _Orientation = value;
                OnPropertyChanged();
            }
        }
        public bool Max
        {
            get { return _Max; }
            set
            {
                if (_Max == value) return;
                _Max = value;
                OnPropertyChanged();
            }
        }
        public bool TextMargin
        {
            get { return _TextMargin; }
            set
            {
                if (_TextMargin == value) return;
                _TextMargin = value;
                OnPropertyChanged();
            }
        }
        public int ScaleSudoCommand
        {
            get { return _ScaleSudoCommand; }
            private set
            {
                _ScaleSudoCommand = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Conditions
        #endregion
        #region Commands
        public ICommand PreviousCommand => _PreviousCommand
            ?? (_PreviousCommand = new RelayCommand(ap => Previous(), pp => true));
        public ICommand NextCommand => _NextCommand
            ?? (_NextCommand = new RelayCommand(ap => Next(), pp => true));
        public ICommand OrientationCommand => _OrientationCommand
            ?? (_OrientationCommand = new RelayCommand(ap => ToggleOrientation(), pp => true));
        public ICommand MaxCommand => _MaxCommand
            ?? (_MaxCommand = new RelayCommand(ap => ToggleMaxScale(), pp => true));
        public ICommand TextMarginCommand => _TextMarginCommand
            ?? (_TextMarginCommand = new RelayCommand(ap => ToggleTextMargin(), pp => true));
        public ICommand ScaleUpCommand => _ScaleUpCommand
            ?? (_ScaleUpCommand = new RelayCommand(ap => ScaleUp(), pp => true));
        public ICommand ScaleDownCommand => _ScaleDownCommand
            ?? (_ScaleDownCommand = new RelayCommand(ap => ScaleDown(), pp => true));
        #endregion
        #region Predicates
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
        private void Previous() { SelectedDate = SelectedDate.AddDays(-1); }
        private void Next() { SelectedDate = SelectedDate.AddDays(1); }
        private void ToggleOrientation()
        {
            if (Orientation == Orientation.Horizontal)
                Orientation = Orientation.Vertical;
            else
                Orientation = Orientation.Horizontal;
        }
        private void ToggleMaxScale() { Max = !Max; }
        private void ToggleTextMargin() { TextMargin = !TextMargin; }
        private void ScaleUp()
        {
            ScaleSudoCommand = 1;
            ScaleSudoCommand = 0;
        }
        private void ScaleDown()
        {
            ScaleSudoCommand = -1;
            ScaleSudoCommand = 0;
        }
        #endregion
    }
}
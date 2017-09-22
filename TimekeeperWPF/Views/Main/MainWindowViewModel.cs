using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TimekeeperWPF.Tools;
using TimekeeperWPF.Examples;
using TimekeeperWPF.Calendar;
using System;

namespace TimekeeperWPF
{
    public class MainWindowViewModel : ObservableObject, IDisposable
    {
        #region Fields
        private ObservableCollection<IView> _views;
        private IView _currentView;
        private ICommand _navigateViewCommand = null;
        private DateTime _Clock;
        private MonthViewModel _MonthVM;
        private WeekViewModel _WeekVM;
        private DayViewModel _DayVM;
        #endregion
        public MainWindowViewModel()
        {
            Day._Timer.Tick += OnTimerTick;

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Views.Add(new FakeDayViewModel());
                Views.Add(new FakeNotesViewModel());
                Navigate(Views[0]);
                return;
            }
            _MonthVM = new MonthViewModel();
            _WeekVM = new WeekViewModel();
            _DayVM = new DayViewModel();
            _MonthVM.RequestViewChange += OnRequestViewChange;
            _WeekVM.RequestViewChange += OnRequestViewChange;
            _DayVM.RequestViewChange += OnRequestViewChange;

            Views.Add(_WeekVM);
            Views.Add(_DayVM);
            Views.Add(_MonthVM);
            Views.Add(new NotesViewModel());
            Views.Add(new LabelsViewModel());
            Views.Add(new ResourcesViewModel());
            Views.Add(new TaskTypesViewModel());
            Views.Add(new TimePointsViewModel());
            Views.Add(new TimeTasksViewModel());
            Views.Add(new TimePatternsViewModel());
            Navigate(Views[0]);

            //RadialSample radialSample = new RadialSample();
            //radialSample.Show();

            //Window1 window1 = new Window1();
            //window1.Show();
        }
        #region Properties
        public DateTime Clock
        {
            get { return _Clock; }
            set
            {
                _Clock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ClockString));
            }
        }
        public string ClockString => Clock.ToLongTimeString();
        public ObservableCollection<IView> Views => _views
            ?? (_views = new ObservableCollection<IView>());
        public IView CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                if (_currentView != value) _currentView = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Events
        private void OnRequestViewChange(object sender, RequestViewChangeEventArgs e)
        {
            switch (e.Type) 
            {
                case CalendarViewType.Day:
                    _DayVM.SelectedDate = e.Date;
                    Navigate(_DayVM);
                    break;
                case CalendarViewType.Week:
                    _WeekVM.SelectedDate = e.Date;
                    Navigate(_WeekVM);
                    break;
                case CalendarViewType.Month:
                    _MonthVM.SelectedDate = e.Date;
                    Navigate(_MonthVM);
                    break;
                case CalendarViewType.Year:
                    break;
            }
        }
        private void OnTimerTick(object sender, EventArgs e)
        {
            Clock = DateTime.Now;
        }
        #endregion
        public ICommand NavigateViewCommand => _navigateViewCommand 
            ?? (_navigateViewCommand = new RelayCommand(ap => Navigate(ap as IView), pp => pp is IView));
        private void Navigate(IView page)
        {
            CurrentView = page;
            if (CurrentView.GetDataCommand.CanExecute(null))
                CurrentView.GetDataCommand.Execute(null);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Day._Timer.Tick -= OnTimerTick;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MainWindowViewModel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

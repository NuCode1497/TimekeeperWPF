using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TimekeeperWPF.Tools;
using TimekeeperWPF.Examples;
using System.Windows;

namespace TimekeeperWPF
{
    public class MainWindowViewModel : ObservableObject
    {
        private ObservableCollection<IView> _views;
        private IView _currentView;
        private ICommand _navigateViewCommand = null;

        public MainWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Views.Add(new FakeDayViewModel());
                Views.Add(new FakeNotesViewModel());
                Views.Add(new FakeMonthViewModel());
                Navigate(Views[0]);
                return;
            }
            Views.Add(new DayViewModel());
            Views.Add(new NotesViewModel());
            Views.Add(new LabelsViewModel());
            Views.Add(new ResourcesViewModel());
            Views.Add(new TaskTypesViewModel());
            Views.Add(new TimePointsViewModel());
            Views.Add(new TimeTasksViewModel());
            Views.Add(new TimePatternsViewModel());
            Views.Add(new MonthViewModel());
            Views.Add(new WeekViewModel());
            Navigate(Views[0]);

            //RadialSample radialSample = new RadialSample();
            //radialSample.Show();

            //Window1 window1 = new Window1();
            //window1.Show();
        }

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

        public ICommand NavigateViewCommand => _navigateViewCommand 
            ?? (_navigateViewCommand = new RelayCommand(ap => Navigate(ap as IView), pp => pp is IView));
        private void Navigate(IView page)
        {
            CurrentView = page;
            if (CurrentView.GetDataCommand.CanExecute(null))
                CurrentView.GetDataCommand.Execute(null);
        }
    }
}

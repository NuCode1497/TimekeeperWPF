using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class MainWindowViewModel : ObservableObject
    {
        private ObservableCollection<IPage> _views;
        private IPage _currentView;
        private ICommand _navigateViewCommand = null;

        public MainWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                Views.Add(new FakeNotesViewModel());
                Views.Add(new FakeMonthViewModel());
                Navigate(Views[0]);
                return;
            }
            Views.Add(new NotesViewModel());
            Views.Add(new LabelsViewModel());
            Views.Add(new ResourcesViewModel());
            Views.Add(new TaskTypesViewModel());
            Views.Add(new TimePointsViewModel());
            Views.Add(new TimeTasksViewModel());
            Views.Add(new TimePatternsViewModel());
            Views.Add(new MonthViewModel());
            Navigate(Views[0]);
        }

        public ObservableCollection<IPage> Views => _views
            ?? (_views = new ObservableCollection<IPage>());

        public IPage CurrentView
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
            ?? (_navigateViewCommand = new RelayCommand(ap => Navigate(ap as IPage), pp => pp is IPage));
        private void Navigate(IPage page)
        {
            CurrentView = page;
            if(CurrentView.GetDataCommand.CanExecute(null))
                CurrentView.GetDataCommand.Execute(null);
        }
    }
}

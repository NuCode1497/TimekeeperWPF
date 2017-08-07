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
                Views.Add(new FakeMonthViewModel());
                Views.Add(new FakeNotesViewModel());
                CurrentView = Views[0];
                return;
            }
            Views.Add(new MonthViewModel());
            Views.Add(new NotesViewModel());
            CurrentView = Views[0];
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
            ?? (_navigateViewCommand = new RelayCommand(ap => CurrentView = ap as IPage, pp => pp is IPage));
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.Models;
using TimekeeperDAL.Repos;
using TimekeeperWPF.Tools;
using System.Windows.Input;
using System.ComponentModel;

namespace TimekeeperWPF
{
    public class MainWindowViewModel : ObservableObject
    {
        private ObservableCollection<IPage> _views;
        private IPage _currentView;
        private ICommand _navigateViewCommand = null;

        public MainWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;

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
            ?? (_navigateViewCommand = new RelayCommand(ap => CurrentView = (IPage)ap, pp => pp is IPage));
    }
}

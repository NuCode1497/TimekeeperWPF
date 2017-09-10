using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TimekeeperWPF.Tools;
using TimekeeperWPF.Examples;
using System.Windows;
using System;

namespace TimekeeperWPF.Examples
{
    public class Window1VM : ObservableObject
    {
        private ObservableCollection<IView> _views;
        private IView _currentView1;
        private IView _currentView2;
        private IView _currentView3;
        private IView _currentView4;
        private ICommand _navigateView1Command = null;
        private ICommand _navigateView2Command = null;
        private ICommand _navigateView3Command = null;
        private ICommand _navigateView4Command = null;
        public Window1VM()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }
            Views.Add(new DayViewModel());
            Navigate1(Views[0]);
            //Views.Add(new WeekViewModel());
            //Navigate3(Views[1]);
        }
        public ObservableCollection<IView> Views => _views
            ?? (_views = new ObservableCollection<IView>());
        public IView CurrentView1
        {
            get
            {
                return _currentView1;
            }
            set
            {
                if (_currentView1 != value) _currentView1 = value;
                OnPropertyChanged();
            }
        }
        public IView CurrentView2
        {
            get
            {
                return _currentView2;
            }
            set
            {
                if (_currentView2 != value) _currentView2 = value;
                OnPropertyChanged();
            }
        }
        public IView CurrentView3
        {
            get
            {
                return _currentView3;
            }
            set
            {
                if (_currentView3 != value) _currentView3 = value;
                OnPropertyChanged();
            }
        }
        public IView CurrentView4
        {
            get
            {
                return _currentView4;
            }
            set
            {
                if (_currentView4 != value) _currentView4 = value;
                OnPropertyChanged();
            }
        }
        public ICommand NavigateView1Command => _navigateView1Command
            ?? (_navigateView1Command = new RelayCommand(ap => Navigate1(ap as IView), pp => pp is IView));
        private void Navigate1(IView page)
        {
            CurrentView1 = page;
            if (CurrentView1.GetDataCommand.CanExecute(null))
                CurrentView1.GetDataCommand.Execute(null);
        }
        public ICommand NavigateView2Command => _navigateView2Command
            ?? (_navigateView2Command = new RelayCommand(ap => Navigate2(ap as IView), pp => pp is IView));
        private void Navigate2(IView page)
        {
            CurrentView1 = page;
            if (CurrentView2.GetDataCommand.CanExecute(null))
                CurrentView2.GetDataCommand.Execute(null);
        }
        public ICommand NavigateView3Command => _navigateView3Command
            ?? (_navigateView3Command = new RelayCommand(ap => Navigate3(ap as IView), pp => pp is IView));
        private void Navigate3(IView page)
        {
            CurrentView3 = page;
            if (CurrentView3.GetDataCommand.CanExecute(null))
                CurrentView3.GetDataCommand.Execute(null);
        }
        public ICommand NavigateView4Command => _navigateView4Command
            ?? (_navigateView4Command = new RelayCommand(ap => Navigate4(ap as IView), pp => pp is IView));
        private void Navigate4(IView page)
        {
            CurrentView1 = page;
            if (CurrentView4.GetDataCommand.CanExecute(null))
                CurrentView4.GetDataCommand.Execute(null);
        }
    }
}

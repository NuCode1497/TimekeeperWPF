// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using TimekeeperDAL.EF;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows.Input;
using TimekeeperWPF.Tools;
using TimekeeperWPF.Calendar;
using System.Windows.Data;
using System.Windows;
using TimekeeperDAL.Tools;

namespace TimekeeperWPF
{
    public class MonthViewModel : CalendarViewModel
    {
        private MonthWeekViewModel _SelectedWeek;
        private bool _HasSelectedWeek = false;
        #region Properties
        public override string Name => "Month View";
        public CollectionViewSource MonthWeeksCollection { get; set; }
        public ObservableCollection<MonthWeekViewModel> MonthWeeksSource => MonthWeeksCollection.Source as ObservableCollection<MonthWeekViewModel>;
        public ListCollectionView MonthWeeksView => MonthWeeksCollection.View as ListCollectionView;
        public override DateTime Start
        {
            get { return base.Start; }
            set
            {
                DateTime newValue = value.MonthStart();
                base.Start = newValue;
            }
        }
        public override DateTime End
        {
            get
            {
                return Start.AddMonths(1);
            }
            set
            {
            }
        }
        public MonthWeekViewModel SelectedWeek
        {
            get { return _SelectedWeek; }
            set
            {
                if (_SelectedWeek == value) return;
                _SelectedWeek = value;
                if (SelectedWeek == null)
                {
                    HasSelectedWeek = false;
                }
                else
                {
                    HasSelectedWeek = true;
                    Status = "Double Click to view week.";
                }
                OnPropertyChanged();
            }
        }
        public override bool TextMargin
        {
            get => base.TextMargin;
            set
            {
                base.TextMargin = value;
                SetTextMargin();
            }
        }
        #endregion
        #region Conditions
        public bool HasSelectedWeek
        {
            get
            {
                return _HasSelectedWeek;
            }
            protected set
            {
                _HasSelectedWeek = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedWeek));
            }
        }
        public bool HasNotSelectedWeek => !HasSelectedWeek;
        #endregion
        #region Predicates
        protected override bool CanAddNew(CalendarObjectTypes CO) { return false; }
        protected override bool CanCancel => false;
        protected override bool CanCommit => false;
        protected override bool CanEditSelected => false;
        protected override bool CanDeleteSelected => false;
        protected override bool CanSave => false;
        protected override bool CanOrientation => false;
        public override bool CanMax => false;
        protected override bool CanScaleDown => false;
        protected override bool CanScaleUp => false;
        protected override bool CanSelectMonth => false;
        #endregion
        #region Actions
        protected override void SelectWeek()
        {
            DateTime selectedDate = Start;
            if (HasSelectedWeek) selectedDate = SelectedWeek.Start;
            RequestViewChangeEventArgs e = new RequestViewChangeEventArgs(
                CalendarViewType.Week, selectedDate);
            OnRequestViewChange(e);
        }
        protected void SetTextMargin()
        {
            foreach (var w in MonthWeeksView)
            {
                MonthWeekViewModel mwVM = w as MonthWeekViewModel;
                mwVM.TextMargin = TextMargin;
            }
        }
        internal override async Task LoadData()
        {
            IsEnabled = false;
            IsLoading = true;
            Status = "Loading Data...";
            try
            {
                await GetDataAsync();
                IsEnabled = true;
                Status = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, String.Format("Error Loading {0} Data", Name), MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Failed to get data!";
            }
            IsLoading = false;
        }
        protected override async Task GetDataAsync()
        {
            await Task.Delay(0);
            await SetUpCalendarObjects();
        }
        protected override async Task PreviousAsync()
        {
            DateTime previousMonth = Start.AddMonths(-1);
            Start = previousMonth;
            await LoadData();
        }
        protected override async Task NextAsync()
        {
            DateTime nextMonth = Start.AddMonths(1);
            Start = nextMonth;
            await LoadData();
        }
        private async Task SetUpCalendarObjects()
        {
            await Task.Delay(0);
            MonthWeeksCollection = new CollectionViewSource();
            MonthWeeksCollection.Source = new ObservableCollection<MonthWeekViewModel>();
            int numWeeks = Start.MonthWeeks();
            DateTime firstDay = new DateTime(Year, Month, 1);
            for (int i = 0; i < numWeeks; i++)
            {
                //TODO: any way to make this more efficient? This is creating up to 6 copies of
                //DbContexts for each month. Are these getting disposed?
                MonthWeekViewModel week = new MonthWeekViewModel();
                week.GetDataCommand.Execute(null);
                week.Start = firstDay.AddDays(7 * i);
                week.SelectedMonthOverride = Month;
                week.TextMargin = TextMargin;
                MonthWeeksView.AddNewItem(week);
                MonthWeeksView.CommitNew();
            }
            OnPropertyChanged(nameof(MonthWeeksView));
        }
        internal override void SaveAs()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

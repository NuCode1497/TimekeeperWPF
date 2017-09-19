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

namespace TimekeeperWPF
{
    public class MonthViewModel : CalendarViewModel
    {
        #region Fields
        private MonthWeekViewModel _SelectedWeek;
        private bool _HasSelectedWeek = false;
        #endregion
        public MonthViewModel() : base()
        {
        }
        #region Properties
        public override string Name => "Month View";
        public CollectionViewSource MonthWeeksCollection { get; set; }
        public ObservableCollection<MonthWeekViewModel> MonthWeeksSource => MonthWeeksCollection.Source as ObservableCollection<MonthWeekViewModel>;
        public ListCollectionView MonthWeeksView => MonthWeeksCollection.View as ListCollectionView;
        public override DateTime SelectedDate
        {
            get { return base.SelectedDate; }
            set
            {
                DateTime newValue = value.MonthStart();
                base.SelectedDate = newValue;
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
        protected override bool CanAddNew => false;
        protected override bool CanCancel => false;
        protected override bool CanCommit => false;
        protected override bool CanDeselect => false;
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
            DateTime selectedDate = SelectedDate;
            if (HasSelectedWeek) selectedDate = SelectedWeek.SelectedDate;
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
        protected override async void LoadData()
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
            SetUpCalendarObjects();
        }
        protected override void Previous()
        {
            DateTime previousMonth = SelectedDate.AddMonths(-1);
            SelectedDate = previousMonth;
            LoadData();
        }
        protected override void Next()
        {
            DateTime nextMonth = SelectedDate.AddMonths(1);
            SelectedDate = nextMonth;
            LoadData();
        }
        protected override void SetUpCalendarObjects()
        {
            MonthWeeksCollection = new CollectionViewSource();
            MonthWeeksCollection.Source = new ObservableCollection<MonthWeekViewModel>();
            int numWeeks = SelectedDate.MonthWeeks();
            DateTime firstDay = new DateTime(SelectedYear, SelectedMonth, 1);
            for (int i = 0; i < numWeeks; i++)
            {
                MonthWeekViewModel week = new MonthWeekViewModel();
                week.GetDataCommand.Execute(null);
                week.SelectedDate = firstDay.AddDays(7 * i);
                week.SelectedMonthOverride = SelectedMonth;
                week.TextMargin = TextMargin;
                MonthWeeksView.AddNewItem(week);
                MonthWeeksView.CommitNew();
            }
            OnPropertyChanged(nameof(MonthWeeksView));
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }

        protected override bool IsTaskRelevant(TimeTask task)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

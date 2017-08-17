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

namespace TimekeeperWPF
{
    public class MonthViewModel : ViewModel<Note>
    {
        #region Fields
        private Calendar _Calendar => CultureInfo.CurrentCulture.Calendar;
        private DateTime _SelectedDateTime;
        private ICommand _NextMonthCommand;
        private ICommand _PrevMonthCommand;
        #endregion
        public MonthViewModel() : base()
        {
            _SelectedDateTime = DateTime.Now;
            Sorter = new DateTimeSorter();
            LoadData();
            BuildMonth();
        }
        public override string Name => "Month";
        #region Properties
        public DateTime SelectedDateTime
        {
            get
            {
                return _SelectedDateTime;
            }
            set
            {
                _SelectedDateTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedYear));
                OnPropertyChanged(nameof(SelectedMonth));
                OnPropertyChanged(nameof(SelectedDay));
                OnPropertyChanged(nameof(DaysInMonth));
                OnPropertyChanged(nameof(SelectedMonthString));
                OnPropertyChanged(nameof(SelectedYearString));
            }
        }
        public int DaysInMonth => _Calendar.GetDaysInMonth(SelectedYear, SelectedMonth);
        public int SelectedYear => _SelectedDateTime.Year;
        public int SelectedMonth => _SelectedDateTime.Month;
        public string SelectedMonthString => SelectedDateTime.ToString("MMMM");
        public string SelectedYearString => SelectedDateTime.ToString("yyy");
        public int SelectedDay => _SelectedDateTime.Day;
        #endregion
        public List<Week> Weeks { get; set; }
        #region Commands
        public ICommand NextMonthCommand => _NextMonthCommand
            ?? (_NextMonthCommand = new RelayCommand(ap => NextMonth(), pp => true));
        public ICommand PrevMonthCommand => _PrevMonthCommand
            ?? (_PrevMonthCommand = new RelayCommand(ap => PrevMonth(), pp => true));
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        #endregion
        #region Actions
        protected override async Task<ObservableCollection<Note>> GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.Notes.LoadAsync();
            return Context.Notes.Local;
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        private void NextMonth()
        {
            SelectedDateTime = SelectedDateTime.AddMonths(1);
            BuildMonth();
        }
        private void PrevMonth()
        {
            SelectedDateTime = SelectedDateTime.AddMonths(-1);
            BuildMonth();
        }
        private void BuildMonth()
        {
            //We will build a month as a list of weeks and weeks as lists of 7 days.
            //To align the month correctly, we need to ask how many days are in the month and what day of the week is the first.
            //Every week starts on Sunday. We need to find the first Sunday of the first week that may or may not be in the month.
            //Starting from the first Sunday, we fill each week with successive days until we run out of days of the month
            //and the last week is filled.
            Weeks = new List<Week>();
            DateTime firstDay = new DateTime(SelectedYear, SelectedMonth, 1);
            DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);
            DayOfWeek firstDayWeekday = _Calendar.GetDayOfWeek(firstDay);
            DayOfWeek lastDayWeekday = _Calendar.GetDayOfWeek(lastDay);
            DateTime firstSunday = firstDay.AddDays(-(int)firstDayWeekday);
            for (int d = -(int)firstDayWeekday; d < DaysInMonth;)
            {
                Week week = new Week();
                for(int wd = 0; wd < 7; wd ++)
                {
                    Day day = new Day()
                    {
                        DateTime = firstDay.AddDays(d),
                        IsNotInMonth = d < 0 || d >= DaysInMonth
                    };
                    week.Days.Add(day);
                    d++;
                }
                Weeks.Add(week);
            }
            OnPropertyChanged(nameof(Weeks));
        }
        #endregion
    }
}

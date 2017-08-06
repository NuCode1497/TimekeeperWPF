using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using TimekeeperDAL.EF;
using System.Collections.ObjectModel;
using System.Data.Entity;

namespace TimekeeperWPF
{
    public class MonthViewModel : ViewModel<Note, TimeKeeperContext>
    {
        private Calendar _Calendar => CultureInfo.CurrentCulture.Calendar;
        private int _DaysInMonth;
        private int _DaysOffset;
        private int _WeekRowsInMonth;
        private int _SelectedYear;
        private int _SelectedMonth;
        private int _SelectedDay;
        private List<Week> _Weeks;
        private DateTime _SelectedDateTime;
        public MonthViewModel() : base()
        {
            _SelectedDateTime = DateTime.Now;
            Sorter = new DateTimeSorter();
            LoadData();
        }
        public override string Name => "Month";
        public int DaysInMonth => _Calendar.GetDaysInMonth(SelectedYear, SelectedMonth);
        public int DaysOffset
        {
            get
            {
                return _DaysOffset;
            }
            private set
            {
                _DaysOffset = value;
                OnPropertyChanged();
            }
        }
        public int WeekRowsInMonth
        {
            get
            {
                return _WeekRowsInMonth;
            }
            private set
            {
                _WeekRowsInMonth = value;
                OnPropertyChanged();
            }
        }
        public int SelectedYear => _SelectedDateTime.Year;
        public int SelectedMonth => _SelectedDateTime.Month;
        public int SelectedDay => _SelectedDateTime.Day;
        public List<Week> Weeks { get; set; }
        protected override async Task<ObservableCollection<Note>> GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.Notes.LoadAsync();
            return Context.Notes.Local;
        }
        private void BuildMonth()
        {
            //We will build a month as a list of weeks and weeks as lists of 7 days.
            //To align the month correctly, we need to ask how many days are in the month and what day of the week is the first.
            //Every week starts on Sunday. We need to find the first Sunday of the first week that may or may not be in the month.
            //Starting from the first Sunday, we fill each week with successive days until we run out of days of the month
            //and the last week is filled.

            Weeks.Clear();
            DateTime firstDay = new DateTime(_SelectedYear, _SelectedMonth, 1);
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
                        DateTime = firstSunday.AddDays(d),
                        IsNotInMonth = d < 0 || d >= DaysInMonth
                    };
                    week.Days.Add(day);
                    d++;
                }
                Weeks.Add(week);
            }
            OnPropertyChanged(nameof(Weeks));
        }
    }
}

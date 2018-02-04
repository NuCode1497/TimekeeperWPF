using System;
using System.Windows;
using System.Windows.Controls;
using TimekeeperDAL.EF;
using TimekeeperDAL.Tools;

namespace TimekeeperWPF.Calendar
{
    public enum CheckInKind
    {
        //The order here is important for proper sorting
        PerZoneStart, InclusionZoneStart, Cancel, EventStart,
        EventEnd, InclusionZoneEnd, PerZoneEnd,
    }
    public partial class CalendarCheckInObject : CalendarObject
    {
        public CalendarCheckInObject()
        {
            InitializeComponent();
        }
        public override string ToString()
        {
            return Kind + " \n" + CheckIn;
        }
        public override string BasicString => CheckIn.ToString();
        public CheckIn CheckIn { get; set; }
        public CheckInKind Kind { get; set; }
        public Orientation Orientation { get; set; }
        public DateTime DateTime => CheckIn.DateTime;
        public TimeTask TimeTask => ParentPerZone?.ParentMap?.TimeTask;
        public int Dimension => TimeTask?.Dimension ?? 0;
        public int DimensionCount { get; set; }
        #region ParentMap
        //public CalendarTimeTaskMap ParentMap
        //{
        //    get { return (CalendarTimeTaskMap)GetValue(ParentMapProperty); }
        //    set { SetValue(ParentMapProperty, value); }
        //}
        //public static readonly DependencyProperty ParentMapProperty =
        //    DependencyProperty.Register(
        //        nameof(ParentMap), typeof(CalendarTimeTaskMap), typeof(CalendarCheckInObject));
        #endregion
        #region ParentPerZone
        public PerZone ParentPerZone
        {
            get { return (PerZone)GetValue(ParentPerZoneProperty); }
            set { SetValue(ParentPerZoneProperty, value); }
        }
        public static readonly DependencyProperty ParentPerZoneProperty =
            DependencyProperty.Register(
                nameof(ParentPerZone), typeof(PerZone), typeof(CalendarCheckInObject));
        #endregion
        #region ParentInclusionZone
        public InclusionZone ParentInclusionZone
        {
            get { return (InclusionZone)GetValue(ParentInclusionZoneProperty); }
            set { SetValue(ParentInclusionZoneProperty, value); }
        }
        public static readonly DependencyProperty ParentInclusionZoneProperty =
            DependencyProperty.Register(
                nameof(ParentInclusionZone), typeof(InclusionZone), typeof(CalendarCheckInObject));
        #endregion
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    /// <summary>
    /// Interaction logic for CalendarCheckInObject.xaml
    /// </summary>
    public partial class CalendarCheckInObject : UserControl
    {
        public CalendarCheckInObject()
        {
            InitializeComponent();
        }
        public CheckIn CheckIn { get; set; }
        public DateTime DateTime => CheckIn.DateTime;
        public bool Intersects(DateTime start, DateTime end) { return start < DateTime && DateTime < end; }
        public bool Intersects(InclusionZone Z) { return Intersects(Z.Start, Z.End); }
        public bool Intersects(TimeTask T) { return Intersects(T.Start, T.End); }
        public bool Intersects(CalendarTaskObject C) { return Intersects(C.Start, C.End); }
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

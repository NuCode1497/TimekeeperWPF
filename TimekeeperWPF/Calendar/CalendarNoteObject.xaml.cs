using System;
using System.Windows;
using System.Windows.Controls;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    public partial class CalendarNoteObject : UserControl
    {
        public CalendarNoteObject()
        {
            InitializeComponent();
        }
        public Note Note { get; set; }
        public DateTime DateTime => Note.DateTime;
        public bool Intersects(DateTime start, DateTime end) { return start < DateTime && DateTime < end; }
        public bool Intersects(InclusionZone Z) { return Intersects(Z.Start, Z.End); }
        public bool Intersects(TimeTask T) { return Intersects(T.Start, T.End); }
        public bool Intersects(CalendarTaskObject C) { return Intersects(C.Start, C.End); }
    }
}

﻿using System;
using System.Windows;
using System.Windows.Controls;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    public partial class CalendarNoteObject : CalendarFlairObject
    {
        public CalendarNoteObject()
        {
            InitializeComponent();
        }
        public override string ToString()
        {
            return Note.ToString();
        }
        public override string BasicString => Note.ToString();
        public override DateTime DateTime => Note.DateTime;
        public override int Dimension => TimeTask?.Dimension ?? 0;
        public Note Note { get; set; }
        public TimeTask TimeTask => Note.TimeTask;
        public bool Intersects(DateTime start, DateTime end) { return start < DateTime && DateTime < end; }
        public bool Intersects(InclusionZone Z) { return Intersects(Z.Start, Z.End); }
        public bool Intersects(TimeTask T) { return Intersects(T.Start, T.End); }
        public bool Intersects(CalendarTaskObject C) { return Intersects(C.Start, C.End); }
    }
}

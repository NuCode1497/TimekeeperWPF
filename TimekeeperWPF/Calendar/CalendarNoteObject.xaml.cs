using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    /// <summary>
    /// Interaction logic for CalendarNoteObject.xaml
    /// </summary>
    public partial class CalendarNoteObject : UserControl
    {
        public CalendarNoteObject()
        {
            InitializeComponent();
        }
        private Note _Note;
        public Note Note
        {
            get { return _Note; }
            set
            {
                if (value == _Note) return;
                _Note = value;
                TaskType = _Note.TaskType;
            }
        }
        public DateTime DateTime
        {
            get { return (DateTime)GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }
        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register(
                nameof(DateTime), typeof(DateTime), typeof(CalendarNoteObject),
                new FrameworkPropertyMetadata(DateTime.Now));
        public TaskType TaskType
        {
            get { return (TaskType)GetValue(TaskTypeProperty); }
            set { SetValue(TaskTypeProperty, value); }
        }
        public static readonly DependencyProperty TaskTypeProperty =
            DependencyProperty.Register(
                nameof(TaskType), typeof(TaskType), typeof(CalendarNoteObject),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnTaskTypeChanged)));
        public static void OnTaskTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarNoteObject CalObj = d as CalendarNoteObject;
            TaskType value = (TaskType)e.NewValue;
            CalObj.SetValue(TaskTypeNamePropertyKey, value.Name);
        }
        public string TaskTypeName
        {
            get { return (string)GetValue(TaskTypeNameProperty); }
            private set { SetValue(TaskTypeNamePropertyKey, value); }
        }
        private static readonly DependencyPropertyKey TaskTypeNamePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TaskTypeName), typeof(string), typeof(CalendarNoteObject),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty TaskTypeNameProperty =
            TaskTypeNamePropertyKey.DependencyProperty;
        public bool Intersects(DateTime start, DateTime end) { return start < DateTime && DateTime < end; }
        public bool Intersects(InclusionZone Z) { return Intersects(Z.Start, Z.End); }
        public bool Intersects(TimeTask T) { return Intersects(T.Start, T.End); }
        public bool Intersects(CalendarTaskObject C) { return Intersects(C.Start, C.End); }
    }
}

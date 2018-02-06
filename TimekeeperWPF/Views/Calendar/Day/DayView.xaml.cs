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
using System.Windows.Threading;

namespace TimekeeperWPF
{
    /// <summary>
    /// Interaction logic for DayView.xaml
    /// </summary>
    public partial class DayView : UserControl
    {
        public DayView()
        {
            InitializeComponent();
        }
        private void EditNotePopUp_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditNotePopUp.Visibility == Visibility.Visible)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    NoteEditor.NoteTextBox.Focus();
                }, DispatcherPriority.Render);
            }
        }
        private void EditTimeTaskPopUp_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditTimeTaskPopUp.Visibility == Visibility.Visible)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    TimeTaskEditor.NameTextBox.Focus();
                }, DispatcherPriority.Render);
            }
        }
        private void EditCheckInPopUp_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditCheckInPopUp.Visibility == Visibility.Visible)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    CheckInEditor.DateTimeTextBox.Focus();
                }, DispatcherPriority.Render);
            }
        }
    }
}

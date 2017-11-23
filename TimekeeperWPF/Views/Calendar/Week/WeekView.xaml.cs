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
    /// Interaction logic for WeekView.xaml
    /// </summary>
    public partial class WeekView : UserControl
    {
        public WeekView()
        {
            InitializeComponent();
        }
        private void EditNotePopUp_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditNotePopUp.Visibility == Visibility.Visible)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    DataEditor.NoteTextBox.Focus();
                }, DispatcherPriority.Render);
            }
        }
    }
}

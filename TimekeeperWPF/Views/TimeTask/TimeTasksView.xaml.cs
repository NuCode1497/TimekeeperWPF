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
    /// Interaction logic for TimeTasksView.xaml
    /// </summary>
    public partial class TimeTasksView : UserControl
    {
        public TimeTasksView()
        {
            InitializeComponent();
        }

        private void EditTaskPopUp_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditTaskPopUp.Visibility == Visibility.Visible)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    DataEditor.NameTextBox.Focus();
                }, DispatcherPriority.Render);
            }
        }
    }
}

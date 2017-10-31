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
    /// Interaction logic for BasicEntitiesView.xaml
    /// </summary>
    public partial class BasicEntitiesView : UserControl
    {
        public BasicEntitiesView()
        {
            InitializeComponent();
        }

        private void EditPopUp_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditPopUp.Visibility == Visibility.Visible)
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    NameTextBox.Focus();
                }, DispatcherPriority.Render);
            }
        }
    }
}

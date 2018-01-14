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

namespace TimekeeperWPF
{
    /// <summary>
    /// Interaction logic for AllocationView.xaml
    /// </summary>
    public partial class AllocationView : UserControl
    {
        public AllocationView()
        {
            InitializeComponent();
        }
        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, value); }
        }
        public static DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(
                nameof(RemoveCommand), typeof(ICommand), typeof(AllocationView));
    }
}

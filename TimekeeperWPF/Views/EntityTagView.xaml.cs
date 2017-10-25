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
    /// Interaction logic for EntityView.xaml
    /// </summary>
    public partial class EntityTagView : UserControl
    {
        public EntityTagView()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, (string)value); }
        }
        public static DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text), typeof(string), typeof(EntityTagView));
        public Visibility RemoveButtonVisibility
        {
            get { return (Visibility)GetValue(RemoveButtonVisibilityProperty); }
            set { SetValue(RemoveButtonVisibilityProperty, (Visibility)value); }
        }
        public static DependencyProperty RemoveButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(RemoveButtonVisibility), typeof(Visibility), typeof(EntityTagView),
                new FrameworkPropertyMetadata(Visibility.Visible));
        public ICommand RemoveCommand
        {
            get { return (ICommand)GetValue(RemoveCommandProperty); }
            set { SetValue(RemoveCommandProperty, (ICommand)value); }
        }
        public static DependencyProperty RemoveCommandProperty =
            DependencyProperty.Register(
                nameof(RemoveCommand), typeof(ICommand), typeof(EntityTagView));
    }
}

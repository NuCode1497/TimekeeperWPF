using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //if (e.Exception.StackTrace.Contains("at Xceed.Wpf.Toolkit.Spinner.OnSpin(SpinEventArgs e)"))
            //{
            //    //Handle these stupid errors I'm not buying Plus for $500 are you kidding me
            //    e.Handled = true;
            //    return;
            //}

            ExceptionViewer ev = new ExceptionViewer("An unexpected error occurred in the application.", e.Exception, sender as Window);
            ev.ShowDialog();
            e.Handled = true;
        }
    }
}

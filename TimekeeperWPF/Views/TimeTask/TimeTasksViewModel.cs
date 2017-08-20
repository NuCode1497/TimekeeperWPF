using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class TimeTasksViewModel : ViewModel<TimeTask>
    {
        public override string Name => throw new NotImplementedException();

        protected override Task<ObservableCollection<TimeTask>> GetDataAsync()
        {
            throw new NotImplementedException();
        }

        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
    }
}

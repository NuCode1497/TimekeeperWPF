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
    public class TimeTasksViewModel : TypedLabeledEntitiesViewModel<TimeTask>
    {
        #region Fields
        private ICommand _ContinueSaveCommand;
        private DateTime _SaveAsStart;
        private DateTime _SaveAsEnd;
        private string _SaveAsError;
        #endregion
        public override string Name => nameof(Context.TimeTasks) + " Editor";

        protected override Task GetDataAsync()
        {
            throw new NotImplementedException();
        }

        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
    }
}

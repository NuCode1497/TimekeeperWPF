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
    public class TimePatternsViewModel : ViewModel<TimePattern>
    {
        public override string Name => nameof(Context.TimePatterns) + " Editor";

        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimePatterns.LoadAsync();
            Items.Source = Context.TimePatterns.Local;
        }

        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
    }
}

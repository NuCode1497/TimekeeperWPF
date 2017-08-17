using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using TimekeeperDAL.EF;

namespace TimekeeperWPF
{
    public class FakeMonthViewModel : MonthViewModel
    {
        public FakeMonthViewModel() : base()
        {

        }
        public override string Name => "Fake " + base.Name;
        protected override async System.Threading.Tasks.Task<ObservableCollection<Note>> GetDataAsync()
        {
            await System.Threading.Tasks.Task.Delay(0);
            Context = new FakeTimeKeeperContext();
            return Context.Notes.Local;
        }
    }
}

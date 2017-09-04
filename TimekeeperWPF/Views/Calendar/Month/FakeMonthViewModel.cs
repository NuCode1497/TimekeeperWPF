using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;

namespace TimekeeperWPF
{
    public class FakeMonthViewModel : MonthViewModel
    {
        public FakeMonthViewModel() : base()
        {

        }
        public override string Name => "Fake " + base.Name;
        protected override async Task GetDataAsync()
        {
            await Task.Delay(0);
            Context = new FakeTimeKeeperContext();
            Items.Source = Context.Notes.Local;
        }
    }
}

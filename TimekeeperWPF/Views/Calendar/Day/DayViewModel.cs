using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF
{
    public class DayViewModel : CalendarViewModel
    {
        public override string Name => throw new NotImplementedException();

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

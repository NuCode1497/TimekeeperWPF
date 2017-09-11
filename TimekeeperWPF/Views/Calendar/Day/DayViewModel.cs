using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF
{
    public class DayViewModel : CalendarViewModel
    {
        public override string Name => "Day View";
        public DayViewModel() : base()
        {
        }
        protected override bool CanSave => false;
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
    }
}

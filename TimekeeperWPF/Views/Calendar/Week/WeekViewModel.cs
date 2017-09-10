using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF
{
    public class WeekViewModel : CalendarViewModel
    {
        public override string Name => "Week View";
        
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
    }
}

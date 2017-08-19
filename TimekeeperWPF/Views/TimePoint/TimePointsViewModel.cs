using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class TimePointsViewModel : ViewModel<TimePoint>
    {
        public TimePointsViewModel()
        {
            Sorter = new BasicEntitySorter();
            LoadData();
        }
        #region Properties
        public override string Name => nameof(Context.TimePoints) + " Editor";
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        #endregion
        #region Actions
        protected override async Task<ObservableCollection<TimePoint>> GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimePoints.LoadAsync();
            return Context.TimePoints.Local;
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

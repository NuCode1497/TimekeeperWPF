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
    public class TaskTypesViewModel : ViewModel<TaskType>
    {
        public TaskTypesViewModel()
        {
            Sorter = new BasicEntitySorter();
            LoadData();
        }
        #region Properties
        public override string Name => nameof(Context.TaskTypes) + " Editor";
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TaskTypes.LoadAsync();
            Items.Source = Context.TaskTypes.Local;
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

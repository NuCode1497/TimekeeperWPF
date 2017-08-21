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
    public class LabelsViewModel : ViewModel<Label>
    {
        public LabelsViewModel()
        {
            Sorter = new BasicEntitySorter();
            LoadData();
        }
        #region Properties
        public override string Name => nameof(Context.Labels) + " Editor";
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            //await Task.Delay(2000);
            Context = new TimeKeeperContext();
            await Context.Labels.LoadAsync();
            Items.Source = Context.Labels.Local;
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

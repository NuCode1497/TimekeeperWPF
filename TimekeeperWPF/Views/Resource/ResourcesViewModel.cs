using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class ResourcesViewModel : ViewModel<Resource>
    {
        public ResourcesViewModel() : base()
        {
            Sorter = NameSorter;
        }
        #region Properties
        public override string Name => nameof(Context.Resources) + " Editor";
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        protected override bool CanCommit => base.CanCommit && IsNotDuplicate;
        private bool IsNotDuplicate => Source.Count(T => T.Name == CurrentEditItem.Name) == 1;
        protected override bool CanDeleteSelected => base.CanDeleteSelected && !SelectedItem.IsTimeResource;
        protected override bool CanEditSelected => base.CanEditSelected && !SelectedItem.IsTimeResource;
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.Resources.LoadAsync();
            Items.Source = Context.Resources.Local;
            await AddMissingDefaultsAsync();
        }
        private async Task AddMissingDefaultsAsync()
        {
            var missingDefaults = Resource.TimeResourceChoices.Except(from R in Source select R.Name);
            if (missingDefaults.Count() > 0)
            {
                foreach (var T in missingDefaults)
                {
                    View.AddNewItem(new TaskType() { Name = T });
                    View.CommitNew();
                }
                if (await SaveChangesAsync()) Status = "Missing defaults were added.";
            }
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        protected override void AddNew()
        {
            CurrentEditItem = new Resource();
            View.AddNewItem(CurrentEditItem);
            base.AddNew();
        }
        #endregion
    }
}

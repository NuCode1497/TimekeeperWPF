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
        public TaskTypesViewModel() : base()
        {
            Sorter = NameSorter;
        }
        #region Properties
        public override string Name => nameof(Context.TaskTypes) + " Editor";
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        protected override bool CanCommit => base.CanCommit && IsNotDuplicate;
        private bool IsNotDuplicate => Source.Count(T => T.Name == CurrentEditItem.Name) == 1;
        protected override bool CanDeleteSelected => base.CanDeleteSelected && !SelectedItem.IsDefaultType;
        protected override bool CanEditSelected => base.CanEditSelected && !SelectedItem.IsDefaultType;
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TaskTypes.LoadAsync();
            Items.Source = Context.TaskTypes.Local;
            await AddMissingDefaultsAsync();
        }
        private async Task AddMissingDefaultsAsync()
        {
            var missingDefaults = TaskType.DefaultChoices.Except(from T in Source select T.Name);
            if (missingDefaults.Count() > 0)
            {
                foreach (var T in missingDefaults)
                {
                    View.AddNewItem(new TaskType() { Name = T });
                    View.CommitNew();
                }
                if(await SaveChangesAsync()) Status = "Missing defaults were added.";
            }
        }
        internal override void SaveAs()
        {
            throw new NotImplementedException();
        }
        internal override void AddNew(object ap)
        {
            CurrentEditItem = new TaskType();
            View.AddNewItem(CurrentEditItem);
            base.AddNew(ap);
        }
        #endregion
    }
}

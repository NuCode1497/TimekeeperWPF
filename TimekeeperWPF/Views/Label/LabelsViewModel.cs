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
        public LabelsViewModel() : base()
        {
            Sorter = NameSorter;
        }
        public override string Name => nameof(Context.Labels) + " Editor";
        protected override bool CanCommit => base.CanCommit && IsNotDuplicate;
        private bool IsNotDuplicate => Source.Count(L => L.Name == CurrentEditItem.Name) == 1;
        protected override bool CanSave => false;
        protected override async Task GetDataAsync()
        {
            //await Task.Delay(2000);
            Context = new TimeKeeperContext();
            await Context.Labels.LoadAsync();
            Items.Source = Context.Labels.Local;
        }
        internal override void SaveAs()
        {
            throw new NotImplementedException();
        }

        internal override void AddNew(object ap)
        {
            CurrentEditItem = new Label();
            View.AddNewItem(CurrentEditItem);
            base.AddNew(ap);
        }
    }
}

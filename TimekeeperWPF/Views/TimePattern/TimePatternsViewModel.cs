using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TimekeeperDAL.EF;

namespace TimekeeperWPF
{
    public class TimePatternsViewModel : LabeledEntitiesViewModel<TimePattern>
    {
        private ICommand _RemoveClauseCommand;
        private ICommand _AddClauseCommand;
        public override string Name => nameof(Context.TimePatterns) + " Editor";
        public CollectionViewSource ClausesCollection { get; set; }
        public ObservableCollection<TimePatternClause> ClausesSource => 
            ClausesCollection?.Source as ObservableCollection<TimePatternClause>;
        public ListCollectionView ClausesView => 
            ClausesCollection?.View as ListCollectionView;
        public ICommand RemoveClauseCommand => _RemoveClauseCommand
            ?? (_RemoveClauseCommand = new RelayCommand(ap => 
            RemoveClause(ap as TimePatternClause), pp => pp is TimePatternClause));
        public ICommand AddClauseCommand => _AddClauseCommand
            ?? (_AddClauseCommand = new RelayCommand(ap => AddClause(), pp => true));
        protected override bool CanCommit => base.CanCommit && !ClausesHaveErrors && Source.Count(C => C.Name == CurrentEditItem.Name) == 1;
        private bool ClausesHaveErrors => ClausesSource?.Count(C => C.HasErrors) > 0;
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimePatterns.LoadAsync();
            Items.Source = Context.TimePatterns.Local;

            await base.GetDataAsync();
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        protected override void AddNew()
        {
            CurrentEditItem = new TimePattern
            {
                Name = "New Time Pattern",
                Any = false
            };
            View.AddNewItem(CurrentEditItem);
            BeginEdit();
            AddClause();
            base.AddNew();
        }
        protected override void EditSelected()
        {
            base.EditSelected();
            BeginEdit();
        }
        private void BeginEdit()
        {
            ClausesCollection = new CollectionViewSource();
            ClausesCollection.Source = 
                new ObservableCollection<TimePatternClause>(CurrentEditItem.Query);
            OnPropertyChanged(nameof(ClausesView));
        }
        protected override void EndEdit()
        {
            ClausesCollection = null;
            base.EndEdit();
        }
        protected override void Commit()
        {
            CurrentEditItem.Query = new HashSet<TimePatternClause>(ClausesSource);
            base.Commit();
        }
        private void AddClause()
        {
            ClausesView.AddNew();
            ClausesView.CommitNew();
            OnPropertyChanged(nameof(ClausesView));
        }
        private void RemoveClause(TimePatternClause clause)
        {
            ClausesView.Remove(clause);
            OnPropertyChanged(nameof(ClausesView));
        }
        #endregion
    }
}

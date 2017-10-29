using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class TimePatternsViewModel : LabeledEntitiesViewModel<TimePattern>
    {
        private ICommand _RemoveClauseCommand;
        private ICommand _AddClauseCommand;
        public TimePatternsViewModel() : base() { }
        public override string Name => nameof(Context.TimePatterns) + " Editor";
        public CollectionViewSource ClausesCollection { get; set; }
        public ObservableCollection<TimePatternClause> ClausesSource => ClausesCollection?.Source as ObservableCollection<TimePatternClause>;
        public ListCollectionView ClausesView => ClausesCollection?.View as ListCollectionView;
        public ICommand RemoveClauseCommand => _RemoveClauseCommand
            ?? (_RemoveClauseCommand = new RelayCommand(ap => RemoveClause(ap as TimePatternClause), pp => pp is TimePatternClause));
        public ICommand AddClauseCommand => _AddClauseCommand
            ?? (_AddClauseCommand = new RelayCommand(ap => AddClause(), pp => true));
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
        protected override int AddNew()
        {
            int errors = base.AddNew();
            if (errors != 0) return errors;
            if (CurrentEditItem == null)
            {
                errors++;
                return errors;
            }
            CurrentEditItem.Name = "New Time Pattern";
            BeginEdit();
            return 0;
        }
        protected override void EditSelected()
        {
            base.EditSelected();
            BeginEdit();
        }
        private void BeginEdit()
        {
            if (!IsEditingItemOrAddingNew) return;

            ClausesCollection = new CollectionViewSource();
            ClausesCollection.Source = new ObservableCollection<TimePatternClause>(CurrentEditItem.Query);
            AddClause();
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
        private void RemoveClause(TimePatternClause ap)
        {
            ClausesView.Remove(ap);
            OnPropertyChanged(nameof(ClausesView));
        }
        #endregion
    }
}

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
    public abstract class LabeledEntitiesViewModel<ModelType> : ViewModel<ModelType>
        where ModelType : LabeledEntity, new()
    {
        #region Fields
        private bool _HasSelectedLabel = false;
        private Label _SelectedLabel;
        private ICommand _RemoveLabelCommand;
        private ICommand _AddLabelCommand;
        private LabelsViewModel LVM;
        #endregion
        public LabeledEntitiesViewModel() : base()
        {

        }
        #region Properties
        public CollectionViewSource LabelsCollection { get; set; }
        public CollectionViewSource CurrentEntityLabelsCollection { get; set; }
        public ObservableCollection<Label> LabelsSource => LabelsCollection?.Source as ObservableCollection<Label>;
        public ObservableCollection<Label> CurrentEntityLabelsSource => CurrentEntityLabelsCollection?.Source as ObservableCollection<Label>;
        public ListCollectionView LabelsView => LabelsCollection?.View as ListCollectionView;
        public ListCollectionView CurrentEntityLabelsView => CurrentEntityLabelsCollection?.View as ListCollectionView;
        public Label SelectedLabel
        {
            get
            {
                return _SelectedLabel;
            }
            set
            {
                //Label must not be itself and must be in LabelsSource
                if ((value == _SelectedLabel) || (value != null && (!LabelsSource?.Contains(value) ?? false))) return;
                _SelectedLabel = value;
                if (SelectedLabel == null)
                {
                    HasSelectedLabel = false;
                }
                else
                {
                    HasSelectedLabel = true;
                }
                OnPropertyChanged();
            }
        }
        #endregion
        #region Conditions
        public bool HasSelectedLabel
        {
            get
            {
                return _HasSelectedLabel;
            }
            protected set
            {
                _HasSelectedLabel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedLabel));
            }
        }
        public bool HasNotSelectedLabel => !HasSelectedLabel;
        #endregion
        #region Commands
        public ICommand RemoveLabelCommand => _RemoveLabelCommand
            ?? (_RemoveLabelCommand = new RelayCommand(ap => RemoveLabel(ap as Label), pp => pp is Label));
        public ICommand AddLabelCommand => _AddLabelCommand
            ?? (_AddLabelCommand = new RelayCommand(ap => AddLabel(), pp => CanAddLabel));
        #endregion
        #region Predicates
        private bool CanAddLabel => HasSelectedLabel && !(CurrentEntityLabelsSource?.Contains(SelectedLabel)??false);
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            LabelsCollection = new CollectionViewSource();
            await Context.Labels.LoadAsync();
            LabelsCollection.Source = Context.Labels.Local;
            LabelsView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(LabelsView));
        }
        protected override int AddNew()
        {
            int errors = base.AddNew();
            if (errors != 0) return errors;
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
            CurrentEntityLabelsCollection = new CollectionViewSource();
            CurrentEntityLabelsCollection.Source = new ObservableCollection<Label>(CurrentEditItem.Labels);
            UpdateViews();
        }
        protected override void EndEdit()
        {
            LabelsView.Filter = null;
            CurrentEntityLabelsCollection = null;
            base.EndEdit();
        }
        protected override void Commit()
        {
            CurrentEditItem.Labels = new HashSet<Label>(CurrentEntityLabelsSource);
            base.Commit();
        }
        private void AddLabel()
        {
            CurrentEntityLabelsView.AddNewItem(SelectedLabel);
            CurrentEntityLabelsView.CommitNew();
            SelectedLabel = null;
            UpdateViews();
        }
        private void RemoveLabel(Label ap)
        {
            CurrentEntityLabelsView.Remove(ap);
            UpdateViews();
        }
        private void UpdateViews()
        {
            LabelsView.Filter = L => CurrentEntityLabelsView.Contains(L) == false;
            OnPropertyChanged(nameof(CurrentEntityLabelsView));
            OnPropertyChanged(nameof(LabelsView));
        }
        #endregion
    }
}

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
        private bool _IsCreatingNewLabel = false;
        private bool _IsLoadingLabels = false;
        private Label _SelectedLabel;
        private ICommand _RemoveLabelCommand;
        private ICommand _AddLabelCommand;
        private ICommand _NewLabelCommand;
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
            get { return _SelectedLabel; }
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
            get { return _HasSelectedLabel; }
            protected set
            {
                _HasSelectedLabel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedLabel));
            }
        }
        public bool HasNotSelectedLabel => !HasSelectedLabel;
        public bool IsCreatingNewLabel
        {
            get { return _IsCreatingNewLabel; }
            set
            {
                _IsCreatingNewLabel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotCreatingNewLabel));
            }
        }
        public bool IsNotCreatingNewLabel => !IsCreatingNewLabel;
        public bool IsLoadingLabels
        {
            get { return _IsLoadingLabels; }
            set
            {
                _IsLoadingLabels = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoadingLabels));
            }
        }
        public bool IsNotLoadingLabels => !IsLoadingLabels;
        #endregion
        #region Commands
        public ICommand RemoveLabelCommand => _RemoveLabelCommand
            ?? (_RemoveLabelCommand = new RelayCommand(ap => RemoveLabel(ap as Label), pp => pp is Label));
        public ICommand AddLabelCommand => _AddLabelCommand
            ?? (_AddLabelCommand = new RelayCommand(ap => AddLabel(), pp => CanAddLabel));
        public ICommand NewLabelCommand => _NewLabelCommand
            ?? (_NewLabelCommand = new RelayCommand(ap => NewLabel(), pp => CanCreateNewLabel));
        #endregion
        #region Predicates
        private bool CanAddLabel => HasSelectedLabel 
            && !(CurrentEntityLabelsSource?.Contains(SelectedLabel)??false)
            && IsNotLoadingLabels
            && IsNotCreatingNewLabel;
        private bool CanCreateNewLabel => true;
        protected override bool CanCancel
        {
            get
            {
                if (IsCreatingNewLabel)
                {
                    return LVM.CancelCommand.CanExecute(null);
                }
                else
                {
                    return base.CanCancel;
                }
            }
        }
        protected override bool CanCommit
        {
            get
            {
                if (IsCreatingNewLabel)
                {
                    return LVM.CommitCommand.CanExecute(null);
                }
                else
                {
                    return base.CanCommit;
                }
            }
        }
        #endregion
        #region Actions
        private void NewLabel()
        {
            LVM = new LabelsViewModel();
            LVM.PropertyChanged += LVM_PropertyChanged;
            if (LVM.GetDataCommand.CanExecute(null))
                LVM.GetDataCommand.Execute(null);
        }

        private async void LVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(LVM.IsNotLoading):
                    if (LVM.IsNotLoading)
                    {
                        //wait for loading to finish then create new label
                        if (LVM.NewItemCommand.CanExecute(null))
                        {
                            IsCreatingNewLabel = true;
                            LVM.NewItemCommand.Execute(null);
                            SelectedLabel = LVM.CurrentEditItem;
                        }
                    }
                    break;
                case nameof(LVM.IsNotEditingItemOrAddingNew):
                    if (LVM.IsNotEditingItemOrAddingNew)
                    {
                        //end create new label
                        LVM.Dispose();
                        LVM.PropertyChanged -= LVM_PropertyChanged;
                        await GetLabelData();
                        IsCreatingNewLabel = false;
                    }
                    break;
            }
        }
        protected override async Task GetDataAsync()
        {
            await GetLabelData();
        }
        private async Task GetLabelData()
        {
            IsLoadingLabels = true;
            LabelsCollection = new CollectionViewSource();
            await Context.Labels.LoadAsync();
            LabelsCollection.Source = Context.Labels.Local;
            LabelsView.CustomSort = NameSorter;
            IsLoadingLabels = false;
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
        protected override void Cancel()
        {
            if (IsCreatingNewLabel)
            {
                if (LVM.CancelCommand.CanExecute(null))
                    LVM.CancelCommand.Execute(null);
            }
            else
            {
                base.Cancel();
            }
        }
        protected override void Commit()
        {
            if (IsCreatingNewLabel)
            {
                if (LVM.CommitCommand.CanExecute(null))
                    LVM.CommitCommand.Execute(null);
            }
            else
            {
                CurrentEditItem.Labels = new HashSet<Label>(CurrentEntityLabelsSource);
                base.Commit();
            }
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

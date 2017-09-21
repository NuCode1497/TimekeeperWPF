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
        #region Fields
        private bool _HasSelectedChild = false;
        private bool _HasSelectedResource = false;
        private bool _IsAddingNewAllocation = false;
        private TimePattern _SelectedChild;
        private Resource _SelectedResource;
        private Allocation _CurrentEditAllocation;
        private ICommand _AddChildCommand;
        private ICommand _RemoveChildCommand;
        private ICommand _DeleteAllocationCommand;
        private ICommand _AddNewAllocationCommand;
        private ICommand _CancelAllocationCommand;
        private ICommand _CommitAllocationCommand;
        #endregion
        public TimePatternsViewModel() : base()
        {

        }
        #region Properties
        public override string Name => nameof(Context.TimePatterns) + " Editor";
        public CollectionViewSource ResourcesCollection { get; set; }
        public CollectionViewSource CurrentEntityAllocationsCollection { get; set; }
        public CollectionViewSource TimePointsCollection { get; set; }
        public CollectionViewSource PatternsCollection { get; set; }
        public ObservableCollection<Resource> ResourcesSource => ResourcesCollection?.Source as ObservableCollection<Resource>;
        public ObservableCollection<Allocation> CurrentEntityAllocationsSource => CurrentEntityAllocationsCollection?.Source as ObservableCollection<Allocation>;
        public ObservableCollection<TimePoint> TimePointsSource => TimePointsCollection?.Source as ObservableCollection<TimePoint>;
        public ObservableCollection<TimePattern> PatternsSource => PatternsCollection?.Source as ObservableCollection<TimePattern>;
        public ListCollectionView ResourcesView => ResourcesCollection?.View as ListCollectionView;
        public ListCollectionView CurrentEntityAllocationsView => CurrentEntityAllocationsCollection?.View as ListCollectionView;
        public ListCollectionView TimePointsView => TimePointsCollection?.View as ListCollectionView;
        public ListCollectionView PatternsView => PatternsCollection?.View as ListCollectionView;
        public TimePattern SelectedChild
        {
            get
            {
                return _SelectedChild;
            }
            set
            {
                if (_SelectedChild == value) return;
                _SelectedChild = value;
                OnPropertyChanged();
            }
        }
        public Resource SelectedResource
        {
            get
            {
                return _SelectedResource;
            }
            set
            {
                //Must not be adding new allocation
                if (IsAddingNewAllocation)
                {
                    //prevent change and cause two way bindings to reselect this
                    OnPropertyChanged();
                    return;
                }
                //Resource must not be itself and must be in ResourcesSource
                if ((value == _SelectedResource) || (value != null && (!ResourcesSource?.Contains(value) ?? false))) return;
                _SelectedResource = value;
                if (SelectedResource == null)
                {
                    HasSelectedResource = false;
                }
                else
                {
                    HasSelectedResource = true;
                }
                OnPropertyChanged();
            }
        }
        public Allocation CurrentEditAllocation
        {
            get
            {
                return _CurrentEditAllocation;
            }
            protected set
            {
                //Must not be adding new allocation
                if (IsAddingNewAllocation)
                {
                    //prevent change and cause two way bindings to reselect this
                    OnPropertyChanged();
                    return;
                }
                if (value == _CurrentEditAllocation) return;
                _CurrentEditAllocation = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Conditions
        public bool HasSelectedChild
        {
            get
            {
                return _HasSelectedChild;
            }
            protected set
            {
                _HasSelectedChild = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedChild));
            }
        }
        public bool HasSelectedResource
        {
            get
            {
                return _HasSelectedResource;
            }
            protected set
            {
                _HasSelectedResource = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedResource));
            }
        }
        public bool IsAddingNewAllocation
        {
            get
            {
                return _IsAddingNewAllocation;
            }
            set
            {
                _IsAddingNewAllocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotAddingNewAllocation));
            }
        }
        public bool CurrentEntityHasChild => CurrentEditItem == null ? false : CurrentEditItem.Child == null ? false : true;
        public bool HasNotSelectedChild => !HasSelectedChild;
        public bool HasNotSelectedResource => !HasSelectedResource;
        public bool IsNotAddingNewAllocation => !IsAddingNewAllocation;
        #endregion
        #region Commands
        public ICommand AddChildCommand => _AddChildCommand
            ?? (_AddChildCommand = new RelayCommand(ap => AddChild(), pp => CanAddChild));
        public ICommand RemoveChildCommand => _RemoveChildCommand
            ?? (_RemoveChildCommand = new RelayCommand(ap => RemoveChild(), pp => true));
        public ICommand DeleteAllocationCommand => _DeleteAllocationCommand
            ?? (_DeleteAllocationCommand = new RelayCommand(ap => DeleteAllocation(ap as Allocation), pp => CanDeleteAllocation(pp)));
        public ICommand AddNewAllocationCommand => _AddNewAllocationCommand
            ?? (_AddNewAllocationCommand = new RelayCommand(ap => AddNewAllocation(), pp => CanAddNewAllocation));
        public ICommand CancelAllocationCommand => _CancelAllocationCommand
            ?? (_CancelAllocationCommand = new RelayCommand(ap => CancelAllocation(), pp => CanCancelAllocation));
        public ICommand CommitAllocationCommand => _CommitAllocationCommand
            ?? (_CommitAllocationCommand = new RelayCommand(ap => CommitAllocation(), pp => CanCommitAllocation));
        #endregion
        #region Predicates
        protected override bool CanCommit => base.CanCommit && IsNotAddingNewAllocation && !(CurrentEditItem.Child?.HasErrors??false);
        private bool CanAddChild => HasSelectedChild && IsValidChild(SelectedChild);
        private bool CanAddNewAllocation => HasSelectedResource && !IsResourceAllocated(SelectedResource);
        private bool CanCancelAllocation => IsAddingNewAllocation;
        private bool CanCommitAllocation => IsAddingNewAllocation && !CurrentEditAllocation.HasErrors;
        private bool CanDeleteAllocation(object pp)
        {
            return pp is Allocation 
                && IsNotAddingNewAllocation;
        }
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimePatterns.LoadAsync();
            Items.Source = Context.TimePatterns.Local;

            PatternsCollection = new CollectionViewSource();
            PatternsCollection.Source = Context.TimePatterns.Local;
            PatternsView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(PatternsView));
            
            ResourcesCollection = new CollectionViewSource();
            await Context.Resources.LoadAsync();
            ResourcesCollection.Source = Context.Resources.Local;
            ResourcesView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(ResourcesView));

            TimePointsCollection = new CollectionViewSource();
            await Context.TimePoints.LoadAsync();
            TimePointsCollection.Source = Context.TimePoints.Local;
            TimePointsView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(TimePointsView));

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
            //CurrentEditItem.Allocations;
            //CurrentEditItem.Child;
            //CurrentEditItem.Labels;
            CurrentEditItem.Duration = TimeSpan.FromHours(1).Ticks;
            CurrentEditItem.ForNth = 0;
            CurrentEditItem.ForSkipDuration = 0;
            CurrentEditItem.ForTimePoint = TimePointsSource.First();
            CurrentEditItem.ForX = 1;
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

            //get current edit entity allocations
            CurrentEntityAllocationsCollection = new CollectionViewSource();
            CurrentEntityAllocationsCollection.Source = new ObservableCollection<Allocation>(CurrentEditItem.Allocations);

            //subscribe to duration changes
            //duration changes affect child choices
            CurrentEditItem.PropertyChanged += CurrentEditItem_DurationChanges;

            //update views
            UpdateAllocationsView();
            UpdatePatternsView();
        }
        private void CurrentEditItem_DurationChanges(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentEditItem.Duration))
            {
                UpdatePatternsView();
            }
        }
        protected override void EndEdit()
        {
            //clear filters
            PatternsView.Filter = null;
            ResourcesView.Filter = null;

            EndEditAllocation();
            CurrentEntityAllocationsCollection = null;

            //unsubscribe
            CurrentEditItem.PropertyChanged -= CurrentEditItem_DurationChanges;

            base.EndEdit();
        }
        protected override void Cancel()
        {
            CancelAllocation();
            base.Cancel();
        }
        protected override void Commit()
        {
            CurrentEditItem.Allocations = new HashSet<Allocation>(CurrentEntityAllocationsSource);
            base.Commit();
        }
        private void AddNewAllocation()
        {
            CurrentEditAllocation = new Allocation()
            {
                maxAmount = 0,
                minAmount = 0,
                Resource = SelectedResource,
                TimePattern = CurrentEditItem
            };
            IsAddingNewAllocation = true;
        }
        private void EndEditAllocation()
        {
            IsAddingNewAllocation = false;
            CurrentEditAllocation = null;
        }
        private void CancelAllocation()
        {
            if (IsAddingNewAllocation) EndEditAllocation();
        }
        private void CommitAllocation()
        {
            if (IsAddingNewAllocation)
            {
                CurrentEntityAllocationsView.AddNewItem(CurrentEditAllocation);
                CurrentEntityAllocationsView.CommitNew();
                EndEditAllocation();
                UpdateAllocationsView();
            }
        }
        private void DeleteAllocation(Allocation ap)
        {
            CurrentEntityAllocationsView.Remove(ap);
            UpdateAllocationsView();
        }
        private void AddChild()
        {
            CurrentEditItem.Child = SelectedChild;
            SelectedChild = null;
            UpdatePatternsView();
        }
        private void RemoveChild()
        {
            CurrentEditItem.Child = null;
            UpdatePatternsView();
        }
        private void UpdatePatternsView()
        {
            PatternsView.Filter = P => IsValidChild((TimePattern)P);
            OnPropertyChanged(nameof(PatternsView));
            OnPropertyChanged(nameof(CurrentEntityHasChild));
        }
        private void UpdateAllocationsView()
        {
            //filter out allocated resources
            ResourcesView.Filter = R => !IsResourceAllocated((Resource)R);
            OnPropertyChanged(nameof(ResourcesView));
            OnPropertyChanged(nameof(CurrentEntityAllocationsView));
        }
        private bool IsValidChild(TimePattern P)
        {
            return CurrentEditItem.Duration > P.Duration 
                && P != CurrentEditItem.Child;
        }
        private bool IsResourceAllocated(Resource R)
        {
            //count number of allocations of resource R
            //if count is > 0 then yes resource is allocated return true
            return (CurrentEntityAllocationsSource?.Count(A => A.Resource == R) > 0);
        }
        #endregion
    }
}

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
    public class TimeTasksViewModel : TypedLabeledEntitiesViewModel<TimeTask>
    {
        #region Fields
        private bool _HasSelectedResource = false;
        private bool _IsAddingNewAllocation = false;
        private Resource _SelectedResource;
        private Allocation _CurrentEditAllocation;
        private ICommand _DeleteAllocationCommand;
        private ICommand _AddNewAllocationCommand;
        private ICommand _CancelAllocationCommand;
        private ICommand _CommitAllocationCommand;
        private bool _HasSelectedInclude = false;
        private bool _HasSelectedExclude = false;
        private TimePattern _SelectedInclude;
        private TimePattern _SelectedExclude;
        private ICommand _RemoveIncludeCommand;
        private ICommand _RemoveExcludeCommand;
        private ICommand _AddIncludeCommand;
        private ICommand _AddExcludeCommand;
        #endregion
        public TimeTasksViewModel() : base() { }
        #region Properties
        public override string Name => nameof(Context.TimeTasks) + " Editor";
        #region Allocations
        public CollectionViewSource ResourcesCollection { get; set; }
        public ObservableCollection<Resource> ResourcesSource => ResourcesCollection?.Source as ObservableCollection<Resource>;
        public ListCollectionView ResourcesView => ResourcesCollection?.View as ListCollectionView;
        public CollectionViewSource CurrentEntityAllocationsCollection { get; set; }
        public ObservableCollection<Allocation> CurrentEntityAllocationsSource => CurrentEntityAllocationsCollection?.Source as ObservableCollection<Allocation>;
        public ListCollectionView CurrentEntityAllocationsView => CurrentEntityAllocationsCollection?.View as ListCollectionView;
        public Resource SelectedResource
        {
            get { return _SelectedResource; }
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
            get { return _CurrentEditAllocation; }
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
        #region Patterns
        public CollectionViewSource PatternsCollection { get; set; }
        public CollectionViewSource CurrentEntityIncludesCollection { get; set; }
        public CollectionViewSource CurrentEntityExcludesCollection { get; set; }
        public ObservableCollection<TimePattern> PatternsSource => PatternsCollection?.Source as ObservableCollection<TimePattern>;
        public ObservableCollection<TimePattern> CurrentEntityIncludesSource => CurrentEntityIncludesCollection?.Source as ObservableCollection<TimePattern>;
        public ObservableCollection<TimePattern> CurrentEntityExcludesSource => CurrentEntityExcludesCollection?.Source as ObservableCollection<TimePattern>;
        public ListCollectionView PatternsView => PatternsCollection?.View as ListCollectionView;
        public ListCollectionView CurrentEntityIncludesView => CurrentEntityIncludesCollection?.View as ListCollectionView;
        public ListCollectionView CurrentEntityExcludesView => CurrentEntityExcludesCollection?.View as ListCollectionView;
        public TimePattern SelectedInclude
        {
            get { return _SelectedInclude; }
            set
            {
                //Pattern must not be itself and must be in PatternsSource
                if ((value == _SelectedInclude) || (value != null && (!PatternsSource?.Contains(value) ?? false))) return;
                _SelectedInclude = value;
                if (SelectedInclude == null)
                {
                    HasSelectedInclude = false;
                }
                else
                {
                    if (SelectedInclude == SelectedExclude)
                    {
                        SelectedExclude = null;
                    }
                    HasSelectedInclude = true;
                }
                OnPropertyChanged();
            }
        }
        public TimePattern SelectedExclude
        {
            get { return _SelectedExclude; }
            set
            {
                //Pattern must not be itself and must be in PatternsSource
                if ((value == _SelectedExclude) || (value != null && (!PatternsSource?.Contains(value) ?? false))) return;
                _SelectedExclude = value;
                if (SelectedExclude == null)
                {
                    HasSelectedExclude = false;
                }
                else
                {
                    if (SelectedExclude == SelectedInclude)
                    {
                        SelectedInclude = null;
                    }
                    HasSelectedExclude = true;
                }
                OnPropertyChanged();
            }
        }
        #endregion
        #endregion
        #region Conditions
        public bool HasSelectedResource
        {
            get { return _HasSelectedResource; }
            protected set
            {
                _HasSelectedResource = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedResource));
            }
        }
        public bool IsAddingNewAllocation
        {
            get { return _IsAddingNewAllocation; }
            set
            {
                _IsAddingNewAllocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotAddingNewAllocation));
            }
        }
        public bool HasNotSelectedResource => !HasSelectedResource;
        public bool IsNotAddingNewAllocation => !IsAddingNewAllocation;
        public bool HasSelectedInclude
        {
            get { return _HasSelectedInclude; }
            protected set
            {
                _HasSelectedInclude = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedInclude));
            }
        }
        public bool HasSelectedExclude
        {
            get { return _HasSelectedExclude; }
            protected set
            {
                _HasSelectedExclude = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedExclude));
            }
        }
        public bool HasNotSelectedInclude => !HasSelectedInclude;
        public bool HasNotSelectedExclude => !HasSelectedExclude;
        #endregion
        #region Commands
        public ICommand DeleteAllocationCommand => _DeleteAllocationCommand
            ?? (_DeleteAllocationCommand = new RelayCommand(ap => DeleteAllocation(ap as Allocation), pp => CanDeleteAllocation(pp)));
        public ICommand AddNewAllocationCommand => _AddNewAllocationCommand
            ?? (_AddNewAllocationCommand = new RelayCommand(ap => AddNewAllocation(), pp => CanAddNewAllocation));
        public ICommand CancelAllocationCommand => _CancelAllocationCommand
            ?? (_CancelAllocationCommand = new RelayCommand(ap => CancelAllocation(), pp => CanCancelAllocation));
        public ICommand CommitAllocationCommand => _CommitAllocationCommand
            ?? (_CommitAllocationCommand = new RelayCommand(ap => CommitAllocation(), pp => CanCommitAllocation));
        public ICommand RemoveIncludeCommand => _RemoveIncludeCommand
            ?? (_RemoveIncludeCommand = new RelayCommand(ap => RemoveInclude(ap as TimePattern), pp => pp is TimePattern));
        public ICommand RemoveExcludeCommand => _RemoveExcludeCommand
            ?? (_RemoveExcludeCommand = new RelayCommand(ap => RemoveExclude(ap as TimePattern), pp => pp is TimePattern));
        public ICommand AddIncludeCommand => _AddIncludeCommand
            ?? (_AddIncludeCommand = new RelayCommand(ap => AddInclude(), pp => CanAddInclude));
        public ICommand AddExcludeCommand => _AddExcludeCommand
            ?? (_AddExcludeCommand = new RelayCommand(ap => AddExclude(), pp => CanAddExclude));
        #endregion
        #region Predicates
        protected override bool CanCommit => base.CanCommit && IsNotAddingNewAllocation;
        protected override bool CanSave => false;
        private bool CanAddNewAllocation => HasSelectedResource && !IsResourceAllocated(SelectedResource);
        private bool CanCancelAllocation => IsAddingNewAllocation;
        private bool CanCommitAllocation => IsAddingNewAllocation && !CurrentEditAllocation.HasErrors;
        private bool CanDeleteAllocation(object pp)
        {
            return pp is Allocation
                && IsNotAddingNewAllocation;
        }
        private bool CanAddInclude => HasSelectedInclude && !(CurrentEntityIncludesSource?.Contains(SelectedInclude)??false);
        private bool CanAddExclude => HasSelectedExclude && !(CurrentEntityExcludesSource?.Contains(SelectedExclude)??false);
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimeTasks.LoadAsync();
            Items.Source = Context.TimeTasks.Local;

            ResourcesCollection = new CollectionViewSource();
            await Context.Resources.LoadAsync();
            ResourcesCollection.Source = Context.Resources.Local;
            ResourcesView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(ResourcesView));

            PatternsCollection = new CollectionViewSource();
            await Context.TimePatterns.LoadAsync();
            PatternsCollection.Source = Context.TimePatterns.Local;
            PatternsView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(PatternsView));

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
            CurrentEditItem.Name = "New Task";
            CurrentEditItem.Description = "Your text here.";
            CurrentEditItem.End = DateTime.Now.RoundToHour().AddHours(1); //init before start
            CurrentEditItem.Start = DateTime.Now.RoundToHour();
            CurrentEditItem.Dimension = 1;
            CurrentEditItem.PowerLevel = 100;
            CurrentEditItem.Priority = 1;
            CurrentEditItem.AsksForCheckin = false;
            CurrentEditItem.AsksForReschedule = false;
            CurrentEditItem.CanReschedule = false;
            CurrentEditItem.RaiseOnReschedule = false;
            CurrentEditItem.CanBeEarly = false;
            CurrentEditItem.CanBeLate = false;
            CurrentEditItem.CanBePushed = false;
            CurrentEditItem.CanInflate = false;
            CurrentEditItem.CanDeflate = false;
            CurrentEditItem.CanFill = false;
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

            CurrentEntityAllocationsCollection = new CollectionViewSource();
            CurrentEntityAllocationsCollection.Source = new ObservableCollection<Allocation>(CurrentEditItem.Allocations);
            UpdateAllocationsView();

            CurrentEntityIncludesCollection = new CollectionViewSource();
            CurrentEntityIncludesCollection.Source = new ObservableCollection<TimePattern>(CurrentEditItem.IncludedPatterns);
            CurrentEntityExcludesCollection = new CollectionViewSource();
            CurrentEntityExcludesCollection.Source = new ObservableCollection<TimePattern>(CurrentEditItem.ExcludedPatterns);
            UpdateViews();
        }
        protected override void EndEdit()
        {
            ResourcesView.Filter = null;
            EndEditAllocation();
            CurrentEntityAllocationsCollection = null;

            CurrentEntityIncludesCollection = null;
            CurrentEntityExcludesCollection = null;
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
            CurrentEditItem.IncludedPatterns = new HashSet<TimePattern>(CurrentEntityIncludesSource);
            CurrentEditItem.ExcludedPatterns = new HashSet<TimePattern>(CurrentEntityExcludesSource);
            base.Commit();
        }
        private void AddInclude()
        {
            CurrentEntityIncludesView.AddNewItem(SelectedInclude);
            CurrentEntityIncludesView.CommitNew();
            SelectedInclude = null;
            UpdateViews();
        }
        private void AddExclude()
        {
            CurrentEntityExcludesView.AddNewItem(SelectedExclude);
            CurrentEntityExcludesView.CommitNew();
            SelectedExclude = null;
            UpdateViews();
        }
        private void RemoveExclude(TimePattern ap)
        {
            CurrentEntityExcludesView.Remove(ap);
            UpdateViews();
        }
        private void RemoveInclude(TimePattern ap)
        {
            CurrentEntityIncludesView.Remove(ap);
            UpdateViews();
        }
        private void UpdateViews()
        {
            PatternsView.Filter = P =>
            {
                return CurrentEntityIncludesView.Contains(P) == false 
                    && CurrentEntityExcludesView.Contains(P) == false;
            };
            OnPropertyChanged(nameof(PatternsView));
            OnPropertyChanged(nameof(CurrentEntityIncludesView));
            OnPropertyChanged(nameof(CurrentEntityExcludesView));
        }
        private void AddNewAllocation()
        {
            CurrentEditAllocation = new Allocation()
            {
                Amount = 0,
                Resource = SelectedResource,
                TimeTask = CurrentEditItem
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
        private void UpdateAllocationsView()
        {
            //filter out allocated resources
            ResourcesView.Filter = R => !IsResourceAllocated((Resource)R);
            OnPropertyChanged(nameof(ResourcesView));
            OnPropertyChanged(nameof(CurrentEntityAllocationsView));
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

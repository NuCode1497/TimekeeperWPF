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
        private ICommand _DeleteFilterCommand;
        private ICommand _AddNewFilterCommand;
        #endregion
        #region Properties
        public override string Name => nameof(Context.TimeTasks) + " Editor";

        //List of resources to choose from when creating an allocation
        public CollectionViewSource ResourcesCollection { get; set; }
        public ObservableCollection<Resource> ResourcesSource => 
            ResourcesCollection?.Source as ObservableCollection<Resource>;
        public ListCollectionView ResourcesView => 
            ResourcesCollection?.View as ListCollectionView;

        //The current editing TimeTask's allocations
        public CollectionViewSource AllocationsCollection { get; set; }
        public ObservableCollection<Allocation> AllocationsSource => 
            AllocationsCollection?.Source as ObservableCollection<Allocation>;
        public ListCollectionView AllocationsView => 
            AllocationsCollection?.View as ListCollectionView;

        //The current editing TimeTask's filters
        public CollectionViewSource FiltersCollection { get; set; }
        public ObservableCollection<TimeTaskFilter> FiltersSource =>
            FiltersCollection?.Source as ObservableCollection<TimeTaskFilter>;
        public ListCollectionView FiltersView =>
            FiltersCollection?.View as ListCollectionView;

        //The rest of these are lists to choose from when selecting a Filterable for a Filter
        public CollectionViewSource FilterLabelsCollection { get; set; }
        public ObservableCollection<Label> FilterLabelsSource =>
            FilterLabelsCollection?.Source as ObservableCollection<Label>;
        public ListCollectionView FilterLabelsView =>
            FilterLabelsCollection?.View as ListCollectionView;

        public CollectionViewSource FilterNotesCollection { get; set; }
        public ObservableCollection<Note> FilterNotesSource =>
            FilterNotesCollection?.Source as ObservableCollection<Note>;
        public ListCollectionView FilterNotesView =>
            FilterNotesCollection?.View as ListCollectionView;

        public CollectionViewSource FilterPatternsCollection { get; set; }
        public ObservableCollection<TimePattern> FilterPatternsSource =>
            FilterPatternsCollection?.Source as ObservableCollection<TimePattern>;
        public ListCollectionView FilterPatternsView =>
            FilterPatternsCollection?.View as ListCollectionView;

        public CollectionViewSource FilterResourcesCollection { get; set; }
        public ObservableCollection<Resource> FilterResourcesSource =>
            FilterResourcesCollection?.Source as ObservableCollection<Resource>;
        public ListCollectionView FilterResourcesView =>
            FilterResourcesCollection?.View as ListCollectionView;

        public CollectionViewSource FilterTasksCollection { get; set; }
        public ObservableCollection<TimeTask> FilterTasksSource =>
            FilterTasksCollection?.Source as ObservableCollection<TimeTask>;
        public ListCollectionView FilterTasksView =>
            FilterTasksCollection?.View as ListCollectionView;

        public CollectionViewSource FilterTaskTypesCollection { get; set; }
        public ObservableCollection<TaskType> FilterTaskTypesSource =>
            FilterTaskTypesCollection?.Source as ObservableCollection<TaskType>;
        public ListCollectionView FilterTaskTypesView =>
            FilterTaskTypesCollection?.View as ListCollectionView;

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
        public ICommand DeleteFilterCommand => _DeleteFilterCommand
            ?? (_DeleteFilterCommand = new RelayCommand(ap => DeleteFilter(ap as TimeTaskFilter), pp => pp is TimeTaskFilter));
        public ICommand AddNewFilterCommand => _AddNewFilterCommand
            ?? (_AddNewFilterCommand = new RelayCommand(ap => AddNewFilter(), pp => true));
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
        private bool ValidateFilters()
        {
            if (FiltersView == null) return false;
            bool noErrors = true;
            foreach (TimeTaskFilter f in FiltersView)
            {
                if (f.TypeChoice == ""
                    || f.Filterable == null)
                {
                    noErrors = false;
                }
            }
            return noErrors;
        }
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimeTasks.LoadAsync();
            Items.Source = Context.TimeTasks.Local;

            await base.GetDataAsync();

            ResourcesCollection = new CollectionViewSource();
            await Context.Resources.LoadAsync();
            ResourcesCollection.Source = Context.Resources.Local;
            ResourcesView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(ResourcesView));

            FilterLabelsCollection = new CollectionViewSource();
            FilterLabelsCollection.Source = Context.Labels.Local;
            FilterLabelsView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(FilterLabelsView));

            FilterNotesCollection = new CollectionViewSource();
            await Context.Notes.LoadAsync();
            FilterNotesCollection.Source = Context.Notes.Local;
            FilterNotesView.CustomSort = new NoteDateTimeSorterDesc();
            OnPropertyChanged(nameof(FilterNotesView));

            FilterPatternsCollection = new CollectionViewSource();
            await Context.TimePatterns.LoadAsync();
            FilterPatternsCollection.Source = Context.TimePatterns.Local;
            FilterPatternsView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(FilterPatternsView));

            FilterResourcesCollection = new CollectionViewSource();
            FilterResourcesCollection.Source = Context.Resources.Local;
            FilterResourcesView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(FilterResourcesView));

            FilterTasksCollection = new CollectionViewSource();
            FilterTasksCollection.Source = Context.TimeTasks.Local;
            FilterTasksView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(FilterTasksView));

            FilterTaskTypesCollection = new CollectionViewSource();
            FilterTaskTypesCollection.Source = Context.TaskTypes.Local;
            FilterTaskTypesView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(FilterTaskTypesView));
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

            AllocationsCollection = new CollectionViewSource();
            AllocationsCollection.Source = new ObservableCollection<Allocation>(CurrentEditItem.Allocations);
            UpdateAllocationsView();

            FiltersCollection = new CollectionViewSource();
            FiltersCollection.Source = new ObservableCollection<TimeTaskFilter>(CurrentEditItem.Filters);
            OnPropertyChanged(nameof(FiltersView));
        }
        protected override void EndEdit()
        {
            ResourcesView.Filter = null;
            EndEditAllocation();
            AllocationsCollection = null;
            FiltersCollection = null;
            base.EndEdit();
        }
        protected override void Cancel()
        {
            CancelAllocation();
            base.Cancel();
        }
        protected override void Commit()
        {
            CurrentEditItem.Allocations = new HashSet<Allocation>(AllocationsSource);
            CurrentEditItem.Filters = new HashSet<TimeTaskFilter>(FiltersSource);
            base.Commit();
        }
        private void AddNewFilter()
        {
            FiltersView.AddNew();
            ((TimeTaskFilter)FiltersView.CurrentAddItem).Include = true;
            FiltersView.CommitNew();
            OnPropertyChanged(nameof(FiltersView));
        }
        private void DeleteFilter(TimeTaskFilter filter)
        {
            FiltersView.Remove(filter);
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
                AllocationsView.AddNewItem(CurrentEditAllocation);
                AllocationsView.CommitNew();
                EndEditAllocation();
                UpdateAllocationsView();
            }
        }
        private void DeleteAllocation(Allocation ap)
        {
            AllocationsView.Remove(ap);
            UpdateAllocationsView();
        }
        private void UpdateAllocationsView()
        {
            //filter out allocated resources
            ResourcesView.Filter = R => !IsResourceAllocated((Resource)R);
            OnPropertyChanged(nameof(ResourcesView));
            OnPropertyChanged(nameof(AllocationsView));
        }
        private bool IsResourceAllocated(Resource R)
        {
            //count number of allocations of resource R
            //if count is > 0 then yes resource is allocated return true
            return (AllocationsSource?.Count(A => A.Resource == R) > 0);
        }
        #endregion
    }
}

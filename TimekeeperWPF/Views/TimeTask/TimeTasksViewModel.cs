using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TimekeeperDAL.EF;
using TimekeeperDAL.Tools;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class TimeTasksViewModel : TypedLabeledEntitiesViewModel<TimeTask>
    {
        #region Fields
        private bool _HasSelectedResource = false;
        private bool _TogglePer = false;
        private bool _IsAddingNewAllocation = false;
        private Resource _SelectedResource;
        private TimeTaskAllocation _CurrentEditAllocation;
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
        public CollectionViewSource ResourcesCollection { get; protected set; }
        public ObservableCollection<Resource> ResourcesSource =>
            ResourcesCollection?.Source as ObservableCollection<Resource>;
        public ListCollectionView ResourcesView =>
            ResourcesCollection?.View as ListCollectionView;

        public CollectionViewSource PersCollection { get; protected set; }
        public ObservableCollection<Resource> PersSource =>
            PersCollection?.Source as ObservableCollection<Resource>;
        public ListCollectionView PersView =>
            PersCollection?.View as ListCollectionView;

        //The current editing TimeTask's allocations
        public CollectionViewSource AllocationsCollection { get; protected set; }
        public ObservableCollection<TimeTaskAllocation> AllocationsSource => 
            AllocationsCollection?.Source as ObservableCollection<TimeTaskAllocation>;
        public ListCollectionView AllocationsView => 
            AllocationsCollection?.View as ListCollectionView;

        //The current editing TimeTask's filters
        public CollectionViewSource FiltersCollection { get; protected set; }
        public ObservableCollection<TimeTaskFilter> FiltersSource =>
            FiltersCollection?.Source as ObservableCollection<TimeTaskFilter>;
        public ListCollectionView FiltersView =>
            FiltersCollection?.View as ListCollectionView;

        //The rest of these are lists to choose from when selecting a Filterable for a Filter
        public CollectionViewSource FilterLabelsCollection { get; protected set; }
        public ObservableCollection<Label> FilterLabelsSource =>
            FilterLabelsCollection?.Source as ObservableCollection<Label>;
        public ListCollectionView FilterLabelsView =>
            FilterLabelsCollection?.View as ListCollectionView;
        
        public CollectionViewSource FilterPatternsCollection { get; protected set; }
        public ObservableCollection<TimePattern> FilterPatternsSource =>
            FilterPatternsCollection?.Source as ObservableCollection<TimePattern>;
        public ListCollectionView FilterPatternsView =>
            FilterPatternsCollection?.View as ListCollectionView;

        public CollectionViewSource FilterResourcesCollection { get; protected set; }
        public ObservableCollection<Resource> FilterResourcesSource =>
            FilterResourcesCollection?.Source as ObservableCollection<Resource>;
        public ListCollectionView FilterResourcesView =>
            FilterResourcesCollection?.View as ListCollectionView;

        public CollectionViewSource FilterTasksCollection { get; protected set; }
        public ObservableCollection<TimeTask> FilterTasksSource =>
            FilterTasksCollection?.Source as ObservableCollection<TimeTask>;
        public ListCollectionView FilterTasksView =>
            FilterTasksCollection?.View as ListCollectionView;

        public CollectionViewSource FilterTaskTypesCollection { get; protected set; }
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
                if (_SelectedResource == null)
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
        public bool TogglePer
        {
            get { return _TogglePer; }
            set
            {
                _TogglePer = value;
                OnPropertyChanged();
                CurrentEditAllocation.OnPropertyChanged(nameof(TimeTaskAllocation.Per));
                CurrentEditAllocation.OnPropertyChanged(nameof(TimeTaskAllocation.Amount));
            }
        }
        public TimeTaskAllocation CurrentEditAllocation
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
            ?? (_DeleteAllocationCommand = new RelayCommand(ap => 
            DeleteAllocation(ap as TimeTaskAllocation), pp => CanDeleteAllocation(pp)));
        public ICommand AddNewAllocationCommand => _AddNewAllocationCommand
            ?? (_AddNewAllocationCommand = new RelayCommand(ap => 
            AddNewAllocation(), pp => CanAddNewAllocation));
        public ICommand CancelAllocationCommand => _CancelAllocationCommand
            ?? (_CancelAllocationCommand = new RelayCommand(ap => 
            CancelAllocation(), pp => CanCancelAllocation));
        public ICommand CommitAllocationCommand => _CommitAllocationCommand
            ?? (_CommitAllocationCommand = new RelayCommand(ap => 
            CommitAllocation(), pp => CanCommitAllocation));
        public ICommand DeleteFilterCommand => _DeleteFilterCommand
            ?? (_DeleteFilterCommand = new RelayCommand(ap => 
            DeleteFilter(ap as TimeTaskFilter), pp => pp is TimeTaskFilter));
        public ICommand AddNewFilterCommand => _AddNewFilterCommand
            ?? (_AddNewFilterCommand = new RelayCommand(ap => AddNewFilter(), pp => true));
        #endregion
        #region Predicates
        protected override bool CanCommit => 
            base.CanCommit && IsNotAddingNewAllocation &&
            !FiltersHaveErrors;
        protected override bool CanSave => false;
        private bool CanAddNewAllocation =>
            IsEditingItemOrAddingNew &&
            IsNotAddingNewAllocation &&
            HasSelectedResource && 
            !IsResourceAllocated(SelectedResource);
        private bool CanCancelAllocation =>
            IsEditingItemOrAddingNew && 
            IsAddingNewAllocation;
        private bool CanCommitAllocation =>
            IsEditingItemOrAddingNew &&
            IsAddingNewAllocation &&
            (CurrentEditAllocation.Per != SelectedResource) &&
            !CurrentEditAllocation.HasErrors && 
            (TogglePer ? CurrentEditAllocation.Per != null : true);
        private bool CanDeleteAllocation(object pp) =>
            IsEditingItemOrAddingNew &&
            IsNotAddingNewAllocation &&
            pp is TimeTaskAllocation;
        private bool FiltersHaveErrors => FiltersSource?.Count(F => F.HasErrors) > 0;
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimeTasks.LoadAsync();
            await Context.Resources.LoadAsync();
            await Context.TimePatterns.LoadAsync();
            await base.GetDataAsync();

            Items.Source = Context.TimeTasks.Local;

            ResourcesCollection = new CollectionViewSource();
            ResourcesCollection.Source = Context.Resources.Local;
            ResourcesView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(ResourcesView));

            PersCollection = new CollectionViewSource();
            PersCollection.Source = Context.Resources.Local;
            PersView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(PersView));

            FilterLabelsCollection = new CollectionViewSource();
            FilterLabelsCollection.Source = Context.Labels.Local;
            FilterLabelsView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(FilterLabelsView));
            
            FilterPatternsCollection = new CollectionViewSource();
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
        internal override void SaveAs()
        {
            throw new NotImplementedException();
        }
        internal override void AddNew()
        {
            CurrentEditItem = new TimeTask
            {
                Name = "New Task",
                Description = "Your text here.",
                End = DateTime.Now.Date.AddDays(1), //init before start
                Start = DateTime.Now.Date,
                TaskType = TaskTypesSource.First(N => N.Name == "Chore"),
                AllocationMethod = "EvenEager",
                Dimension = 1,
                Priority = 100,
                CanFill = false
            };
            View.AddNewItem(CurrentEditItem);
            BeginEdit();
            base.AddNew();
        }
        internal override void EditSelected()
        {
            base.EditSelected();
            BeginEdit();
        }
        private void BeginEdit()
        {
            AllocationsCollection = new CollectionViewSource();
            AllocationsCollection.Source = new ObservableCollection<TimeTaskAllocation>(CurrentEditItem.Allocations);
            UpdateAllocationsView();

            FiltersCollection = new CollectionViewSource();
            FiltersCollection.Source = new ObservableCollection<TimeTaskFilter>(CurrentEditItem.Filters);
            OnPropertyChanged(nameof(FiltersView));
            foreach (var f in FiltersSource)
            {
                var t = f.FilterTypeName;
                switch (t)
                {
                    case nameof(Label):
                        f.TypeChoice = "Label";
                        break;
                    case nameof(Note):
                        f.TypeChoice = "Note";
                        break;
                    case nameof(TimePattern):
                        f.TypeChoice = "Pattern";
                        break;
                    case nameof(Resource):
                        f.TypeChoice = "Resource";
                        break;
                    case nameof(TimeTask):
                        f.TypeChoice = "Task";
                        break;
                    case nameof(TaskType):
                        f.TypeChoice = "Task Type";
                        break;
                }
                f.OnPropertyChanged(nameof(f.TypeChoice));
                f.OnPropertyChanged(nameof(f.Filterable));
            }
        }
        protected override void EndEdit()
        {
            if (ResourcesView != null) ResourcesView.Filter = null;
            EndEditAllocation();
            AllocationsCollection = null;
            FiltersCollection = null;
            base.EndEdit();
        }
        internal override void Cancel()
        {
            CancelAllocation();
            base.Cancel();
        }
        internal override async Task<bool> Commit()
        {
            CurrentEditItem.Allocations = new HashSet<TimeTaskAllocation>(AllocationsSource);
            CurrentEditItem.Filters = new HashSet<TimeTaskFilter>(FiltersSource);
            return await base.Commit();
        }
        private void AddNewFilter()
        {
            FiltersView.AddNewItem(new TimeTaskFilter
            {
                Include = true,
                TypeChoice = "Pattern"
            });
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
            CurrentEditAllocation = new TimeTaskAllocation()
            {
                Amount = 0,
                Resource = SelectedResource,
                TimeTask = CurrentEditItem
            };
            TogglePer = false;
            IsAddingNewAllocation = true;
            UpdatePersView();
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
                if (!TogglePer) CurrentEditAllocation.Per = null;
                AllocationsView.AddNewItem(CurrentEditAllocation);
                AllocationsView.CommitNew();
                EndEditAllocation();
                UpdateAllocationsView();
            }
        }
        private void DeleteAllocation(TimeTaskAllocation ap)
        {
            AllocationsView.Remove(ap);
            UpdateAllocationsView();
        }
        private void UpdateAllocationsView()
        {
            //filter out allocated resources
            bool timeAllocationExists = AllocationsSource.Count(A => A.Resource.IsTimeResource) > 0;
            ResourcesView.Filter = R => 
                (timeAllocationExists ? !((Resource)R).IsTimeResource : true) &&
                !IsResourceAllocated((Resource)R);
            OnPropertyChanged(nameof(ResourcesView));
            OnPropertyChanged(nameof(AllocationsView));
        }
        private void UpdatePersView()
        {
            //filter out resource pers that won't work based on SelectedResource
            if (SelectedResource.IsTimeResource)
            {
                PersView.Filter = R =>
                    ((Resource)R).IsTimeResource &&
                    ((Resource)R).AsTimeSpan() > SelectedResource.AsTimeSpan();
            }
            else
            {
                PersView.Filter = R => R != SelectedResource;
            }
            OnPropertyChanged(nameof(PersView));
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

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
        private ICommand _DeleteAllocationCommand;
        private ICommand _AddNewAllocationCommand;
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
        #endregion
        #region Commands
        public ICommand DeleteAllocationCommand => _DeleteAllocationCommand
            ?? (_DeleteAllocationCommand = new RelayCommand(ap => 
            DeleteAllocation(ap as TimeTaskAllocation), pp => pp is TimeTaskAllocation));
        public ICommand AddNewAllocationCommand => _AddNewAllocationCommand
            ?? (_AddNewAllocationCommand = new RelayCommand(ap => 
            AddNewAllocation(), pp => true));
        public ICommand DeleteFilterCommand => _DeleteFilterCommand
            ?? (_DeleteFilterCommand = new RelayCommand(ap => 
            DeleteFilter(ap as TimeTaskFilter), pp => pp is TimeTaskFilter));
        public ICommand AddNewFilterCommand => _AddNewFilterCommand
            ?? (_AddNewFilterCommand = new RelayCommand(ap => 
            AddNewFilter(), pp => true));
        #endregion
        #region Predicates
        protected override bool CanCommit => 
            base.CanCommit && 
            !AllocationsHaveErrors &&
            !FiltersHaveErrors;
        protected override bool CanSave => false;
        private bool AllocationsHaveErrors => AllocationsSource?.Count(A => A.HasErrors) > 0;
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
        internal override void AddNew(object ap)
        {
            CurrentEditItem = new TimeTask
            {
                Name = "New Task",
                Description = "Your text here.",
                End = DateTime.Now.Date.AddDays(1), //init before start
                Start = DateTime.Now.Date,
                TaskType = TaskTypesSource.First(N => N.Name == "Chore"),
                Dimension = 0,
                Priority = 100,
                CanFill = false,
                CanReDist = true,
                CanSplit = true,
            };
            View.AddNewItem(CurrentEditItem);
            StartEdit();
            base.AddNew(ap);
        }
        internal override void EditSelected()
        {
            base.EditSelected();
            StartEdit();
        }
        private void StartEdit()
        {
            AllocationsCollection = new CollectionViewSource();
            AllocationsCollection.Source = new ObservableCollection<TimeTaskAllocation>();
            //Doing this will allow us to simultaneouly edit all pre-existing allocations
            foreach (var A in CurrentEditItem.Allocations)
            {
                var newA = new TimeTaskAllocation
                {
                    Amount = A.Amount,
                    InstanceMinimum = A.InstanceMinimum,
                    Method = A.Method,
                    Limited = A.Limited,
                    Per = A.Per,
                    PerId = A.PerId,
                    Resource = A.Resource,
                    Resource_Id = A.Resource_Id,
                    TimeTask = A.TimeTask,
                    TimeTask_Id = A.TimeTask_Id,
                    PerOffset = A.PerOffset,
                };
                newA.TogglePer = newA.Per == null ? false : true;
                AllocationsSource.Add(newA);
            }
            OnPropertyChanged(nameof(AllocationsView));

            FiltersCollection = new CollectionViewSource();
            FiltersCollection.Source = new ObservableCollection<TimeTaskFilter>();
            //Doing this will allow us to simultaneously edit all pre-existing filters
            foreach (var F in CurrentEditItem.Filters)
            {
                FiltersSource.Add(new TimeTaskFilter
                {
                    Filterable_Id = F.Filterable_Id,
                    TimeTask_Id = F.TimeTask_Id,
                    TimeTask = F.TimeTask,
                    Filterable = F.Filterable,
                    Include = F.Include,
                });
            }
            OnPropertyChanged(nameof(FiltersView));
            foreach (var F in FiltersSource)
            {
                var t = F.FilterTypeName;
                switch (t)
                {
                    case nameof(Label):
                        F.TypeChoice = "Label";
                        break;
                    case nameof(Note):
                        F.TypeChoice = "Note";
                        break;
                    case nameof(TimePattern):
                        F.TypeChoice = "Pattern";
                        break;
                    case nameof(Resource):
                        F.TypeChoice = "Resource";
                        break;
                    case nameof(TimeTask):
                        F.TypeChoice = "Task";
                        break;
                    case nameof(TaskType):
                        F.TypeChoice = "Task Type";
                        break;
                }
                F.OnPropertyChanged(nameof(F.TypeChoice));
                F.OnPropertyChanged(nameof(F.Filterable));
            }
        }
        protected override void FinishEdit()
        {
            if (ResourcesView != null) ResourcesView.Filter = null;
            AllocationsCollection = null;
            FiltersCollection = null;
            base.FinishEdit();
        }
        internal override async Task<bool> Commit()
        {
            foreach (var A in AllocationsSource)
                if (!A.TogglePer)
                    A.Per = null;
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
            FiltersView.CommitNew();
            OnPropertyChanged(nameof(FiltersView));
        }
        private void DeleteFilter(TimeTaskFilter filter)
        {
            FiltersView.Remove(filter);
        }
        private void AddNewAllocation()
        {
            AllocationsView.AddNewItem(new TimeTaskAllocation
            {
                Amount = 1,
                InstanceMinimum = 0,
                Method = "Eager",
                Limited = false,
                TimeTask = CurrentEditItem
            });
        }
        private void DeleteAllocation(TimeTaskAllocation allocation)
        {
            AllocationsView.Remove(allocation);
            UpdateAllocationsView();
        }
        private void UpdateAllocationsView()
        {
            //filter out allocated resources
            bool timeAllocationExists = AllocationsSource.Count(A => A.Resource.IsTimeResource) > 0;
            ResourcesView.Filter = R =>
                (timeAllocationExists ? !((Resource)R).IsTimeResource : true) &&
                AllocationsSource?.Count(A => A.Resource == R) <= 0;
            OnPropertyChanged(nameof(ResourcesView));
            OnPropertyChanged(nameof(AllocationsView));
        }
        #endregion
    }
}

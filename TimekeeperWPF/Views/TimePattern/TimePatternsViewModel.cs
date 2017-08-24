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
        private bool _HasSelectedResource = false;
        private bool _IsAddingNewAllocation = false;
        private bool _IsChildCollapsed = true;
        private Resource _SelectedResource;
        private Allocation _CurrentEditAllocation;
        private TimePatternsViewModel _ChildVM;
        private long _CurrentEditAllocationMin = 0;
        private long _CurrentEditAllocationMax = 0;
        private ICommand _RemoveChildCommand;
        private ICommand _AddChildCommand;
        private ICommand _RemoveAllocationCommand;
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
        public ObservableCollection<Resource> ResourcesSource => ResourcesCollection?.Source as ObservableCollection<Resource>;
        public ObservableCollection<Allocation> CurrentEntityAllocationsSource => CurrentEntityAllocationsCollection?.Source as ObservableCollection<Allocation>;
        public ObservableCollection<TimePoint> TimePointsSource => TimePointsCollection?.Source as ObservableCollection<TimePoint>;
        public ListCollectionView ResourcesView => ResourcesCollection?.View as ListCollectionView;
        public ListCollectionView CurrentEntityAllocationsView => CurrentEntityAllocationsCollection?.View as ListCollectionView;
        public ListCollectionView TimePointsView => TimePointsCollection?.View as ListCollectionView;
        public Resource SelectedResource
        {
            get
            {
                return _SelectedResource;
            }
            set
            {
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
                if (value == _CurrentEditAllocation) return;
                _CurrentEditAllocation = value;
                OnPropertyChanged();
            }
        }
        public long CurrentEditAllocationMin
        {
            get
            {
                return _CurrentEditAllocationMin;
            }
            set
            {
                if (value == _CurrentEditAllocationMin) return;
                _CurrentEditAllocationMin = value;
                OnPropertyChanged();
            }
        }
        public long CurrentEditAllocationMax
        {
            get
            {
                return _CurrentEditAllocationMax;
            }
            set
            {
                if (value == _CurrentEditAllocationMax) return;
                _CurrentEditAllocationMax = value;
                OnPropertyChanged();
            }
        }
        public TimePatternsViewModel ChildVM
        {
            get
            {
                return _ChildVM;
            }
            private set
            {
                if (value == _ChildVM) return;
                _ChildVM = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Conditions
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
        public bool IsChildCollapsed
        {
            get
            {
                return _IsChildCollapsed;
            }
            protected set
            {
                _IsChildCollapsed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotChildCollapsed));
            }
        }
        public bool HasNotSelectedResource => !HasSelectedResource;
        public bool IsNotAddingNewAllocation => !IsAddingNewAllocation;
        public bool IsNotChildCollapsed => !IsChildCollapsed;
        #endregion
        #region Commands
        //public ICommand RemoveChildCommand => _RemoveChildCommand
        //    ?? (_RemoveChildCommand = new RelayCommand(ap => RemoveChild(), pp => CanRemoveChild));
        public ICommand RemoveAllocationCommand => _RemoveAllocationCommand
            ?? (_RemoveAllocationCommand = new RelayCommand(ap => RemoveAllocation(ap as Allocation), pp => CanDeleteAllocation(pp)));
        public ICommand AddNewAllocationCommand => _AddNewAllocationCommand
            ?? (_AddNewAllocationCommand = new RelayCommand(ap => AddNewAllocation(), pp => CanAddNewAllocation));
        public ICommand CancelAllocationCommand => _CancelAllocationCommand
            ?? (_CancelAllocationCommand = new RelayCommand(ap => CancelAllocation(), pp => CanCancelAllocation));
        public ICommand CommitAllocationCommand => _CommitAllocationCommand
            ?? (_CommitAllocationCommand = new RelayCommand(ap => CommitAllocation(), pp => CanCommitAllocation));
        #endregion
        #region Predicates
        private bool CanAddNewAllocation => HasSelectedResource && !IsResourceAllocated(SelectedResource);
        private bool CanCancelAllocation => IsAddingNewAllocation;
        private bool CanCommitAllocation => IsAddingNewAllocation && !CurrentEditAllocation.HasErrors;
        private bool CanDeleteAllocation(object pp)
        {
            return
                pp is Allocation &&
                IsNotAddingNewAllocation;
        }
        protected override bool CanSave => ChildVM.CanSave && base.CanSave;
        protected override bool CanCancel => ChildVM.CanSave && base.CanCancel;
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimePatterns.LoadAsync();
            Items.Source = Context.TimePatterns.Local;
            
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
        protected override void AddNew()
        {
            base.AddNew();
            //CurrentEditItem.Child =
            CurrentEditItem.Duration = TimeSpan.FromHours(1).Ticks;
            CurrentEditItem.ForNth = 0;
            CurrentEditItem.ForSkipDuration = 0;
            CurrentEditItem.ForTimePoint = TimePointsSource.First();
            CurrentEditItem.ForX = 1;
            BeginEdit();
        }
        protected override void EditSelected()
        {
            base.EditSelected();
            BeginEdit();
        }
        private void BeginEdit()
        {
            //CurrentEntityChild?????
            CurrentEntityAllocationsCollection = new CollectionViewSource();
            CurrentEntityAllocationsCollection.Source = new ObservableCollection<Allocation>(CurrentEditItem.Allocations);
            UpdateViews();
        }
        protected override void EndEdit()
        {
            CurrentEntityAllocationsCollection = null;
            base.EndEdit();
        }
        protected override void Commit()
        {
            CurrentEditItem.Allocations = new HashSet<Allocation>(CurrentEntityAllocationsSource);
            base.Commit();
        }
        private void AddNewAllocation()
        {
            Allocation A = new Allocation()
            {
                maxAmount = 0,
                minAmount = 0,
                Resource = ResourcesSource.First(),
                TimePattern = CurrentEditItem
            };
            //CurrentEntityAllocationsView.AddNewItem()
        }
        private void UpdateViews()
        {
            ResourcesView.Filter = R => !IsResourceAllocated(R as Resource);
            OnPropertyChanged(nameof(ResourcesView));
        }
        private bool IsResourceAllocated(Resource R)
        {
            return (CurrentEntityAllocationsSource?.Count(A => A.Resource == R) == 0);
        }
        #endregion
    }
}

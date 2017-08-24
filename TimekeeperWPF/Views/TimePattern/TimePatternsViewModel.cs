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
        private bool CanAddChild => HasSelectedChild && IsValidChild(SelectedChild);
        private bool CanAddNewAllocation => HasSelectedResource && !IsResourceAllocated(SelectedResource);
        private bool CanCancelAllocation => IsAddingNewAllocation;
        private bool CanCommitAllocation => IsAddingNewAllocation && !CurrentEditAllocation.HasErrors;
        private bool CanDeleteAllocation(object pp)
        {
            return
                pp is Allocation &&
                IsNotAddingNewAllocation;
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
        protected override void AddNew()
        {
            base.AddNew();
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
        private void AddChild()
        {
            CurrentEditItem.Child = SelectedChild;
            SelectedChild = null;
            UpdateViews();
        }
        private void AddNewAllocation()
        {
            CurrentEditAllocation = (Allocation)CurrentEntityAllocationsView.AddNew();
            CurrentEditAllocation.maxAmount = 0;
            CurrentEditAllocation.minAmount = 0;
            CurrentEditAllocation.Resource = SelectedResource;
            CurrentEditAllocation.TimePattern = CurrentEditItem;
            IsAddingNewAllocation = true;
        }
        private void DeleteAllocation(Allocation ap)
        {
            CurrentEntityAllocationsView.Remove(ap);
        }
        private void UpdateViews()
        {
            PatternsView.Filter = P => IsValidChild((TimePattern)P);
            OnPropertyChanged(nameof(PatternsView));

            ResourcesView.Filter = R => !IsResourceAllocated((Resource)R);
            CurrentEntityAllocationsView.Filter = A => ((Allocation)A).TimePattern == CurrentEditItem;
            OnPropertyChanged(nameof(ResourcesView));
            OnPropertyChanged(nameof(CurrentEntityAllocationsView));
        }
        private bool IsValidChild(TimePattern P)
        {
            return
                CurrentEditItem.Duration > P.Duration &&
                P != CurrentEditItem.Child;
        }
        private bool IsResourceAllocated(Resource R)
        {
            return (CurrentEntityAllocationsSource?.Count(A => A.Resource == R) == 0);
        }
        #endregion
    }
}

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
        private bool _HasSelectedInclude = false;
        private bool _HasSelectedExclude = false;
        private TimePattern _SelectedInclude;
        private TimePattern _SelectedExclude;
        private ICommand _RemoveIncludeCommand;
        private ICommand _RemoveExcludeCommand;
        private ICommand _AddIncludeCommand;
        private ICommand _AddExcludeCommand;
        #endregion
        public TimeTasksViewModel() : base()
        {

        }
        #region Properties
        public override string Name => nameof(Context.TimeTasks) + " Editor";
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
            get
            {
                return _SelectedInclude;
            }
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
            get
            {
                return _SelectedExclude;
            }
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
        #region Conditions
        public bool HasSelectedInclude
        {
            get
            {
                return _HasSelectedInclude;
            }
            protected set
            {
                _HasSelectedInclude = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelectedInclude));
            }
        }
        public bool HasSelectedExclude
        {
            get
            {
                return _HasSelectedExclude;
            }
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
        protected override bool CanSave => false;
        private bool CanAddInclude => HasSelectedInclude && !(CurrentEntityIncludesSource?.Contains(SelectedInclude)??false);
        private bool CanAddExclude => HasSelectedExclude && !(CurrentEntityExcludesSource?.Contains(SelectedExclude)??false);
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimeTasks.LoadAsync();
            Items.Source = Context.TimeTasks.Local;

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
        protected override void AddNew()
        {
            base.AddNew();
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
        }
        protected override void EditSelected()
        {
            base.EditSelected();
            BeginEdit();
        }
        private void BeginEdit()
        {
            if (!IsEditingItemOrAddingNew) return;
            CurrentEntityIncludesCollection = new CollectionViewSource();
            CurrentEntityIncludesCollection.Source = new ObservableCollection<TimePattern>(CurrentEditItem.IncludedPatterns);
            CurrentEntityExcludesCollection = new CollectionViewSource();
            CurrentEntityExcludesCollection.Source = new ObservableCollection<TimePattern>(CurrentEditItem.ExcludedPatterns);
            UpdateViews();
        }
        protected override void EndEdit()
        {
            CurrentEntityIncludesCollection = null;
            CurrentEntityExcludesCollection = null;
            base.EndEdit();
        }
        protected override void Commit()
        {
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
        #endregion
    }
}

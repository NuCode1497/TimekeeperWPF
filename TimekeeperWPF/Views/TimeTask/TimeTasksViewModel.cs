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
        #region Predicates
        protected override bool CanSave => false;
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimeTasks.LoadAsync();
            Items.Source = Context.TimeTasks.Local;

            await base.GetDataAsync();
        }
        protected override void AddNew()
        {
            base.AddNew();
            CurrentEditItem.AsksForCheckin = false;
            CurrentEditItem.AsksForReschedule = false;
            CurrentEditItem.CanBeEarly = false;
            CurrentEditItem.CanBeLate = false;
            CurrentEditItem.CanBePushed = false;
            CurrentEditItem.CanDeflate = false;
            CurrentEditItem.CanFill = false;
            CurrentEditItem.CanInflate = false;
            CurrentEditItem.CanReschedule = false;
            CurrentEditItem.Description = "Your text here.";
            CurrentEditItem.Dimension = 1;
            CurrentEditItem.End = DateTime.Now.RoundToHour().AddHours(1);
            CurrentEditItem.Start = DateTime.Now.RoundToHour();
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

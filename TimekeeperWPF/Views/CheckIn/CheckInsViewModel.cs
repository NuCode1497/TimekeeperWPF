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
using TimekeeperDAL.Tools;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class CheckInsViewModel : ViewModel<CheckIn>
    {
        public CheckInsViewModel() : base()
        {
            Sorter = new CheckInDateTimeSorterDesc();
        }
        public override string Name => nameof(Context.CheckIns) + " Editor";
        public CollectionViewSource TimeTasksCollection { get; protected set; }
        public ObservableCollection<TimeTask> TimeTasksSource =>
            TimeTasksCollection?.Source as ObservableCollection<TimeTask>;
        public ListCollectionView TimeTasksView =>
            TimeTasksCollection?.View as ListCollectionView;
        protected override bool CanSave => false;
        protected override bool CanEditSelected => false;
        protected override bool CanCommit => base.CanCommit && CurrentEditItem.TimeTask != null;
        protected override bool CanAddNew(object pp)
        {
            return TimeTasksView.Count > 0
                && base.CanAddNew(pp);
        }
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.CheckIns.LoadAsync();
            Items.Source = Context.CheckIns.Local;

            TimeTasksCollection = new CollectionViewSource();
            await Context.TimeTasks.LoadAsync();
            TimeTasksCollection.Source = Context.TimeTasks.Local;
            OnPropertyChanged(nameof(TimeTasksView));
        }
        internal override void AddNew(object ap)
        {
            var dt = DateTime.Now;
            if (ap is DateTime)
                dt = (DateTime)ap;
            CurrentEditItem = new CheckIn
            {
                DateTime = dt.RoundDown(new TimeSpan(0, 1, 0)),
                Text = "Start",
                TimeTask = TimeTasksSource.FirstOrDefault(),
            };
            View.AddNewItem(CurrentEditItem);
            base.AddNew(ap);
        }
        internal override void SaveAs()
        {
            throw new NotImplementedException();
        }
    }
}

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
    public class NotesViewModel : ViewModel<Note>
    {
        private bool _HasTimeTask;
        public NotesViewModel() : base()
        {
            Sorter = new NoteDateTimeSorterDesc();
        }
        public override string Name => nameof(Context.Notes) + " Editor";
        public CollectionViewSource TimeTasksCollection { get; protected set; }
        public ObservableCollection<TimeTask> TimeTasksSource => 
            TimeTasksCollection?.Source as ObservableCollection<TimeTask>;
        public ListCollectionView TimeTasksView => 
            TimeTasksCollection?.View as ListCollectionView;
        public bool HasTimeTask
        {
            get { return _HasTimeTask; }
            set
            {
                _HasTimeTask = value;
                if (CurrentEditItem != null && value == false)
                {
                    CurrentEditItem.TimeTask = null;
                }
                OnPropertyChanged();
            }
        }
        protected override bool CanSave => false;
        protected override bool CanCommit => base.CanCommit && (HasTimeTask ? CurrentEditItem.TimeTask != null : true);
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.Notes.LoadAsync();
            Items.Source = Context.Notes.Local;

            TimeTasksCollection = new CollectionViewSource();
            await Context.TimeTasks.LoadAsync();
            TimeTasksCollection.Source = Context.TimeTasks.Local;
            OnPropertyChanged(nameof(TimeTasksView));
        }
        protected override void AddNew()
        {
            CurrentEditItem = new Note
            {
                DateTime = DateTime.Now,
                Text = "Your text here."
            };
            View.AddNewItem(CurrentEditItem);
            base.AddNew();
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
    }
}

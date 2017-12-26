﻿using Microsoft.Win32;
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
        protected override void AddNew()
        {
            CurrentEditItem = new CheckIn
            {
                DateTime = DateTime.Now,
                Start = true
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

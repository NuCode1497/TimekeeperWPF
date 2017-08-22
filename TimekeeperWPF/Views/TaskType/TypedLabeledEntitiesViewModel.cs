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
    public abstract class TypedLabeledEntitiesViewModel<ModelType> : LabeledEntitiesViewModel<ModelType>
        where ModelType : TypedLabeledEntity, new()
    {
        public TypedLabeledEntitiesViewModel() : base()
        { }
        #region Properties
        public CollectionViewSource TaskTypesCollection { get; set; }
        public ObservableCollection<TaskType> TaskTypesSource => TaskTypesCollection?.Source as ObservableCollection<TaskType>;
        public ListCollectionView TaskTypesView => TaskTypesCollection?.View as ListCollectionView;
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            TaskTypesCollection = new CollectionViewSource();
            await Context.TaskTypes.LoadAsync();
            TaskTypesCollection.Source = Context.TaskTypes.Local;
            TaskTypesView.CustomSort = BasicSorter;
            OnPropertyChanged(nameof(TaskTypesView));

            await base.GetDataAsync();
        }
        protected override void AddNew()
        {
            base.AddNew();
            CurrentEditItem.TaskType =
                (from t in TaskTypesSource
                 where t.Name == "Note"
                 select t).DefaultIfEmpty(TaskTypesSource[0]).First();
            BeginEdit();
        }
        protected override void EditSelected()
        {
            base.EditSelected();
            BeginEdit();
        }
        private void BeginEdit()
        {
            TaskTypesView.MoveCurrentTo(
                (from t in TaskTypesSource
                 where t.Name == CurrentEditItem?.TaskType.Name
                 select t).DefaultIfEmpty(TaskTypesSource[0]).First());
        }
        #endregion
    }
}

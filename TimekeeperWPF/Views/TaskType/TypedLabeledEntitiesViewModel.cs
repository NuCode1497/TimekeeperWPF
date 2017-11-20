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
            TaskTypesView.CustomSort = NameSorter;
            OnPropertyChanged(nameof(TaskTypesView));

            await base.GetDataAsync();
        }
        #endregion
    }
}

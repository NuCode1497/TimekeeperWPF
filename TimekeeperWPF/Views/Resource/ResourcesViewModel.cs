﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class ResourcesViewModel : ViewModel<Resource>
    {
        public ResourcesViewModel()
        {
            Sorter = new BasicEntitySorter();
            LoadData();
        }
        #region Properties
        public override string Name => nameof(Context.Resources) + " Editor";
        #endregion
        #region Predicates
        protected override bool CanSave => false;
        #endregion
        #region Actions
        protected override async Task<ObservableCollection<Resource>> GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.Resources.LoadAsync();
            return Context.Resources.Local;
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
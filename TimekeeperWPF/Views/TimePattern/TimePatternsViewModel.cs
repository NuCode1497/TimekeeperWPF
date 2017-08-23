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
        #endregion
        public TimePatternsViewModel() : base()
        {

        }
        #region Properties
        public override string Name => nameof(Context.TimePatterns) + " Editor";
        #endregion
        #region Conditions
        #endregion
        #region Commands
        #endregion
        #region Predicates
        #endregion
        #region Actions
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            await Context.TimePatterns.LoadAsync();
            Items.Source = Context.TimePatterns.Local;
        }
        protected override void SaveAs()
        {
            throw new NotImplementedException();
        }
        protected override void AddNew()
        {
            base.AddNew();
            //CurrentEditItem.Allocations
            //CurrentEditItem.Child
            CurrentEditItem.Duration = TimeSpan.FromHours(1).Ticks;
            //CurrentEditItem.ForNth
            //CurrentEditItem.ForSkipDuration;
            //CurrentEditItem.ForTimePoint;
            //CurrentEditItem.ForX;
            BeginEdit();
        }
        private void BeginEdit()
        {
            //create allocationscollection
            UpdateViews();
        }
        protected override void EndEdit()
        {
            //null allocationscollection
            base.EndEdit();
        }
        protected override void Commit()
        {
            //temp collection to actual collection
            base.Commit();
        }
        private void UpdateViews()
        {
            //filter resource list by exclude resources allocated by current pattern
            //propertychanged
        }
        #endregion
    }
}

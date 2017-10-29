// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using PropertyChanged;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TimekeeperDAL.Tools
{
    //This annotation from PropertyChanged.Fody weaver injects INotifyPropertyChanged into each model.
    //Implementing INotifyProperyChanged creates an Observable Model that lets the UI update when data changes.
    [AddINotifyPropertyChangedInterface]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Implemented with INotifyPropertyChanged. Flagged true when a property is changed. You must manually flag false.
        /// </summary>
        [NotMapped]
        public bool IsChanged { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

// Copyright 2017 (C) Cody Neuburger  All rights reserved.
//Notes: A filter is an associative entity that creates a junction table for a
//many-to-many mapping. That means the Filter class must have composite foreign keys
//using [Column] [Key] [ForeignKey] attributes on a Filterable and a <parent entity>.
//Filterable is an object that inherits from abstract class Filterable.
//If a Filterable class has a set of Filters (like TimeTask does), that class must be
//separated from the Filterables table into its own table using the [Table] attribute.

using TimekeeperDAL.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimeTaskFilter : EntityBase
    {
        public override string ToString()
        {
            string s = "Include";
            if (!Include) s = "Exclude";
            return String.Format("{0} {1}", s, Filterable.Name);
        }

        [NotMapped]
        public string FilterTypeName => Filterable?.GetType().Name;

        [NotMapped]
        public override string this[string columnName]
        {
            get
            {
                string[] errors = null;
                bool hasError = false;
                switch (columnName)
                {
                    case nameof(Filterable):
                        errors = GetErrorsFromAnnotations(nameof(Filterable), Filterable);
                        break;
                    case nameof(Include):
                        errors = GetErrorsFromAnnotations(nameof(Include), Include);
                        break;
                }
                if (errors != null && errors.Length != 0)
                {
                    AddErrors(columnName, errors);
                    hasError = true;
                }
                if (!hasError) ClearErrors(columnName);
                return string.Empty;
            }
        }

        private string _TypeChoice = "";
        [NotMapped]
        public string TypeChoice
        {
            get { return _TypeChoice; }
            set
            {
                _TypeChoice = value;
                OnPropertyChanged();
            }
        }

        [NotMapped]
        public static readonly List<string> FilterableTypesChoices = new List<string>()
        {
            "Label",
            "Note",
            "Pattern",
            "Resource",
            "Task",
            "Task Type"
        };
    }
}

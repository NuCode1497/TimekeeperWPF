﻿// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using TimekeeperDAL.Tools;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System;

namespace TimekeeperDAL.EF
{
    public abstract partial class Filterable : EntityBase, INamedObject
    {
        public override string ToString()
        {
            return Name;
        }

        [NotMapped]
        public override string this[string columnName]
        {
            get
            {
                string[] errors = null;
                bool hasError = false;
                switch (columnName)
                {
                    case nameof(Name):
                        errors = GetErrorsFromAnnotations(nameof(Name), Name);
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

        [NotMapped]
        public static readonly List<string> FilterableTypesChoices = new List<string>()
        {
            "Label",
            "Pattern",
            "Resource",
            "Task",
            "Task Type"
        };

        public abstract bool HasDateTime(DateTime dt);
    }
}

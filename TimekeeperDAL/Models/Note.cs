// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using TimekeeperDAL.Tools;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace TimekeeperDAL.EF
{
    public partial class Note : TypedLabeledEntity
    {
        public override string ToString()
        {
            return TaskType.Name + " - " + DateTime.ToString() + " - " + Text;
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
                    case nameof(Dimension):
                        errors = GetErrorsFromAnnotations(nameof(Dimension), Dimension);
                        break;
                    case nameof(DateTime):
                        errors = GetErrorsFromAnnotations(nameof(DateTime), DateTime);
                        break;
                    case nameof(Text):
                        errors = GetErrorsFromAnnotations(nameof(Text), Text);
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

        public override bool HasDateTime(DateTime dt)
        {
            return false;
        }
    }
}

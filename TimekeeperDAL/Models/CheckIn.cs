﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public partial class CheckIn : EntityBase
    {
        public override string ToString()
        {
            return "📌" + Text + ": " + TimeTask + ": " + DateTime.ToString();
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
                    case nameof(TimeTask):
                        errors = GetErrorsFromAnnotations(nameof(TimeTask), TimeTask);
                        break;
                    case nameof(DateTime):
                        errors = GetErrorsFromAnnotations(nameof(DateTime), DateTime);
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
        public static readonly List<string> CheckInChoices = new List<string>()
        {
            "Start",
            "End",
            "Cancel",
        };
    }
}

// Copyright 2017 (C) Cody Neuburger  All rights reserved.
//Notes: An Allocation is an associative entity that creates a junction table for a
//many-to-many mapping. That means the Allocation class must have composite foreign keys
//using [Column] [Key] [ForeignKey] attributes on a Resource and a <parent entity>.
//Set [Required] attribute on Resource for validation.

using TimekeeperDAL.Tools;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace TimekeeperDAL.EF
{
    public partial class TimeTaskAllocation : EntityBase
    {
        public override string ToString()
        {
            string s = "";
            if (Amount == 1)
                s = Amount + " " + Resource;
            else
                s = Amount + " " + Resource.ToString().Pluralize();
            if (Per != null)
                s += " Per " + Per.ToString();
            return s;
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
                    case nameof(Per):
                        hasError = ValidateTimeSelection(hasError);
                        errors = GetErrorsFromAnnotations(nameof(Per), Per);
                        break;
                    case nameof(Resource):
                        errors = GetErrorsFromAnnotations(nameof(Resource), Resource);
                        break;
                    case nameof(Amount):
                        hasError = ValidateTimeSelection(hasError);
                        errors = GetErrorsFromAnnotations(nameof(Amount), Amount);
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

        private bool ValidateTimeSelection(bool hasError)
        {
            if (Per != null
                && Per.IsTimeResource
                && Resource.IsTimeResource
                && AmountAsTimeSpan() > Per.AsTimeSpan())
            {
                AddError(nameof(Per), "Per must be larger than Resource");
                AddError(nameof(Amount), "Per must be larger than Resource");
                hasError = true;
            }
            else if (Per != null
                && Resource.IsTimeResource
                && !Per.IsTimeResource)
            {
                AddError(nameof(Per), "If Resource is a time, then Per must be a time");
                hasError = true;
            }
            if (!hasError) ClearErrors(nameof(Per));
            if (!hasError) ClearErrors(nameof(Amount));
            return hasError;
        }
        
        /// <summary>
        /// A rough estimate of the duration of the time allocation. e.g. Month = Amount * 30.437
        /// </summary>
        public TimeSpan AmountAsTimeSpan()
        {
            TimeSpan allocatedTime = new TimeSpan();
            switch (Resource.Name)
            {
                case "Minute":
                    allocatedTime = new TimeSpan(0, (int)Amount, 0);
                    break;
                case "Hour":
                    allocatedTime = new TimeSpan((int)Amount, 0, 0);
                    break;
                case "Day":
                    allocatedTime = new TimeSpan((int)Amount, 0, 0, 0);
                    break;
                case "Week":
                    allocatedTime = new TimeSpan((int)Amount * 7, 0, 0, 0);
                    break;
                case "Month":
                    allocatedTime = new TimeSpan((int)(Amount * 30.437), 0, 0, 0);
                    break;
                case "Year":
                    allocatedTime = new TimeSpan((int)(Amount * 365.2425), 0, 0, 0);
                    break;
            }
            return allocatedTime;
        }
    }
}

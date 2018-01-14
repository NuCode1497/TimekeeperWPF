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
        public bool TogglePer { get; set; }

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
                        if (Amount <= 0)
                        {
                            AddError(nameof(Amount), "Amount must be positive");
                            hasError = true;
                        }
                        hasError = ValidateTimeSelection(hasError);
                        errors = GetErrorsFromAnnotations(nameof(Amount), Amount);
                        break;
                    case nameof(InstanceMinimum):
                        if (InstanceMinimum < 0)
                        {
                            AddError(nameof(InstanceMinimum), "InstanceMinimum must be positive or 0");
                            hasError = true;
                        }
                        errors = GetErrorsFromAnnotations(nameof(InstanceMinimum), InstanceMinimum);
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
                && AmountAsTimeSpan > Per.AsTimeSpan())
            {
                AddError(nameof(Per), "Per must be larger than Resource");
                AddError(nameof(Amount), "Per must be larger than Resource");
                hasError = true;
            }
            else 
            if (Per != null
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
        public TimeSpan AmountAsTimeSpan
        {
            get
            {
                TimeSpan allocatedTime = new TimeSpan();
                switch (Resource?.Name)
                {
                    case "Minute":
                        allocatedTime = TimeSpan.FromMinutes(Amount);
                        break;
                    case "Hour":
                        allocatedTime = TimeSpan.FromHours(Amount);
                        break;
                    case "Day":
                        allocatedTime = TimeSpan.FromDays(Amount);
                        break;
                    case "Week":
                        allocatedTime = TimeSpan.FromDays(Amount * 7.0d);
                        break;
                    case "Month":
                        allocatedTime = TimeSpan.FromDays(Amount * 30.437d);
                        break;
                    case "Year":
                        allocatedTime = TimeSpan.FromDays(Amount * 365.2425);
                        break;
                }
                return allocatedTime;
            }
        }

        public TimeSpan InstanceMinimumAsTimeSpan
        {
            get
            {
                TimeSpan minTime = new TimeSpan();
                switch (Resource?.Name)
                {
                    case "Minute":
                        minTime = TimeSpan.FromMinutes(InstanceMinimum);
                        break;
                    case "Hour":
                        minTime = TimeSpan.FromHours(InstanceMinimum);
                        break;
                    case "Day":
                        minTime = TimeSpan.FromDays(InstanceMinimum);
                        break;
                    case "Week":
                        minTime = TimeSpan.FromDays(InstanceMinimum * 7.0d);
                        break;
                    case "Month":
                        minTime = TimeSpan.FromDays(InstanceMinimum * 30.437d);
                        break;
                    case "Year":
                        minTime = TimeSpan.FromDays(InstanceMinimum * 365.2425);
                        break;
                }
                return minTime;
            }
        }
    }
}

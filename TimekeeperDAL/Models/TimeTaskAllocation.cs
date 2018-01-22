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
                        else if (InstanceMinimum != 0 && InstanceMinimumAsTimeSpan < TimeTask.MinimumDuration)
                        {
                            AddError(nameof(InstanceMinimum), $"InstanceMinimum must be >= {TimeTask.MinimumDuration.ToString()}");
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
        public TimeSpan AmountAsTimeSpan => ResourceDoubleToTimeSpan(Amount);
        public TimeSpan PerOffsetAsTimeSpan => ResourceDoubleToTimeSpan(PerOffset);
        public TimeSpan InstanceMinimumAsTimeSpan => ResourceDoubleToTimeSpan(InstanceMinimum);

        private TimeSpan ResourceDoubleToTimeSpan(double amount)
        {
            TimeSpan allocatedTime = new TimeSpan();
            switch (Resource?.Name)
            {
                case "Minute":
                    allocatedTime = TimeSpan.FromMinutes(amount);
                    break;
                case "Hour":
                    allocatedTime = TimeSpan.FromHours(amount);
                    break;
                case "Day":
                    allocatedTime = TimeSpan.FromDays(amount);
                    break;
                case "Week":
                    allocatedTime = TimeSpan.FromDays(amount * 7.0d);
                    break;
                case "Month":
                    allocatedTime = TimeSpan.FromDays(amount * 30.437d);
                    break;
                case "Year":
                    allocatedTime = TimeSpan.FromDays(amount * 365.2425);
                    break;
            }

            return allocatedTime;
        }
    }
}

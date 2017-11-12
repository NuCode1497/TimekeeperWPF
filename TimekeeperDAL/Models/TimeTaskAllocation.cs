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
            if (Amount == 1) return Amount + " " + Resource;
            return Amount + " " + Resource.ToString().Pluralize();
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
                        if (ResourceAsTimeSpan() > PerAsTimeSpan())
                        {
                            AddError(nameof(Per), "Per must be larger than Resource");
                            hasError = true;
                        }
                        errors = GetErrorsFromAnnotations(nameof(Per), Per);
                        break;
                    case nameof(Resource):
                        if (ResourceAsTimeSpan() > PerAsTimeSpan())
                        {
                            AddError(nameof(Resource), "Resource must be smaller than Per");
                            hasError = true;
                        }
                        errors = GetErrorsFromAnnotations(nameof(Resource), Resource);
                        break;
                    case nameof(Amount):
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
        
        [NotMapped]
        public double Remaining { get; set; }

        [NotMapped]
        public TimeSpan RemainingAsTimeSpan => new TimeSpan((long)Remaining);

        public TimeSpan ResourceAsTimeSpan()
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

        public TimeSpan PerAsTimeSpan()
        {
            TimeSpan allocatedTime = new TimeSpan();
            switch (Resource.Name)
            {
                case "Minute":
                    allocatedTime = new TimeSpan(0, 1, 0);
                    break;
                case "Hour":
                    allocatedTime = new TimeSpan(1, 0, 0);
                    break;
                case "Day":
                    allocatedTime = new TimeSpan(1, 0, 0, 0);
                    break;
                case "Week":
                    allocatedTime = new TimeSpan(7, 0, 0, 0);
                    break;
                case "Month":
                    allocatedTime = new TimeSpan(28, 0, 0, 0);
                    break;
                case "Year":
                    allocatedTime = new TimeSpan(365, 0, 0, 0);
                    break;
            }
            return allocatedTime;
        }
    }
}

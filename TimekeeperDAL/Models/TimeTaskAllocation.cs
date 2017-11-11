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
                    case nameof(Resource):
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

        /// <summary>
        /// Needs to be set manually
        /// </summary>
        [NotMapped]
        public double Remaining { get; set; }

        [NotMapped]
        public TimeSpan RemainingAsTimeSpan => new TimeSpan((long)Remaining);

        public TimeSpan AsTimeSpan()
        {
            TimeSpan allocatedTime = new TimeSpan();
            switch (Resource.Name)
            {
                case "Second":
                    allocatedTime = new TimeSpan(0, 0, (int)Amount);
                    break;
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

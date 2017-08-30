using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimePattern : LabeledEntity, INamedObject
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
                    case nameof(Id):
                        break;
                    case nameof(Name):
                        errors = GetErrorsFromAnnotations(nameof(Name), Name);
                        break;
                    case nameof(Duration):
                        if (Child?.Duration > Duration)
                        {
                            AddError(nameof(Duration), "Duration must be greater than Child duration");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors(nameof(Duration));
                        errors = GetErrorsFromAnnotations(nameof(Duration), Duration);
                        break;
                    case nameof(Child):
                        if(Child?.Duration > Duration)
                        {
                            AddError(nameof(Child), "Child duration must be less than Parent duration");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors(nameof(Child));
                        break;
                    case nameof(ForNth):
                        errors = GetErrorsFromAnnotations(nameof(ForNth), ForNth);
                        break;
                    case nameof(ForSkipDuration):
                        errors = GetErrorsFromAnnotations(nameof(ForSkipDuration), ForSkipDuration);
                        break;
                    case nameof(ForTimePoint):
                        errors = GetErrorsFromAnnotations(nameof(ForTimePoint), ForTimePoint);
                        break;
                    case nameof(ForX):
                        errors = GetErrorsFromAnnotations(nameof(ForX), ForX);
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
        public string AllocationsToString
        {
            get
            {
                string s = "";
                foreach (Allocation A in Allocations)
                {
                    s += A.minAmount + " - " + A.maxAmount + " of " + A.Resource.Name.ToString() + ", ";
                }
                if (s.Length >= 2)
                    s = s.Substring(0, s.Length - 2);
                return s;
            }
        }

        [NotMapped]
        public TimeSpan DurationTS
        {
            get { return new TimeSpan(Duration); }
            set { Duration = value.Ticks; }
        }

        [NotMapped]
        public TimeSpan ForSkipDurationTS
        {
            get { return new TimeSpan(ForSkipDuration); }
            set { ForSkipDuration = value.Ticks; }
        }
    }
}

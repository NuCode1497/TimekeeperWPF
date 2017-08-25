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
                        if (Child.Duration > Duration)
                        {
                            AddError(nameof(Duration), "Duration must be greater than Child duration");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors(nameof(Duration));
                        break;
                    case nameof(Child):
                        if(Child.Duration > Duration)
                        {
                            AddError(nameof(Child), "Child duration must be less than Parent duration");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors(nameof(Child));
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
    }
}

using System;
using System.ComponentModel.DataAnnotations.Schema;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public partial class TimeTaskFilter : EntityBase
    {
        public override string ToString()
        {
            string s = "Include";
            if (!Include) s = "Exclude";
            return String.Format("{0} {1}", s, Filterable.Name);
        }

        [NotMapped]
        public string FilterTypeName => Filterable.GetType().Name;

        [NotMapped]
        public override string this[string columnName]
        {
            get
            {
                string[] errors = null;
                bool hasError = false;
                switch (columnName)
                {
                    case nameof(Include):
                        errors = GetErrorsFromAnnotations(nameof(Include), Include);
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

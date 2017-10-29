using System.ComponentModel.DataAnnotations.Schema;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public partial class Allocation : EntityBase
    {
        public override string ToString()
        {
            return Amount + " of " + Resource.ToString();
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
                    case nameof(Amount):
                        if (!hasError) ClearErrors(nameof(Amount));
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
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public partial class Allocation : EntityBase
    {
        private static PluralizationService pserve = PluralizationService.CreateService(CultureInfo.CurrentCulture);
        public override string ToString()
        {
            if (Amount == 1) return Amount + " " + Resource;
            return Amount + " " + pserve.Pluralize(Resource.ToString());
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

namespace TimekeeperDAL.EF
{
    public partial class Allocation : EntityBase
    {
        public override string ToString()
        {
            return minAmount + " - " + maxAmount + " of " + Resource.ToString();
        }

        public override string this[string columnName]
        {
            get
            {
                string[] errors = null;
                bool hasError = false;
                switch (columnName)
                {
                    case nameof(minAmount):
                        if(minAmount > maxAmount)
                        {
                            AddError(nameof(minAmount), "minAmount must be less than maxAmount");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors(nameof(minAmount));
                        errors = GetErrorsFromAnnotations(nameof(minAmount), minAmount);
                        break;
                    case nameof(maxAmount):
                        if(maxAmount < minAmount)
                        {
                            AddError(nameof(maxAmount), "maxAmount must be greater than minAmount");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors(nameof(maxAmount));
                        errors = GetErrorsFromAnnotations(nameof(maxAmount), maxAmount);
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

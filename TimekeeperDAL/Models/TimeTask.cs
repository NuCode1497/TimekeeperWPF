using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimeTask : TypedLabeledEntity, INamedObject
    {
        public override string ToString()
        {
            return Name + " - " + Description;
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
                    case nameof(Name):
                        errors = GetErrorsFromAnnotations(nameof(Name), Name);
                        break;
                    case nameof(Start):
                        if(Start > End)
                        {
                            AddError(nameof(Start), "Start must come before End");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors();
                        errors = GetErrorsFromAnnotations(nameof(Start), Start);
                        break;
                    case nameof(End):
                        if(Start > End)
                        {
                            AddError(nameof(End), "Start must come before End");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors();
                        errors = GetErrorsFromAnnotations(nameof(End), End);
                        break;
                    case nameof(Description):
                        errors = GetErrorsFromAnnotations(nameof(Description), Description);
                        break;
                    case nameof(Priority):
                        if (Priority < 1)
                        {
                            AddError(nameof(Priority), "Priority must be greater than 0");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors();
                        errors = GetErrorsFromAnnotations(nameof(Priority), Priority);
                        break;
                    case nameof(Dimension):
                        if (Dimension < 1)
                        {
                            AddError(nameof(Dimension), "Dimension must be greater than 0");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors();
                        errors = GetErrorsFromAnnotations(nameof(Dimension), Dimension);
                        break;
                    case nameof(PowerLevel):
                        if (PowerLevel < 1)
                        {
                            AddError(nameof(PowerLevel), "PowerLevel must be greater than 0");
                            hasError = true;
                        }
                        if (!hasError) ClearErrors();
                        errors = GetErrorsFromAnnotations(nameof(PowerLevel), PowerLevel);
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
        public TimeSpan Duration => End - Start;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;

namespace TimekeeperDAL.EF
{
    public partial class TaskType : EntityBase
    {
        public TaskType()
        {
            Type = "Change Me";
        }
        public override string this[string columnName]
        {
            get
            {
                string[] errors = null;
                bool hasError = false;
                switch (columnName)
                {
                    case nameof(Type):
                        errors = GetErrorsFromAnnotations(nameof(Type), Type);
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
        public override string ToString()
        {
            return Type;
        }
    }
}

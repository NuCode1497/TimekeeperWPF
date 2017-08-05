using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperDAL.EF
{
    public partial class Note : EntityBase
    {
        public Note()
        {
            DateTime = DateTime.Now;
            Text = "Your text here.";
            Type = "Note";
            IsChanged = false;
        }
        public override string this[string columnName]
        {
            get
            {
                string[] errors = null;
                bool hasError = false;
                switch (columnName)
                {
                    case nameof(Id):
                        //Add Validation code here
                        break;
                    case nameof(DateTime):
                        //hasError = CheckHolidays();
                        //if (!hasError) ClearErrors(nameof(DateTime));
                        errors = GetErrorsFromAnnotations(nameof(DateTime), DateTime);
                        break;
                    case nameof(Text):
                        errors = GetErrorsFromAnnotations(nameof(Text), Text);
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
            return DateTime.ToString() + " " + Text;
        }
    }
}

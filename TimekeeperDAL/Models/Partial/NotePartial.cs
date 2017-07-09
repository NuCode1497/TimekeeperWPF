using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperDAL.Models
{
    public partial class Note : EntityBase
    {
        //Validation code
        public override string this[string columnName]
        {
            get
            {
                string[] errors = null;
                bool hasError = false;
                switch (columnName)
                {
                    case nameof(NoteDateTime):
                        // Add validation logic here
                        //hasError = CheckMakeAndColor();
                        //if (Make == "ModelT")
                        //{
                        //    AddError(nameof(Make), "Too Old");
                        //    hasError = true;
                        //}
                        //if (!hasError) ClearErrors(nameof(Make));

                        errors = GetErrorsFromAnnotations(nameof(NoteDateTime), NoteDateTime);
                        break;
                    case nameof(NoteText):
                        errors = GetErrorsFromAnnotations(nameof(NoteText), NoteText);
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

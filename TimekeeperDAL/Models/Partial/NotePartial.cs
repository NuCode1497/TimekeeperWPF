using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperDAL.Models
{
    public partial class Note : EntityBase
    {
        public Note()
        {
            NoteDateTime = DateTime.Now;
            NoteText = "Your text here.";
        }
        public override string this[string columnName]
        {
            get
            {
                string[] errors = null;
                bool hasError = false;
                switch (columnName)
                {
                    case nameof(NoteID):
                        //Add Validation code here
                        break;
                    case nameof(NoteDateTime):
                        //hasError = CheckHolidays();
                        //if (!hasError) ClearErrors(nameof(NoteDateTime));
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

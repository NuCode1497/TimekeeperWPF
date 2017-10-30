using System.ComponentModel.DataAnnotations.Schema;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public partial class TimePattern : LabeledEntity, INamedObject
    {
        public override string ToString()
        {
            return Name;
        }

        [NotMapped]
        public string QueryToString
        {
            get
            {
                string s = "where ";
                foreach (TimePatternClause c in Query)
                {
                    s += c.ToString() + ", ";
                }
                if (s.Length >= 2)
                    s = s.Substring(0, s.Length - 2);
                return s;
            }
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

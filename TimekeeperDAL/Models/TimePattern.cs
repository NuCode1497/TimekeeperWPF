﻿using System.ComponentModel.DataAnnotations.Schema;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public partial class TimePattern : LabeledEntity
    {
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
        public string LabelsToString
        {
            get
            {
                string s = "";
                foreach (Label l in Labels)
                {
                    s += l.ToString() + ", ";
                }
                if (s.Length >= 2)
                    s = s.Substring(0, s.Length - 2);
                return s;
            }
        }
    }
}

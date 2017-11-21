// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using TimekeeperDAL.Tools;
using System.ComponentModel.DataAnnotations.Schema;
using System;

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

        public override bool HasDateTime(DateTime dt)
        {
            if (Any)
            {
                //must pass at least one clause to return true
                foreach (TimePatternClause clause in Query)
                {
                    bool pass = false;
                    pass = HasDTPart2(dt, clause, pass);
                    if (pass) return true;
                }
                return false;
            }
            else
            {
                //must pass all clauses to return true
                foreach (TimePatternClause clause in Query)
                {
                    bool pass = false;
                    pass = HasDTPart2(dt, clause, pass);
                    if (!pass) return false;
                }
                return true;
            }
        }

        private static bool HasDTPart2(DateTime dt, TimePatternClause clause, bool pass)
        {
            switch (clause.TimeProperty)
            {
                case "WeekDay":
                    pass = clause.CheckEquivalency(dt.DayOfWeek.ToString());
                    break;
                case "Month":
                    pass = clause.CheckEquivalency(dt.ToString("MMMM"));
                    break;
                case "MonthDay":
                    pass = clause.CheckEquivalency(dt.Day);
                    break;
                case "MonthWeek":
                    pass = clause.CheckEquivalency(dt.WeekOfMonth());
                    break;
                case "Time":
                    pass = clause.CheckEquivalency(dt.TimeOfDay);
                    break;
                case "Year":
                    pass = clause.CheckEquivalency(dt.Year);
                    break;
                case "YearDay":
                    pass = clause.CheckEquivalency(dt.DayOfYear);
                    break;
                case "YearWeek":
                    pass = clause.CheckEquivalency(dt.WeekOfYear());
                    break;
            }

            return pass;
        }
    }
}

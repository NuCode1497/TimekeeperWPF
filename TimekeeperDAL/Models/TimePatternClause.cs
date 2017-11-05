// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using TimekeeperDAL.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimePatternClause : EntityBase
    {
        public TimePatternClause()
        {
            TimeProperty = TimePropertyChoices[0];
            Equivalency = BinaryEquivalencyChoices[0];
            TimePropertyValue = WeekDayChoices[0];
        }

        public override string ToString()
        {
            return "[" + TimeProperty + " " + Equivalency + " " + TimePropertyValue + "]";
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
                    case nameof(TimeProperty):
                        hasError = ValidateTimeProperty(hasError);
                        errors = GetErrorsFromAnnotations(nameof(TimeProperty), TimeProperty);
                        break;
                    case nameof(Equivalency):
                        hasError = ValidateEquivalency(hasError);
                        errors = GetErrorsFromAnnotations(nameof(Equivalency), Equivalency);
                        break;
                    case nameof(TimePropertyValue):
                        hasError = ValidateTimePropertyValue(hasError);
                        errors = GetErrorsFromAnnotations(nameof(TimePropertyValue), TimePropertyValue);
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

        private bool ValidateTimePropertyValue(bool hasError)
        {
            switch (TimeProperty)
            {
                case "WeekDay":
                    if (!WeekDayChoices.Contains(TimePropertyValue))
                    {
                        AddError(nameof(TimePropertyValue), "Not a WeekDay");
                        hasError = true;
                    }
                    break;
                case "Month":
                    if (!MonthChoices.Contains(TimePropertyValue))
                    {
                        AddError(nameof(TimePropertyValue), "Not a Month");
                        hasError = true;
                    }
                    break;
                case "MonthDay":
                    hasError = ValTPVInt(hasError, 1, 31);
                    break;
                case "MonthWeek":
                    hasError = ValTPVInt(hasError, 1, 6);
                    break;
                case "Time":
                    if (!DateTime.TryParse(TimePropertyValue, out DateTime dtz))
                    {
                        AddError(nameof(TimePropertyValue), "Bad time format");
                        hasError = true;
                    }
                    break;
                case "Year":
                    if (!int.TryParse(TimePropertyValue, out int iz))
                    {
                        AddError(nameof(TimePropertyValue), "Must be an integer");
                        hasError = true;
                    }
                    break;
                case "YearDay":
                    hasError = ValTPVInt(hasError, 1, 6);
                    break;
                case "YearWeek":
                    hasError = ValTPVInt(hasError, 1, 53);
                    break;
            }
            return hasError;
        }

        private bool ValTPVInt(bool hasError, int r1, int r2)
        {
            int value;
            if (!int.TryParse(TimePropertyValue, out value)
                || !(value >= r1 && value <= r2))
            {
                AddError(nameof(TimePropertyValue), String.Format("Must be an integer between {0} and {1}", r1, r2));
                hasError = true;
            }
            return hasError;
        }

        private bool ValidateEquivalency(bool hasError)
        {
            if (TimeProperty == "WeekDay"
                || TimeProperty == "Month")
            {
                if (!BinaryEquivalencyChoices.Contains(Equivalency))
                {
                    AddError(nameof(Equivalency), "Must be a binary Equivalency");
                    hasError = true;
                }
            }
            else
            {
                if (!AllEquivalencyChoices.Contains(Equivalency))
                {
                    AddError(nameof(Equivalency), "Not a valid Equivalency");
                    hasError = true;
                }
            }

            return hasError;
        }

        private bool ValidateTimeProperty(bool hasError)
        {
            if (!TimePropertyChoices.Contains(TimeProperty))
            {
                AddError(nameof(TimeProperty), String.Format("\"{0}\" is not a TimeProperty", TimeProperty));
                hasError = true;
            }

            return hasError;
        }

        [NotMapped]
        public static readonly List<string> TimePropertyChoices = new List<string>()
        {
            "WeekDay",
            "Month",
            "MonthDay",
            "MonthWeek",
            "Time",
            "Year",
            "YearDay",
            "YearWeek"
        };
        [NotMapped]
        public static readonly List<string> AllEquivalencyChoices = new List<string>()
        {
            "==",
            "!=",
            "<",
            ">",
            "<=",
            ">=",
        };
        [NotMapped]
        public static readonly List<string> BinaryEquivalencyChoices = new List<string>()
        {
            "==",
            "!=",
        };
        [NotMapped]
        public static readonly List<string> WeekDayChoices = new List<string>()
        {
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday"
        };
        [NotMapped]
        public static readonly List<string> MonthChoices = new List<string>()
        {
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"
        };
    }
}

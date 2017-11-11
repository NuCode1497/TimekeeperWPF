// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using TimekeeperDAL.Tools;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace TimekeeperDAL.EF
{
    public partial class TimeTask : TypedLabeledEntity
    {
        [NotMapped]
        public string AllocationsToString
        {
            get
            {
                string s = "";
                foreach (TimeTaskAllocation a in Allocations)
                {
                    s += a + ", ";
                }
                if (s.Length >= 2)
                    s = s.Substring(0, s.Length - 2);
                return s;
            }
        }

        [NotMapped]
        public string FiltersToString
        {
            get
            {
                string s = "";
                foreach (TimeTaskFilter f in Filters)
                {
                    s += f + ", ";
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
                    case nameof(Start):
                        if (Start > End)
                        {
                            AddError(nameof(Start), "Start must come before End");
                            hasError = true;
                        }
                        errors = GetErrorsFromAnnotations(nameof(Start), Start);
                        break;
                    case nameof(End):
                        if (Start > End)
                        {
                            AddError(nameof(End), "Start must come before End");
                            hasError = true;
                        }
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
                        errors = GetErrorsFromAnnotations(nameof(Priority), Priority);
                        break;
                    case nameof(Dimension):
                        if (Dimension < 1)
                        {
                            AddError(nameof(Dimension), "Dimension must be greater than 0");
                            hasError = true;
                        }
                        errors = GetErrorsFromAnnotations(nameof(Dimension), Dimension);
                        break;
                    case nameof(PowerLevel):
                        if (PowerLevel < 1)
                        {
                            AddError(nameof(PowerLevel), "PowerLevel must be greater than 0");
                            hasError = true;
                        }
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

        [NotMapped]
        public Dictionary<DateTime, TimeSpan> InclusionZones { get; set; }

        public override bool HasDateTime(DateTime dt)
        {
            if ((InclusionZones == null) || (InclusionZones.Count == 0)) return false;
            foreach (var z in InclusionZones)
            {
                if (dt < z.Key) return false;
                if (dt < z.Key + z.Value) return true;
            }
            return false;
        }

        public void BuildInclusionZones()
        {
            InclusionZones = new Dictionary<DateTime, TimeSpan>();
            DateTime dt = Start;
            bool include = false;
            DateTime zoneStart = dt;
            while (dt < End)
            {
                //we want to determine if there exists at least one relevant filter that includes this time
                bool prevInclude = include;
                bool hasRelevantFilter = false;
                foreach (TimeTaskFilter F in Filters)
                {
                    bool isRelevant = false;
                    switch (F.FilterTypeName)
                    {
                        case nameof(TimeTask):
                            isRelevant = ((TimeTask)F.Filterable).HasDateTime(dt);
                            break;
                        case nameof(Label):
                            isRelevant = ((Label)F.Filterable).HasDateTime(dt);
                            break;
                        case nameof(Resource):
                            isRelevant = ((Resource)F.Filterable).HasDateTime(dt);
                            break;
                        case nameof(TaskType):
                            isRelevant = ((TaskType)F.Filterable).HasDateTime(dt);
                            break;
                        case nameof(TimePattern):
                            isRelevant = ((TimePattern)F.Filterable).HasDateTime(dt);
                            break;
                    }
                    //last relevant filter ultimately decides to include
                    if (isRelevant)
                    {
                        hasRelevantFilter = true;
                        include = F.Include;
                    }
                }
                //if no filters exist at this time, exclude
                if (!hasRelevantFilter) include = false;
                //detect a filter edge and add an inclusion zone
                if (prevInclude != include)
                {
                    if (include)
                    {
                        //we are starting an include zone
                        zoneStart = dt;
                    }
                    else
                    {
                        //we are ending an include zone
                        InclusionZones.Add(zoneStart, dt - zoneStart);
                    }
                }
                dt.AddMinutes(5);
            }
            //end any trailing inclusion zones
            if (include)
            {
                InclusionZones.Add(zoneStart, End - zoneStart);
            }
        }
    }
}

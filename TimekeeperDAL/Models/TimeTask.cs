// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using TimekeeperDAL.Tools;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    case nameof(AllocationMethod):
                        if (!AllocationMethodChoices.Contains(AllocationMethod))
                        {
                            AddError(nameof(AllocationMethod), String.Format("{0} must be: {1}",
                                nameof(AllocationMethod),
                                String.Join(", ", AllocationMethod)));
                            hasError = true;
                        }
                        errors = GetErrorsFromAnnotations(nameof(AllocationMethod), AllocationMethod);
                        break;
                    case nameof(TaskType):
                        if (!TaskType.DefaultChoices.Contains(TaskType.Name))
                        {
                            AddError(nameof(TaskType), String.Format("{0} must be: {1}",
                                nameof(TaskType),
                                String.Join(", ", TaskType)));
                            hasError = true;
                        }
                        errors = GetErrorsFromAnnotations(nameof(TaskType), TaskType);
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
        public static readonly List<string> AllocationMethodChoices = new List<string>
        {
            "Eager",
            "Even",
            "EvenCentered",
            "EvenApathetic",
            "Apathetic",
        };
        
        [NotMapped]
        public static TimeSpan MinimumDuration => new TimeSpan(0, 5, 0);

        [NotMapped]
        public TimeSpan Duration => End - Start;

        [NotMapped]
        public Dictionary<DateTime, DateTime> InclusionZones { get; set; }
        
        [NotMapped]
        public Dictionary<DateTime, DateTime> PerZones { get; set; }

        [NotMapped]
        public TimeTaskAllocation TimeAllocation { get; set; }

        public override bool HasDateTime(DateTime dt)
        {
            if ((InclusionZones == null) || (InclusionZones.Count == 0)) return false;
            foreach (var z in InclusionZones)
            {
                if (dt < z.Key) return false;
                if (dt < z.Value) return true;
            }
            return false;
        }
        public async Task BuildInclusionZonesAsync(Dictionary<DateTime, DateTime> PerZonesSubset)
        {
            await Task.Run(() => BuildInclusionZones(PerZonesSubset));
        }
        public void BuildInclusionZones(Dictionary<DateTime, DateTime> PerZonesSubset)
        {
            InclusionZones = new Dictionary<DateTime, DateTime>();
            foreach (var P in PerZonesSubset) BIZPart2(P.Key, P.Value);
        }
        private void BIZPart2(DateTime start, DateTime end)
        {
            DateTime zoneStart = start;
            DateTime dt = start;
            bool include = false;
            while (dt < end)
            {
                //we want to determine if there exists at least one relevant filter that includes this time
                bool prevInclude = include;
                bool hasRelevantFilter = false;
                foreach (TimeTaskFilter F in Filters)
                {
                    bool isRelevant = false;
                    switch (F.FilterTypeName)
                    {
                        case nameof(EF.TimeTask):
                            isRelevant = ((TimeTask)F.Filterable).HasDateTime(dt);
                            break;
                        case nameof(EF.Label):
                            isRelevant = ((Label)F.Filterable).HasDateTime(dt);
                            break;
                        case nameof(EF.Resource):
                            isRelevant = ((Resource)F.Filterable).HasDateTime(dt);
                            break;
                        case nameof(EF.TaskType):
                            isRelevant = ((TaskType)F.Filterable).HasDateTime(dt);
                            break;
                        case nameof(EF.TimePattern):
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
                        //we are starting a new inclusion zone
                        zoneStart = dt;
                    }
                    else
                    {
                        //we are ending an inclusion zone
                        var duration = new TimeSpan(Math.Max(MinimumDuration.Ticks, (dt - zoneStart).Ticks));
                        InclusionZones.Add(zoneStart, zoneStart + duration);
                    }
                }
                dt += MinimumDuration;
            }
            //end any trailing inclusion zones
            if (include)
            {
                InclusionZones.Add(zoneStart, end);
            }
        }
        public async Task BuildPerZonesAsync()
        {
            await Task.Run(() => BuildPerZones());
        }
        public void BuildPerZones()
        {
            PerZones = new Dictionary<DateTime, DateTime>();
            //Find the time allocation if it exists
            TimeAllocation = Allocations.Where(A => Resource.TimeResourceChoices.Contains(A.Resource.Name)).FirstOrDefault();
            if (TimeAllocation == null || TimeAllocation.Per == null)
            {
                //if per is not defined, we will create a per zone the size of the task
                PerZones.Add(Start, End);
            }
            else switch (TimeAllocation?.Per?.Name)
            {
                case "Hour":
                    BPZPart2(dt => dt.HourStart(), dt => dt.AddHours(1));
                    break;
                case "Day":
                    BPZPart2(dt => dt.Date, dt => dt.AddDays(1));
                    break;
                case "Week":
                    BPZPart2(dt => dt.WeekStart(), dt => dt.AddDays(7));
                    break;
                case "Month":
                    BPZPart2(dt => dt.MonthStart(), dt => dt.AddMonths(1));
                    break;
                case "Year":
                    BPZPart2(dt => dt.YearStart(), dt => dt.AddYears(1));
                    break;
            }
        }
        private void BPZPart2(Func<DateTime, DateTime> starter, Func<DateTime, DateTime> adder)
        {
            DateTime perStart = starter(Start);
            DateTime perEnd = adder(perStart);
            //for each relevant per zone
            while (Intersects(perStart, perEnd))
            {
                PerZones.Add(perStart, perEnd);
                perStart = perEnd;
                perEnd = adder(perStart);
            }
        }
        public bool Intersects(DateTime dt) { return Start <= dt && dt <= End; }
        public bool Intersects(Note N) { return Intersects(N.DateTime); }
        public bool Intersects(DateTime start, DateTime end) { return start < End && Start < end; }
        public bool Intersects(TimeTask T) { return Intersects(T.Start, T.End); }
        public bool Intersects(CheckIn CI) { return Intersects(CI.DateTime); }
    }
}

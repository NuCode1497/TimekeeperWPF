using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public partial class TaskType : Filterable
    {
        public override bool HasDateTime(DateTime dt)
        {
            bool result = false;
            foreach (TypedLabeledEntity T in TypedEntities)
            {
                switch (T.GetTypeName())
                {
                    case nameof(TimeTask):
                        result = ((TimeTask)T).HasDateTime(dt);
                        break;
                }
                //if at least one entity with this type exists at time, return true
                if (result) return true;
            }
            return false;
        }

        [NotMapped]
        public static readonly List<string> DefaultChoices = new List<string>()
        {
            "Work",
            "Play",
            "Eat",
            "Sleep",
            "Chore",
        };

        [NotMapped]
        public bool IsDefaultType => DefaultChoices.Contains(Name);

        [NotMapped]
        public override bool IsEditable => !IsDefaultType;
    }
}

using System;
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
                    case nameof(Note):
                        break;
                    case nameof(TimeTask):
                        result = ((TimeTask)T).HasDateTime(dt);
                        break;
                }
                //if at least one entity with this type exists at time, return true
                if (result) return true;
            }
            return false;
        }
    }
}

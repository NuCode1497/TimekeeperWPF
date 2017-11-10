using System;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public partial class Label : Filterable
    {
        public override bool HasDateTime(DateTime dt)
        {
            bool result = false;
            foreach (Labelling L in Labellings)
            {
                switch (L.LabeledEntity.GetTypeName())
                {
                    case nameof(Note):
                        break;
                    case nameof(TimeTask):
                        result = ((TimeTask)L.LabeledEntity).HasDateTime(dt);
                        break;
                    case nameof(TimePattern):
                        result = ((TimePattern)L.LabeledEntity).HasDateTime(dt);
                        break;
                }
                //if at least one entity with this label exists at time, return true
                if (result) return true;
            }
            return false;
        }
    }
}

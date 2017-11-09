using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public partial class Labelling : EntityBase
    {
        public override string this[string columnName] => string.Empty;
    }
}

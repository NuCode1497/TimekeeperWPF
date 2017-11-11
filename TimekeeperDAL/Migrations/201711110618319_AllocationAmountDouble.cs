namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AllocationAmountDouble : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TimeTaskAllocations", "Amount", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TimeTaskAllocations", "Amount", c => c.Long(nullable: false));
        }
    }
}

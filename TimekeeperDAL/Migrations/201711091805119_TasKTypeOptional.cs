namespace TimekeeperDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TasKTypeOptional : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TypedLabeledEntities", new[] { "TaskTypeId" });
            RenameColumn(table: "dbo.TypedLabeledEntities", name: "TaskTypeId", newName: "TaskType_Id");
            AlterColumn("dbo.TypedLabeledEntities", "TaskType_Id", c => c.Int());
            CreateIndex("dbo.TypedLabeledEntities", "TaskType_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TypedLabeledEntities", new[] { "TaskType_Id" });
            AlterColumn("dbo.TypedLabeledEntities", "TaskType_Id", c => c.Int(nullable: false));
            RenameColumn(table: "dbo.TypedLabeledEntities", name: "TaskType_Id", newName: "TaskTypeId");
            CreateIndex("dbo.TypedLabeledEntities", "TaskTypeId");
        }
    }
}

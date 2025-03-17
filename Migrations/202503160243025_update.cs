namespace GuidanceTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "DepartmentId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "DepartmentId", c => c.Int());
        }
    }
}

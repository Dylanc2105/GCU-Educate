namespace GuidanceTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Departments", "DepartmentName", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.Departments", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Departments", "Name", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.Departments", "DepartmentName");
        }
    }
}

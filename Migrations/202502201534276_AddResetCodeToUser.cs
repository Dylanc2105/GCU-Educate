namespace GuidanceTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResetCodeToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ResetCode", c => c.String());
            AddColumn("dbo.AspNetUsers", "ResetCodeExpiry", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ResetCodeExpiry");
            DropColumn("dbo.AspNetUsers", "ResetCode");
        }
    }
}

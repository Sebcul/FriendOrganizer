namespace FriendOrganizer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedMeeting1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Meeting", "Title", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Meeting", "Title", c => c.String(maxLength: 50));
        }
    }
}

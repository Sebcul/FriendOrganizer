namespace FriendOrganizer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingJoke : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Joke",
                c => new
                    {
                        JokeId = c.Int(nullable: false, identity: true),
                        FriendId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.JokeId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Joke");
        }
    }
}

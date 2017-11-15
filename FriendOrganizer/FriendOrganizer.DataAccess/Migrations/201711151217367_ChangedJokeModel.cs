namespace FriendOrganizer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedJokeModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JokeMeeting",
                c => new
                    {
                        Joke_JokeId = c.Int(nullable: false),
                        Meeting_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Joke_JokeId, t.Meeting_Id })
                .ForeignKey("dbo.Joke", t => t.Joke_JokeId, cascadeDelete: true)
                .ForeignKey("dbo.Meeting", t => t.Meeting_Id, cascadeDelete: true)
                .Index(t => t.Joke_JokeId)
                .Index(t => t.Meeting_Id);
            
            AddColumn("dbo.Joke", "Type", c => c.String());
            AddColumn("dbo.Joke", "Setup", c => c.String());
            AddColumn("dbo.Joke", "Punchline", c => c.String());
            DropColumn("dbo.Joke", "FriendId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Joke", "FriendId", c => c.Int(nullable: false));
            DropForeignKey("dbo.JokeMeeting", "Meeting_Id", "dbo.Meeting");
            DropForeignKey("dbo.JokeMeeting", "Joke_JokeId", "dbo.Joke");
            DropIndex("dbo.JokeMeeting", new[] { "Meeting_Id" });
            DropIndex("dbo.JokeMeeting", new[] { "Joke_JokeId" });
            DropColumn("dbo.Joke", "Punchline");
            DropColumn("dbo.Joke", "Setup");
            DropColumn("dbo.Joke", "Type");
            DropTable("dbo.JokeMeeting");
        }
    }
}

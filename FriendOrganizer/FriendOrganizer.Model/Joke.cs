using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendOrganizer.Model
{
    public class Joke
    {
        public Joke()
        {
            Meetings = new List<Meeting>();
        }

        [Required]
        public int JokeId { get; set; }

        public string Type { get; set; }

        public string Setup { get; set; }

        public string Punchline { get; set; }

        public virtual ICollection<Meeting> Meetings { get; set; }
    }
}

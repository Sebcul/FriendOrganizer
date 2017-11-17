using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FriendOrganizer.Model;

namespace FriendOrganizer.UI.Wrapper
{
    public class JokeWrapper : ModelWrapper<Joke>
    {
        public JokeWrapper(Joke model) : base(model)
        {

        }

        public int id { get; set; }

        public string type { get; set; }

        public string setup { get; set; }

        public string punchline { get; set; }



        public override string ToString()
        {
            return setup;
        }
    }
}

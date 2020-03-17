using Rocket.API;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UD_CommandVote.Commands
{
    class CommandVoteCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "CommandVote";

        public string Help => "Allows players to vote on certain commands to be executed";

        public string Syntax => "/commandvote [command]";

        public List<string> Aliases => new List<string> { "cv", "cvote" };

        public List<string> Permissions => new List<string> { "commandvote" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if(command.Length == 0)
            {
                CommandVote.Instance.Vote(caller);
            } else if(command.Length == 1)
            {
                CommandVote.Instance.StartVote(caller, command[0]);
            } else
            {
                UnturnedChat.Say(caller, "You have used the command incorrectly");
            }
        }
    }
}

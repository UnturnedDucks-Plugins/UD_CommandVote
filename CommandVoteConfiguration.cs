using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UD_CommandVote
{
    public class CommandVoteConfiguration : IRocketPluginConfiguration
    {
        public bool DayEnabled;
        public bool NightEnabled;
        public bool AirdropEnabled;
        public bool KickEnabled;

        public float VoteRunTimeSeconds;
        public int RequiredPercentage; 
        public float VoteCooldownTimeSeconds;


        public void LoadDefaults()
        {
            this.DayEnabled = false;
            this.NightEnabled = false;
            this.AirdropEnabled = false;
            this.KickEnabled = false;

            this.VoteRunTimeSeconds = 60;
            this.RequiredPercentage = 60;
            this.VoteCooldownTimeSeconds = 300;
        }
    }
}

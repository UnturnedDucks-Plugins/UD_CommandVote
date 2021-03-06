﻿using Rocket.Core.Plugins;
using Logger = Rocket.Core.Logging.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API.Collections;
using UnityEngine;
using Rocket.Unturned.Chat;
using Rocket.API;
using Rocket.Core.Utils;
using SDG.Unturned;
using Rocket.Unturned.Player;

namespace UD_CommandVote
{
    public class CommandVote : RocketPlugin<CommandVoteConfiguration>
    {
        #region Boilerplate
        public static CommandVote Instance;

        protected override void Load()
        {
            Logger.LogWarning("Loading CommandVote Plugin");
            Instance = this;

            this.ActivatedCommands = new List<string>();

            LoadCommand("Airdrop", Configuration.Instance.AirdropEnabled);
            LoadCommand("Day", Configuration.Instance.DayEnabled);
            LoadCommand("Night", Configuration.Instance.NightEnabled);
            LoadCommand("Kick", Configuration.Instance.KickEnabled);

            this.RequiredPercentage = Configuration.Instance.RequiredPercentage;
            if (this.RequiredPercentage < 0 || this.RequiredPercentage > 1) { this.RequiredPercentage = 60; }

            this.VoteRunTime = Configuration.Instance.VoteRunTimeSeconds;
            this.VoteCooldownTime = Configuration.Instance.VoteCooldownTimeSeconds;
        }

        private void LoadCommand(string name, bool enabled)
        {
            if (enabled)
            {
                Logger.Log(Translate("load_command", name, enabled));
                ActivatedCommands.Add(name.ToLower());
            }
        }

        protected override void Unload()
        {
            Instance = null;
            this.ActivatedCommands = null;
            this.VotedPlayers = null;
        }

        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {"load_command", "Loaded Command: {0}, Enabled: {1}"},
                {"vote_cooldown", "Votes are only available every {0} seconds"},
                {"vote_not_available", "The vote option {0} is not available" },
                {"no_vote_running", "There is no vote currently running, type /cv [votename] to start one!" },
                {"vote_already_running", "There is already a vote running, wait for this one to complete first" },
                {"vote_started", "The vote for {0} that requires {1}% has started and will finish in {2} seconds" },
                {"already_voted", "You have already voted!"},
                {"voted", "The vote for {0} is now at {1}%" },
                {"vote_failed", "Not enough players voted for {0} :(" },
                {"vote_successful", "Vote for {0} was Successful!" }
            };

        #endregion

        #region Commands
        public float VoteRunTime;
        public float VoteCooldownTime;
        public List<string> ActivatedCommands; // activated commands initialized in boilerplate
        public double RequiredPercentage;

        public bool VoteOnCooldown;
        public bool VoteRunning;
        public string CurrentVote;
        public List<string> VotedPlayers;
        public int OnlinePlayers;

        public void Vote(IRocketPlayer caller)
        {
            if (VoteRunning)
            {
                if (hasVoted(caller.DisplayName))
                {
                    UnturnedChat.Say(caller, Translate("already_voted"), Color.yellow);
                    return;
                }
                else
                {
                    // add the player to the already voted list
                    VotedPlayers.Add(caller.DisplayName); 

                    // tell the entire server the status of the poll
                    double CurPercentage = getCurrentPercentage();
                    UnturnedChat.Say(Translate("voted", CurrentVote, CurPercentage), Color.yellow);

                    // check if the minimum has been reached
                    if (CurPercentage > RequiredPercentage)
                    {
                        finishVote();
                        return;
                    }

                }
            }
            else
            {
                // tell the caller that there is no vote actually running
                UnturnedChat.Say(caller, Translate("no_vote_running"), Color.yellow);
            }
        }

        private double getCurrentPercentage()
        {
            return 100 * (((double)VotedPlayers.Count) / OnlinePlayers);
        }

        private bool hasVoted(string caller)
        {
            return VotedPlayers == null || VotedPlayers.Contains(caller);
        }

        public void StartVote(IRocketPlayer caller, string command)
        {
            if (checkCooldown(caller)) { return; }

            if (!VoteRunning)
            {
                if (!ActivatedCommands.Contains(command))
                {
                    UnturnedChat.Say(caller, Translate("vote_not_available", command), Color.yellow);
                    return;
                }

                // initiate the voting sequence
                initiateVote(command);

                UnturnedChat.Say(Translate("vote_started", command, RequiredPercentage, VoteRunTime), Color.yellow);
                Vote(caller);

                // begin the cooldown
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    VoteOnCooldown = false;
                }, VoteCooldownTime + VoteRunTime);

                // set the timer
                TaskDispatcher.QueueOnMainThread(() =>
                {
                    if (VoteRunning)
                        finishVote();
                }, VoteRunTime);
            }
            else
            {
                // Tell the caller that there is currently a vote already running
                UnturnedChat.Say(caller, Translate("vote_already_running"), Color.yellow);
            }
        }

        private bool checkCooldown(IRocketPlayer caller)
        {
            if (VoteCooldownTime > 0 && VoteOnCooldown)
            {
                UnturnedChat.Say(caller, Translate("vote_cooldown", VoteCooldownTime), Color.yellow);
                return true;
            }

            return false;
        }

        private void finishVote()
        {
            double CurPercentage = getCurrentPercentage();
            // check if the minimum has been reached
            if (CurPercentage > RequiredPercentage)
            {
                UnturnedChat.Say(Translate("vote_successful", CurrentVote), Color.yellow);
                runCommand(CurrentVote);
            }
            else
            {
                UnturnedChat.Say(Translate("vote_failed", CurrentVote), Color.yellow);
            }

            resetVote();
        }

        private void resetVote()
        {
            VoteRunning = false;
            VotedPlayers = null;
            CurrentVote = null;
            OnlinePlayers = 0;
        }

        private void initiateVote(string command)
        {
            this.VoteRunning = true;
            this.CurrentVote = command;
            this.VotedPlayers = new List<string>();
            this.OnlinePlayers = Provider.clients.Select(p => UnturnedPlayer.FromCSteamID(p.playerID.steamID)).Count();
            this.VoteOnCooldown = true;
        }

        private void runCommand(string command)
        {
            switch (command.ToLower())
            {
                case ("day"):
                    Logger.LogWarning("Setting time to Day");
                    LightingManager.time = (uint)(LightingManager.cycle * LevelLighting.transition);
                    return;
                case ("night"):
                    // cant actually figure out the time for night
                    Logger.LogWarning("Setting time to Night");
                    LightingManager.time = 2600;
                    return;
                case ("airdrop"):
                    return;
            }
        }
        #endregion
    }
}

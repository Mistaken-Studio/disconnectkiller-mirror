// -----------------------------------------------------------------------
// <copyright file="KillPlayerHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.RoundLogger;
using UnityEngine;

namespace Mistaken.DisconnectKiller
{
    internal class KillPlayerHandler : API.Diagnostics.Module
    {
        public KillPlayerHandler(PluginHandler p)
            : base(p)
        {
            Instance = this;
        }

        public override string Name => "KillPlayer";

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Left += this.Player_Left;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Left -= this.Player_Left;
        }

        internal static KillPlayerHandler Instance { get; private set; }

        internal void RespawnSCP(Player player)
        {
            RLogger.Log("SCP RESPAWN", "SCP", $"Respawning SCP, Current: {player.PlayerToString()}");

            var spectators = RealPlayers.Get(Team.RIP).Where(x => !x.IsOverwatchEnabled).ToArray();

            if (spectators.Length == 0)
            {
                player.IsGodModeEnabled = false;
                MapPlus.Broadcast("RESPAWN", 10, $"SCP player Change, ({player.Id}) {player.Nickname} -> Nobody", Broadcast.BroadcastFlags.AdminChat);
                player.Kill("Unknown cause of death", "TERMINATED SUCCESSFULLY");
            }
            else
            {
                var randomPlayer = spectators[UnityEngine.Random.Range(0, spectators.Length)];

                var position = player.Position + (Vector3.up * 0.5f);
                var hp = player.Health;
                var ahp = player.ArtificialHealth;
                var lvl = player.Level;
                var energy = player.Energy;
                var experience = player.Experience;
                Camera079 camera = player.Camera;

                bool scp079 = player.Role == RoleType.Scp079;
                randomPlayer.SetRole(player.Role, SpawnReason.ForceClass, false);
                this.CallDelayed(
                    .2f,
                    () =>
                    {
                        if (scp079)
                        {
                            randomPlayer.Level = lvl;
                            randomPlayer.Energy = energy;
                            randomPlayer.Experience = experience;
                            if (player.Camera != null)
                                randomPlayer.Camera = player.Camera;
                        }
                        else
                        {
                            randomPlayer.Health = hp;
                            randomPlayer.ArtificialHealth = ahp;
                        }
                    },
                    "KillPlayerHandler.LateSync");

                this.CallDelayed(.5f, () => randomPlayer.Position = position, "KillPlayerHandler.LateTeleport");

                player.SetRole(RoleType.Spectator, SpawnReason.None);
                randomPlayer.Broadcast(10, $"Player {player.GetDisplayName()} left game so you were moved to replace him");
                MapPlus.Broadcast("RESPAWN", 10, $"SCP player Change, ({player.Id}) {player.Nickname} -> ({randomPlayer.Id}) {randomPlayer.Nickname}", Broadcast.BroadcastFlags.AdminChat);
            }
        }

        private void RespawnPlayer(Player currentPlayer)
        {
            if (!currentPlayer.IsReadyPlayer())
                return;

            if (currentPlayer.IsDead)
                return;

            if (currentPlayer.IsScp && currentPlayer.Role != RoleType.Scp0492)
            {
                this.RespawnSCP(currentPlayer);
            }
            else
            {
                currentPlayer.Kill("Heart Attack");
                RLogger.Log("DISCONNECT KILLER", "HUMAN", $"Killing human {currentPlayer.PlayerToString()}");
            }
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (Round.IsStarted)
                this.RespawnPlayer(ev.Player);
        }
    }
}

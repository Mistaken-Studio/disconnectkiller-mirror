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
using Exiled.API.Features.Roles;
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
            Exiled.Events.Handlers.Scp106.Containing += this.Scp106_Containing;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Left -= this.Player_Left;
            Exiled.Events.Handlers.Scp106.Containing -= this.Scp106_Containing;
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
                Scp079Role scp079role = null;
                player.Role.Is<Scp079Role>(out scp079role);
                randomPlayer.SetRole(player.Role, SpawnReason.ForceClass, false);
                this.CallDelayed(
                    .5f,
                    () =>
                    {
                        if (scp079role is null)
                        {
                            randomPlayer.Health = hp;
                            randomPlayer.ArtificialHealth = ahp;
                        }
                        else
                        {
                            var rscp = randomPlayer.Role.As<Scp079Role>();
                            rscp.Level = scp079role.Level;
                            rscp.MaxEnergy = scp079role.MaxEnergy;
                            rscp.Energy = scp079role.Energy;
                            rscp.Experience = scp079role.Experience;
                            if (!(scp079role.Camera is null))
                                rscp.SetCamera(scp079role.Camera);
                        }
                    },
                    "KillPlayerHandler.LateSync");

                if (scp079role is null)
                    this.CallDelayed(.5f, () => randomPlayer.Position = position, "KillPlayerHandler.LateTeleport");

                player.SetRole(RoleType.Spectator, SpawnReason.None);
                randomPlayer.Broadcast(10, $"Player {player.GetDisplayName()} left game so you were moved to replace him");
                MapPlus.Broadcast("RESPAWN", 10, $"SCP player Change, ({player.Id}) {player.Nickname} -> ({randomPlayer.Id}) {randomPlayer.Nickname}", Broadcast.BroadcastFlags.AdminChat);
            }
        }

        private bool scp106contained = false;

        private void RespawnPlayer(Player currentPlayer)
        {
            if (!currentPlayer.IsReadyPlayer())
                return;

            if (currentPlayer.IsDead)
                return;

            if (currentPlayer.Role == RoleType.Scp106 && this.scp106contained)
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

        private void Scp106_Containing(Exiled.Events.EventArgs.ContainingEventArgs ev)
        {
            this.scp106contained = true;
        }
    }
}

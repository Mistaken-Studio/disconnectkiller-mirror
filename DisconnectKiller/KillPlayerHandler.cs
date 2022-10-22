// -----------------------------------------------------------------------
// <copyright file="KillPlayerHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Mistaken.API;
using Mistaken.API.Extensions;
using Mistaken.RoundLogger;
using PlayerStatsSystem;
using UnityEngine;

namespace Mistaken.DisconnectKiller
{
    internal sealed class KillPlayerHandler : API.Diagnostics.Module
    {
        public KillPlayerHandler(PluginHandler p)
            : base(p)
        {
            Instance = this;
        }

        public override string Name => nameof(KillPlayerHandler);

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

        public void RespawnSCP(Player player)
        {
            RLogger.Log("SCP RESPAWN", "SCP", $"Respawning SCP, Current: {player.PlayerToString()}");
            var spectators = RealPlayers.Get(Team.RIP).Where(x => !x.IsOverwatchEnabled).ToArray();

            if (spectators.Length == 0)
            {
                player.IsGodModeEnabled = false;
                MapPlus.Broadcast("RESPAWN", 10, $"SCP player Change, ({player.Id}) {player.Nickname} -> Nobody", Broadcast.BroadcastFlags.AdminChat);
                player.Kill("Unknown cause of death", "TERMINATED SUCCESSFULLY");
                return;
            }

            var randomPlayer = spectators[UnityEngine.Random.Range(0, spectators.Length)];

            var position = player.Position + (Vector3.up * 0.5f);
            var hp = player.Health;
            var ahp = player.ArtificialHealth;
            player.Role.Is<Scp079Role>(out var scp079role);
            randomPlayer.SetRole(player.Role.Type, SpawnReason.ForceClass, false);
            void LateSync()
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
                    if (scp079role.Camera is not null)
                        rscp.SetCamera(scp079role.Camera);
                }
            }

            this.CallDelayed(0.5f, LateSync, nameof(LateSync));

            if (scp079role is null)
                this.CallDelayed(.5f, () => randomPlayer.Position = position, "KillPlayerHandler.LateTeleport");

            player.SetRole(RoleType.Spectator, SpawnReason.None);
            randomPlayer.Broadcast(10, $"Player {player.GetDisplayName()} left game so you were moved to replace him");
            MapPlus.Broadcast("RESPAWN", 10, $"SCP player Change, ({player.Id}) {player.Nickname} -> ({randomPlayer.Id}) {randomPlayer.Nickname}", Broadcast.BroadcastFlags.AdminChat);
        }

        internal static KillPlayerHandler Instance { get; private set; }

        private static bool _scp106contained = false;

        private void RespawnPlayer(Player currentPlayer)
        {
            if (!currentPlayer.IsReadyPlayer())
                return;

            if (currentPlayer.IsDead)
                return;

            if (currentPlayer.Role.Type == RoleType.Scp106 && _scp106contained)
                return;

            if (currentPlayer.IsScp && currentPlayer.Role.Type != RoleType.Scp0492)
            {
                this.RespawnSCP(currentPlayer);
            }
            else
            {
                RLogger.Log("DISCONNECT KILLER", "HUMAN", $"Killing human {currentPlayer.PlayerToString()}");
                currentPlayer.ReferenceHub.playerStats.KillPlayer(new DisruptorDamageHandler(new Footprinting.Footprint(Server.Host.ReferenceHub), -1f));
            }
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (Round.IsStarted)
                this.RespawnPlayer(ev.Player);
        }

        private void Scp106_Containing(Exiled.Events.EventArgs.ContainingEventArgs ev)
        {
            _scp106contained = true;
        }
    }
}

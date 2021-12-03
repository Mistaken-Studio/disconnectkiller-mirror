// -----------------------------------------------------------------------
// <copyright file="KillPlayerHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using Exiled.API.Features;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.RoundLogger;

namespace Mistaken.DisconnectKiller
{
    internal class KillPlayerHandler : API.Diagnostics.Module
    {
        public KillPlayerHandler(PluginHandler p)
            : base(p)
        {
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

        private static void RespawnPlayer(Player currentPlayer)
        {
            if (!currentPlayer.IsReadyPlayer())
                return;

            if (currentPlayer.IsDead)
                return;

            if (currentPlayer.IsScp && currentPlayer.Role != RoleType.Scp0492)
            {
                try
                {
                    RespawnSCP(currentPlayer);
                    return;
                }
                catch (TypeLoadException)
                {
                    Exiled.API.Features.Log.Debug("BetterSCP not found", PluginHandler.Instance.Config.VerbouseOutput);
                }
                catch (Exception ex)
                {
                    Exiled.API.Features.Log.Error(ex);
                    return;
                }
            }

            currentPlayer.Kill("Heart Attack");
            RLogger.Log("DISCONNECT KILLER", "HUMAN", "Killing human");
        }

        private static void RespawnSCP(Player currentSCP)
        {
            Exiled.API.Features.Log.Debug(Assembly.GetAssembly(typeof(BetterSCP.PluginHandler)).FullName, PluginHandler.Instance.Config.VerbouseOutput);
            RLogger.Log("DISCONNECT KILLER", "SCP", "Killing SCP");
            currentSCP.Kill("Heart Attack");
        }

        private void Player_Left(Exiled.Events.EventArgs.LeftEventArgs ev)
        {
            if (Round.IsStarted)
                RespawnPlayer(ev.Player);
        }
    }
}

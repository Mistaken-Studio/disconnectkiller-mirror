// -----------------------------------------------------------------------
// <copyright file="ChangeSCPCommand.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using CommandSystem;
using Mistaken.API.Commands;
using Mistaken.API.Extensions;

namespace Mistaken.DisconnectKiller
{
    [CommandSystem.CommandHandler(typeof(CommandSystem.RemoteAdminCommandHandler))]
    internal class ChangeSCPCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "changeSCP";

        public override string Description => "Gives away SCP";

        public string PluginName => PluginHandler.Instance.Name;

        public override string Command => "changescp";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = sender.GetPlayer();
            if (!player.IsScp)
                return new string[] { "This command is only avaiable for SCPs" };

            success = true;
            KillPlayerHandler.Instance.RespawnSCP(player);
            return new string[] { "Done" };
        }
    }
}

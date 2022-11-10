// -----------------------------------------------------------------------
// <copyright file="ChangeSCPCommand.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using CommandSystem;
using Exiled.API.Features;
using Mistaken.API.Commands;

namespace Mistaken.DisconnectKiller
{
    // [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal sealed class ChangeSCPCommand : IBetterCommand, IPermissionLocked
    {
        public string Permission => "changeSCP";

        public override string Description => "Gives away SCP";

        public string PluginName => PluginHandler.Instance.Name;

        public override string Command => "changescp";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            var player = Player.Get(sender);

            if (!player.IsScp)
                return new string[] { "This command is only avaiable for SCPs" };

            KillPlayerHandler.Instance.RespawnSCP(player);
            success = true;
            return new string[] { "Done" };
        }
    }
}

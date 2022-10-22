// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Mistaken.DisconnectKiller
{
    internal sealed class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        [Description("If true then debug will be displayed")]
        public bool VerbouseOutput { get; set; }
    }
}

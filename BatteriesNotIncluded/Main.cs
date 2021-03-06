﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatteriesNotIncluded.Framework;
using BatteriesNotIncluded.Framework.Managers;
using BatteriesNotIncluded.Framework.MinigameTypes;
using BatteriesNotIncluded.Framework.Network;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace BatteriesNotIncluded {
    [ApiVersion(2, 1)]
    public class Main : TerrariaPlugin {
        public static Minigame ActiveVote;
        public static ArenaManager ArenaManager;
        public static CommandManager CommandManager;
        public static Database ArenaDatabase;
        public static Main Instance;
        public static Config Config;

        public override string Name => "BatteriesNotIncluded";
        public override string Author => "Johuan";
        public override string Description => "Adds minigames to Terraria";
        public override Version Version => new Version(0, 1, 0, 0);

        public Main(Terraria.Main game) : base(game) { }

        public override void Initialize() {
            Instance = this;

            Config = Config.Read(Config.ConfigPath);
            if (!File.Exists(Config.ConfigPath)) {
                Config.Write(Config.ConfigPath);
            }

            ArenaDatabase = new Database("GamemodeArenas").ConnectDB();
            ArenaManager = new ArenaManager();
            CommandManager = new CommandManager();
            GeneralHooks.ReloadEvent += OnReload;

            ServerApi.Hooks.NetGetData.Register(this, GetData);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (ActiveVote != null) {
                    ServerApi.Hooks.GameUpdate.Deregister(Instance, ActiveVote.OnGameUpdate);
                }

                ServerApi.Hooks.NetGetData.Deregister(this, GetData);
                GeneralHooks.ReloadEvent -= OnReload;
            }
            base.Dispose(disposing);
        }

        private void OnReload(ReloadEventArgs e) {
            Config = Config.Read(Config.ConfigPath);
        }

        /// <summary>
        /// Processes data so it can be used in <see cref="DataHandler"/>.
        /// </summary>
        /// <param name="args">The data needed to be processed.</param> 
        private void GetData(GetDataEventArgs args) {
            MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length);
            TSPlayer attacker = TShock.Players[args.Msg.whoAmI];

            if (attacker == null || !attacker.TPlayer.active || !attacker.ConnectionAlive) return;

            DataHandler.HandleData(args, data, attacker);
        }
    }
}

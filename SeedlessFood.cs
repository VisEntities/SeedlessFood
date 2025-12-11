/*
 * Copyright (C) 2024 Game4Freak.io
 * This mod is provided under the Game4Freak EULA.
 * Full legal terms can be found at https://game4freak.io/eula/
 */

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Seedless Food", "VisEntities", "1.0.2")]
    [Description("Prevents seeds from being produced when players eat fruits or vegetables.")]

    public class SeedlessFood : RustPlugin
    {
        #region Fields

        private static SeedlessFood _plugin;
        private static Configuration _config;
        private HashSet<ulong> _playersConsuming = new HashSet<ulong>();

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Block Seeds From (shortnames)")]
            public List<string> BlockSeedsFrom { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                BlockSeedsFrom = new List<string>
                {
                    "pumpkin",
                    "corn",
                    "potato",
                    "black.berry",
                    "blue.berry",
                    "green.berry",
                    "white.berry",
                    "yellow.berry",
                    "red.berry"
                }
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
        }

        private void Unload()
        {
            _playersConsuming?.Clear();
            _config = null;
            _plugin = null;
        }

        private void OnItemAction(Item item, string action, BasePlayer player)
        {
            if (player == null || item == null)
                return;

            if (action == "consume")
            {
                if (_config.BlockSeedsFrom.Contains(item.info.shortname))
                {
                    _playersConsuming.Add(player.userID);

                    NextTick(() =>
                    {
                        _playersConsuming.Remove(player.userID);
                    });
                }
            }
        }

        private object CanAcceptItem(ItemContainer container, Item item, int targetPos)
        {
            if (!item.info.shortname.StartsWith("seed."))
                return null;

            BasePlayer player = container.GetOwnerPlayer();
            if (player == null)
                return null;

            if (!_playersConsuming.Contains(player.userID))
                return null;

            string foodName = item.info.shortname.Substring(5);
            if (!_config.BlockSeedsFrom.Contains(foodName))
                return null;

            item.Remove();
            return ItemContainer.CanAcceptResult.CannotAccept;
        }

        #endregion Oxide Hooks
    }
}

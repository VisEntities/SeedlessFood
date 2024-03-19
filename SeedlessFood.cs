using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Seedless Food", "VisEntities", "1.0.0")]
    [Description("Automatically removes seeds from your inventory when you eat fruits or vegetables.")]

    public class SeedlessFood : RustPlugin
    {
        #region Fields

        private static SeedlessFood _plugin;
        private static Configuration _config;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Food Item Shortnames")]
            public List<string> FoodItemShortnames { get; set; }
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
                FoodItemShortnames = new List<string>
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
            _config = null;
            _plugin = null;
        }

        private void OnItemAction(Item item, string action, BasePlayer player)
        {
            if (player == null || item == null)
                return;

            if (action == "consume")
            {
                if (_config.FoodItemShortnames.Contains(item.info.shortname))
                {
                    string seedShortname = "seed." + item.info.shortname;
                    NextTick(() =>
                    {
                        RemoveSeedItemsFromPlayer(seedShortname, player);
                    });
                }
            }
        }

        #endregion Oxide Hooks

        #region Functions

        private void RemoveSeedItemsFromPlayer(string seedShortname, BasePlayer player)
        {
            Item[] allItems = player.inventory.AllItems();
            foreach (Item seed in allItems)
            {
                if (seed.info.shortname == seedShortname)
                {
                    seed.Remove();
                    break;
                }
            }
        }

        #endregion Functions
    }
}
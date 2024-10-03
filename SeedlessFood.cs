/*
 * Copyright (C) 2024 Game4Freak.io
 * This mod is provided under the Game4Freak EULA.
 * Full legal terms can be found at https://game4freak.io/eula/
 */

using Facepunch;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Seedless Food", "VisEntities", "1.0.1")]
    [Description("Removes seeds from players' inventories when they eat fruits or vegetables.")]

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

            [JsonProperty("Food Item Short Names")]
            public List<string> FoodItemShortNames { get; set; }
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
                FoodItemShortNames = new List<string>
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
                if (_config.FoodItemShortNames.Contains(item.info.shortname))
                {
                    string seedShortName = "seed." + item.info.shortname;
                    NextTick(() =>
                    {
                        RemoveSeedItemsFromPlayer(seedShortName, player);
                    });
                }
            }
        }

        #endregion Oxide Hooks

        #region Seeds Removal

        private void RemoveSeedItemsFromPlayer(string seedShortName, BasePlayer player)
        {
            List<Item> allItems = Pool.Get<List<Item>>();
            player.inventory.GetAllItems(allItems);

            foreach (Item seed in allItems)
            {
                if (seed.info.shortname == seedShortName)
                {
                    seed.Remove();
                    break;
                }
            }

            Pool.FreeUnmanaged(ref allItems);
        }

        #endregion Seeds Removal
    }
}
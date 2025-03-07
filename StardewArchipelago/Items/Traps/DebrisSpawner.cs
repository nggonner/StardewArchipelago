﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace StardewArchipelago.Items.Traps
{
    public class DebrisSpawner
    {
        private const int TWIG_1 = 294;
        private const int TWIG_2 = 295;
        private const int STONE_1 = 343;
        private const int STONE_2 = 450;
        private const int WEEDS = 750;

        private IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private TrapDifficultyBalancer _difficultyBalancer;

        public DebrisSpawner(IMonitor monitor, ArchipelagoClient archipelago, TrapDifficultyBalancer difficultyBalancer)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _difficultyBalancer = difficultyBalancer;
        }

        public void CreateDebris()
        {
            var farm = Game1.getFarm();
            var hasGoldClock = farm.isBuildingConstructed("Gold Clock");
            var currentLocation = Game1.player.currentLocation;
            var locations = new List<GameLocation>();
            locations.Add(farm);
            if (currentLocation != farm)
            {
                locations.Add(currentLocation);
            }

            var amountOfDebris = _difficultyBalancer.AmountOfDebris[_archipelago.SlotData.TrapItemsDifficulty];
            if (hasGoldClock)
            {
                amountOfDebris /= 2;
            }
            var amountOfDebrisPerLocation = amountOfDebris / locations.Count;
            foreach (var gameLocation in locations)
            {
                if (hasGoldClock && gameLocation == farm)
                {
                    SpawnDebris(gameLocation, amountOfDebrisPerLocation / 2);
                }
                else
                {
                    SpawnDebris(gameLocation, amountOfDebrisPerLocation);
                }
            }
        }

        private void SpawnDebris(GameLocation location, int amount)
        {
            for (var i = 0; i < amount; ++i)
            {
                var tile = new Vector2(Game1.random.Next(location.map.Layers[0].LayerWidth), Game1.random.Next(location.map.Layers[0].LayerHeight));
                var noSpawn = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NoSpawn", "Back") != null;
                var wood = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Type", "Back") == "Wood";
                var tileIsClear = location.isTileLocationTotallyClearAndPlaceable(tile) && !location.objects.ContainsKey(tile) && !location.terrainFeatures.ContainsKey(tile);
                if (noSpawn || wood || !tileIsClear)
                {
                    continue;
                }

                if (Game1.random.NextDouble() < 0.05)
                {
                    SpawnRandomTree(location, tile);
                    continue;
                }

                var itemToSpawn = ChooseRandomDebris(location);
                location.objects.Add(tile, new Object(tile, itemToSpawn, 1));
            }
        }

        private static void SpawnRandomTree(GameLocation location, Vector2 tile)
        {
            location.terrainFeatures.Add(tile, new Tree(Game1.random.Next(3) + 1, Game1.random.Next(3)));
        }

        private static int ChooseRandomDebris(GameLocation location)
        {
            var typeRoll = Game1.random.NextDouble();
            if (typeRoll < 0.33)
            {
                return Game1.random.NextDouble() < 0.5 ? TWIG_1 : TWIG_2;
            }
            if (typeRoll < 0.67)
            {
                return Game1.random.NextDouble() < 0.5 ? STONE_1 : STONE_2;
            }

            return GameLocation.getWeedForSeason(Game1.random, location.GetSeasonForLocation());
        }
    }
}

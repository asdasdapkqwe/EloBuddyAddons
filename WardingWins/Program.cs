﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace WardingWins
{
    public class Program
    {
        static readonly List<KeyValuePair<int, String>> _wards = new List<KeyValuePair<int, String>>
        {
            new KeyValuePair<int, String>(3340, "Warding Totem Trinket"),
            new KeyValuePair<int, String>(2301, "Eye of the Watchers"),
            new KeyValuePair<int, String>(2302, "Eye of the Oasis"),
            new KeyValuePair<int, String>(2303, "Eye of the Equinox"),
            new KeyValuePair<int, String>(3205, "Quill Coat"),
            new KeyValuePair<int, String>(3207, "Spirit Of The Ancient Golem"),
            new KeyValuePair<int, String>(3154, "Wriggle's Lantern"),
            new KeyValuePair<int, String>(2049, "Sight Stone"),
            new KeyValuePair<int, String>(2045, "Ruby Sightstone"),
            new KeyValuePair<int, String>(3160, "Feral Flare"),
            new KeyValuePair<int, String>(2050, "Explorer's Ward"),
            new KeyValuePair<int, String>(2044, "Stealth Ward"),
        };
        public static float lastuseward = 0;
        public static Menu WardingWins;

        public class Wardspoting
        {
            public static WardSpot _PutSafeWard;
        }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);
            WardingWins = MainMenu.AddMenu("WardingWins", "wardingwins");
            WardingWins.AddGroupLabel("Warding is OP!");
            WardingWins.AddSeparator();
            WardingWins.Add("drawplaces", new CheckBox("Draw ward places", true));
            WardingWins.AddSeparator();
            WardingWins.Add("drawDistance", new Slider("Don't draw if the distance >", 2000, 1, 10000));
            WardingWins.Add("placekey", new KeyBind("NormalWard Key", false, KeyBind.BindTypes.HoldActive));
            WardingWins.Add("placekeypink", new KeyBind("PinkWard Key", false, KeyBind.BindTypes.HoldActive));
            WardingWins.AddSeparator();
            WardingWins.AddLabel("Made by GameHackerPM.");
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (WardingWins["drawplaces"].Cast<CheckBox>().CurrentValue)
            {
                WardDatabase.DrawWardSpots();
                WardDatabase.DrawSafeWardSpots();
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            InventorySlot wardSpellSlot = null;
            if (WardingWins["placekey"].Cast<KeyBind>().CurrentValue)
            {
                wardSpellSlot = _wards.Select(x => x.Key).Where(Item.CanUseItem).Select(wardId => ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId)).FirstOrDefault();
            }
            else if (WardingWins["placekeypink"].Cast<KeyBind>().CurrentValue)
            {
                wardSpellSlot = WardDatabase.GetPinkSlot();
            }

            if (wardSpellSlot == null || lastuseward + 1000 > Environment.TickCount)
            {
                return;
            }
            Vector3? nearestWard = WardDatabase.FindNearestWardSpot(Drawing.ScreenToWorld(Game.CursorPos.X, Game.CursorPos.Y));

            if (nearestWard != null)
            {
                if (wardSpellSlot != null)
                {
                    ObjectManager.Player.Spellbook.CastSpell(wardSpellSlot.SpellSlot, (Vector3)nearestWard);
                    lastuseward = Environment.TickCount;
                }
            }

            WardSpot nearestSafeWard = WardDatabase.FindNearestSafeWardSpot(Drawing.ScreenToWorld(Game.CursorPos.X, Game.CursorPos.Y));

            if (nearestSafeWard != null)
            {
                if (wardSpellSlot != null)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, nearestSafeWard.MovePosition);
                    Wardspoting._PutSafeWard = nearestSafeWard;
                }
            }

            if (Wardspoting._PutSafeWard != null && lastuseward + 1000 < Environment.TickCount)
            {
                wardSpellSlot = _wards.Select(x => x.Key).Where(id => Item.CanUseItem(id)).Select(wardId => ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId)).FirstOrDefault();
                if (Math.Sqrt(Math.Pow(Wardspoting._PutSafeWard.ClickPosition.X - ObjectManager.Player.Position.X, 2) + Math.Pow(Wardspoting._PutSafeWard.ClickPosition.Y - ObjectManager.Player.Position.Y, 2)) <= 640.0)
                {
                    if (WardingWins["placekey"].Cast<KeyBind>().CurrentValue)
                    {
                        wardSpellSlot = _wards.Select(x => x.Key).Where(Item.CanUseItem).Select(wardId => ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId)).FirstOrDefault();
                    }
                    else if (WardingWins["placekeypinky"].Cast<KeyBind>().CurrentValue)
                    {
                        wardSpellSlot = WardDatabase.GetPinkSlot();
                    }
                    if (wardSpellSlot != null)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(wardSpellSlot.SpellSlot, Wardspoting._PutSafeWard.ClickPosition);
                        lastuseward = Environment.TickCount;
                    }
                    Wardspoting._PutSafeWard = null;
                }
            }
        }

        Obj_AI_Base GetNearObject(String name, Vector3 pos, int maxDistance)
        {
            return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.Name == name && x.Distance(pos) <= maxDistance);
        }

        Vector3 GetWardPos(Vector3 lastPos, int radius = 165, int precision = 3)
        {
            var count = precision;

            while (count > 0)
            {
                var vertices = radius;

                var wardLocations = new WardLocation[vertices];
                var angle = 2 * Math.PI / vertices;

                for (var i = 0; i < vertices; i++)
                {
                    var th = angle * i;
                    var pos = new Vector3((float)(lastPos.X + radius * Math.Cos(th)), (float)(lastPos.Y + radius * Math.Sin(th)), 0);
                    wardLocations[i] = new WardLocation(pos, NavMesh.IsWallOfGrass(pos, 5));
                }

                var grassLocations = new List<GrassLocation>();

                for (var i = 0; i < wardLocations.Length; i++)
                {
                    if (!wardLocations[i].Grass) continue;
                    if (i != 0 && wardLocations[i - 1].Grass)
                        grassLocations.Last().Count++;
                    else
                        grassLocations.Add(new GrassLocation(i, 1));
                }

                var grassLocation = grassLocations.OrderByDescending(x => x.Count).FirstOrDefault();

                if (grassLocation != null) //else: no pos found. increase/decrease radius?
                {
                    var midelement = (int)Math.Ceiling(grassLocation.Count / 2f);
                    lastPos = wardLocations[grassLocation.Index + midelement - 1].Pos;
                    radius = (int)Math.Floor(radius / 2f);
                }

                count--;
            }

            return lastPos;
        }

        class WardLocation
        {
            public readonly Vector3 Pos;
            public readonly bool Grass;

            public WardLocation(Vector3 pos, bool grass)
            {
                Pos = pos;
                Grass = grass;
            }
        }

        class GrassLocation
        {
            public readonly int Index;
            public int Count;

            public GrassLocation(int index, int count)
            {
                Index = index;
                Count = count;
            }
        }
    }
}

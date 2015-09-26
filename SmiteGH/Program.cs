using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using EloBuddy.SDK.Rendering;
using Color1 = System.Drawing.Color;

namespace SmiteGH
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static double TotalDamage = 0;

        public static Spell.Targeted Smite;
        public static Obj_AI_Base Monster;
        public static string[] SupportedChampions =
        {
            "Nunu" , "Chogath", "Shaco", "Vi", "MasterYi", "Rengar",
            "Nasus" , "Khazix", "Fizz", "Elise"
        };
        public static string[] MonstersNames =
        {
            "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith",
            "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", 
            "SRU_Red", "SRU_Krug", "SRU_Dragon", "Sru_Crab", "SRU_Baron"
        };
        public static Menu SmiteGHMenu, MobsToSmite, DrawingMenu;
        private static string[] SmiteNames = new[] { "s5_summonersmiteplayerganker", "itemsmiteaoe", "s5_summonersmitequick", "s5_summonersmiteduel", "summonersmite" };
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);

            if (SmiteNames.Contains(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner1).Name))
            {
                Smite = new Spell.Targeted(SpellSlot.Summoner1, (uint)570f);
            }
            if (SmiteNames.Contains(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Summoner2).Name))
            {
                Smite = new Spell.Targeted(SpellSlot.Summoner2, (uint)570f);
            }

            if (Smite == null) //if you don't have smite, so ? ^_^ Why we load?
                return;

            if (SupportedChampions.Contains(ObjectManager.Player.ChampionName))
                Chat.Print("[SmiteGH Loaded] " + ObjectManager.Player.ChampionName + " Has Loaded!", Color1.Violet);
            else
                Chat.Print("[SmiteGH Loaded] " + ObjectManager.Player.ChampionName + " is not supported, but Smite still working!", Color1.Violet);

            SmiteGHMenu = MainMenu.AddMenu("SmiteGH", "smitegh");
            SmiteGHMenu.AddGroupLabel("SmiteGH");
            SmiteGHMenu.AddSeparator();
            SmiteGHMenu.Add("active", new CheckBox("Enabled"));
            SmiteGHMenu.Add("activekey", new KeyBind("Enabled (Toggle Key)", true, KeyBind.BindTypes.PressToggle));
            SmiteGHMenu.AddSeparator();
            SmiteGHMenu.AddLabel("Made By GameHackerPM");

            MobsToSmite = SmiteGHMenu.AddSubMenu("Monsters", "Monsters");
            MobsToSmite.AddGroupLabel("Monsters Settings");
            MobsToSmite.AddSeparator();
            MobsToSmite.Add("killsmite", new CheckBox("KS Enemy with Smite"));
            MobsToSmite.AddSeparator();
            MobsToSmite.Add("SRU_Baron", new CheckBox("Baron Enabled"));
            MobsToSmite.Add("SRU_Dragon", new CheckBox("Dragon Enabled"));
            MobsToSmite.Add("SRU_Blue", new CheckBox("Blue Enabled"));
            MobsToSmite.Add("SRU_Red", new CheckBox("Red Enabled"));
            MobsToSmite.Add("SRU_Gromp", new CheckBox("Gromp Enabled"));
            MobsToSmite.Add("SRU_Murkwolf", new CheckBox("Murkwolf Enabled"));
            MobsToSmite.Add("SRU_Krug", new CheckBox("Krug Enabled"));
            MobsToSmite.Add("SRU_Razorbeak", new CheckBox("Razorbeak Enabled"));
            MobsToSmite.Add("Sru_Crab", new CheckBox("Crab Enabled"));

            DrawingMenu = SmiteGHMenu.AddSubMenu("Drawing", "drawing");
            DrawingMenu.AddGroupLabel("Drawing Settings");
            DrawingMenu.AddSeparator();
            DrawingMenu.Add("draw", new CheckBox("Enabled"));
            DrawingMenu.AddSeparator(10);
            DrawingMenu.Add("smite", new CheckBox("Draw Smite"));
            DrawingMenu.Add("drawTxt", new CheckBox("Draw Text"));
            DrawingMenu.Add("killable", new CheckBox("Draw Circle on Killable Monster"));


            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_Settings;
        }

        public static void Drawing_Settings(EventArgs args)
        {
            if (DrawingMenu["draw"].Cast<CheckBox>().CurrentValue == false)
                return;

            if (DrawingMenu["drawTxt"].Cast<CheckBox>().CurrentValue)
            {
                if (SmiteGHMenu["active"].Cast<CheckBox>().CurrentValue && SmiteGHMenu["activekey"].Cast<KeyBind>().CurrentValue)
                    Drawing.DrawText(Drawing.WorldToScreen(Player.Instance.Position) - new Vector2(30, -30), Color1.White, "Smite : ON", 2);
                else
                    Drawing.DrawText(Drawing.WorldToScreen(Player.Instance.Position) - new Vector2(30, -30), Color1.DarkRed, "Smite : OFF", 2);
            }

            if (DrawingMenu["smite"].Cast<CheckBox>().CurrentValue)
                if (Smite.IsReady())
                    Circle.Draw(Color.CadetBlue, 500f, ObjectManager.Player.ServerPosition);
                else
                    Circle.Draw(Color.Red, 500f, ObjectManager.Player.ServerPosition);

            if (DrawingMenu["killable"].Cast<CheckBox>().CurrentValue)
            {
                Monster = GetNearest(ObjectManager.Player.ServerPosition);
                if (Monster != null)
                {
                    if (Monster.Health <= (TotalDamage == 0 ? GetSmiteDamage() : TotalDamage) && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < 900f)
                        Circle.Draw(Color.Purple, 100f, Monster.ServerPosition);
                }
            }
        }

        public static int GetSmiteDamage()
        {
            int[] CalcSmiteDamage =
            {
                20 * ObjectManager.Player.Level + 370,
                30 * ObjectManager.Player.Level + 330,
                40 * ObjectManager.Player.Level + 240,
                50 * ObjectManager.Player.Level + 100
            };
            return CalcSmiteDamage.Max();
        }

        static double SmiteChampDamage()
        {
            if (Smite.Slot == EloBuddy.SDK.Extensions.GetSpellSlotFromName(ObjectManager.Player, "s5_summonersmiteduel"))
            {
                var damage = new int[] { 54 + 6 * ObjectManager.Player.Level };
                return Player.CanUseSpell(Smite.Slot) == SpellState.Ready ? damage.Max() : 0;
            }

            if (Smite.Slot == EloBuddy.SDK.Extensions.GetSpellSlotFromName(ObjectManager.Player, "s5_summonersmiteplayerganker"))
            {
                var damage = new int[] { 20 + 8 * ObjectManager.Player.Level };
                return Player.CanUseSpell(Smite.Slot) == SpellState.Ready ? damage.Max() : 0;
            }
            return 0;
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            if (SmiteGHMenu["killsmite"].Cast<CheckBox>().CurrentValue)
            {
                try
                {
                    var KillEnemy =
                        HeroManager.Enemies.FirstOrDefault(hero => hero.IsValidTarget(500f)
                            && SmiteChampDamage() >= hero.Health);
                    if (KillEnemy != null)
                        Player.CastSpell(Smite.Slot, KillEnemy);
                }
                catch { }
            }

            if (SmiteGHMenu["active"].Cast<CheckBox>().CurrentValue || SmiteGHMenu["activekey"].Cast<KeyBind>().CurrentValue)
            {
                double SpellDamage = 0;
                Monster = GetNearest(ObjectManager.Player.ServerPosition);
                if (Monster != null && MobsToSmite[Monster.BaseSkinName].Cast<CheckBox>().CurrentValue)
                {
                    switch (ObjectManager.Player.ChampionName)
                    {
                        #region Nunu
                        case "Nunu":
                            {
                                Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, (uint)200f);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && Q.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Q.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.Nunu(Q.Level - 1);
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(Q.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        #region ChoGath
                        case "Chogath":
                            {
                                Spell.Targeted R = new Spell.Targeted(SpellSlot.R, (uint)175f);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && R.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < R.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.ChoGath();
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(R.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        #region Shaco
                        case "Shaco":
                            {
                                Spell.Targeted E = new Spell.Targeted(SpellSlot.E, (uint)625f);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && E.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < E.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.Shaco(E.Level - 1);
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(E.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        #region Vi
                        case "Vi":
                            {
                                Spell.Active E = new Spell.Active(SpellSlot.E, (uint)600f);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && E.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < E.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.Vi(E.Level - 1);
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(E.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        #region MasterYi
                        case "MasterYi":
                            {
                                Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, (uint)600f);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && Q.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Q.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.Master(Q.Level - 1);
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(Q.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        #region Rengar
                        case "Rengar":
                            {
                                Spell.Active Q = new Spell.Active(SpellSlot.Q, (uint)ObjectManager.Player.AttackRange);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && Q.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Q.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.Rengar(Q.Level - 1);
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(Q.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        #region Nasus
                        case "Nasus":
                            {
                                Spell.Active Q = new Spell.Active(SpellSlot.Q, (uint)ObjectManager.Player.AttackRange);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && Q.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Q.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.Nasus(Q.Level - 1);
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(Q.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        #region KhaZix
                        case "Khazix":
                            {
                                Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, (uint)325f);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && Q.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Q.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.Khazix(Q.Level - 1);
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(Q.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        #region Fizz
                        case "Fizz":
                            {
                                Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, (uint)550f);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && Q.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Q.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.Fizz(Q.Level - 1);
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(Q.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        #region Elise
                        case "Elise":
                            {
                                Spell.Targeted Q = new Spell.Targeted(SpellSlot.Q, (uint)475f);
                                //Smite and Spell  ==> OKAY
                                if (Smite.IsReady() && Q.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Q.Range
                                    && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    SpellDamage = GetDamages.Elise(Q.Level - 1, Monster);
                                    TotalDamage = SpellDamage + GetSmiteDamage();
                                    if (Monster.Health <= TotalDamage)
                                    {
                                        Player.CastSpell(Q.Slot, Monster);
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                }
                                //If Spell is busy, Go Smite only! ^_^
                                else if (Smite.IsReady() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                {
                                    if (Monster.Health <= GetSmiteDamage())
                                    {
                                        Player.CastSpell(Smite.Slot, Monster);
                                    }
                                    TotalDamage = 0;
                                }
                                break;
                            }
                        #endregion
                        default:
                            {
                                if (Smite.IsReady() && Monster.Health <= GetSmiteDamage() && Vector3.Distance(ObjectManager.Player.ServerPosition, Monster.ServerPosition) < Smite.Range)
                                    Player.CastSpell(Smite.Slot, Monster);
                                TotalDamage = 0;
                            }
                            break;
                    }
                }
            }
        }

        public static Obj_AI_Minion GetNearest(Vector3 pos)
        {
            var mobs = ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValid && MonstersNames.Any(name => minion.Name.StartsWith(name)) && !MonstersNames.Any(name => minion.Name.Contains("Mini")) && !MonstersNames.Any(name => minion.Name.Contains("Spawn")));
            var objAimobs = mobs as Obj_AI_Minion[] ?? mobs.ToArray();
            Obj_AI_Minion NearestMonster = objAimobs.FirstOrDefault();
            double? nearest = null;
            foreach (Obj_AI_Minion Monster in objAimobs)
            {
                double distance = Vector3.Distance(pos, Monster.Position);
                if (nearest == null || nearest > distance)
                {
                    nearest = distance;
                    NearestMonster = Monster;
                }
            }
            return NearestMonster;
        }
    }
}
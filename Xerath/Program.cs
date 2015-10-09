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
using Color1 = System.Drawing.Color; // For Drawing.. you have to get this!

namespace Xerath
{
    class Program
    {
        public const string ChampionName = "Xerath";
        public static string LastCastedSpell;
        public static DateTime LastCastedSpellTime;

        //Orbwalker instance
        //public static Orbwalker Orbwalker;

        //Spells
        public static List<Spell.Skillshot> SpellList = new List<Spell.Skillshot>();

        public static Spell.Chargeable Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;

        //Menu
        public static Menu Config;

        private static AIHeroClient Player;

        private static Vector2 PingLocation;
        private static int LastPingT = 0;
        private static bool AttacksEnabled
        {
            get
            {
                if (IsCastingR)
                    return false;

                if (Q.IsCharging)
                    return false;

                //if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                //    return IsPassiveUp || (!Q.IsReady() && !W.IsReady() && !E.IsReady());

                return true;
            }
        }

        public static bool IsPassiveUp
        {
            get { return ObjectManager.Player.HasBuff("xerathascended2onhit"); }
        }

        public static bool IsCastingR
        {
            get
            {
                return ObjectManager.Player.HasBuff("XerathLocusOfPower2") ||
                       (LastCastedSpell == "XerathLocusOfPower2" &&
                        Environment.TickCount - LastCastedSpellTime.Ticks < 500);
            }
        }

        public static class RCharge
        {
            public static int CastT;
            public static int Index;
            public static Vector3 Position;
            public static bool TapKeyPressed;
        }
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);
            Q = new Spell.Chargeable(SpellSlot.Q, 750, 1550, (int)1.5f);
            W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 250, null, 800);
            E = new Spell.Skillshot(SpellSlot.E, 1150, SkillShotType.Linear);
            R = new Spell.Skillshot(SpellSlot.R, (uint)(1200 * R.Level + 2000), SkillShotType.Circular, 250, null, 675);

            Gapcloser.OnGapCloser += OnGapCloser;
            Spellbook.OnCastSpell += CastSpell;
            Game.OnUpdate += GameOnUpdate;
            Interrupter.OnInterruptableSpell += OnPossibleToInterrupt;
            AIHeroClient.OnProcessSpellCast += OnProcessSpell;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Game.OnWndProc += Game_OnWndProc;
            EloBuddy.Player.OnIssueOrder += OnIssueOrder;
            Orbwalker.OnPreAttack += Orb_BeforeAttack;
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            
        }

        static void Orb_BeforeAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            args.Process = AttacksEnabled;
        }

        static void OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (IsCastingR) //blackmovment
            {
                args.Process = false;
            }
        }

        static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)0x0101) // KeyUP
                RCharge.TapKeyPressed = true;
        }

        private static void GameOnUpdate(EventArgs args)
        {
            UseSpells(true, true, true);
        }

        private static void Combo()
        {

            //UseSpells(Config.Item("UseQCombo").GetValue<bool>(), Config.Item("UseWCombo").GetValue<bool>(),
            //    Config.Item("UseECombo").GetValue<bool>());
        }

        private static void Harass()
        {
            //UseSpells(Config.Item("UseQHarass").GetValue<bool>(), Config.Item("UseWHarass").GetValue<bool>(),
            //    false);
        }

        private static void UseSpells(bool useQ, bool useW, bool useE)
        {
            var qTarget = TargetSelector.GetTarget(Q.MaximumRange, DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range + W.Width * 0.5f, DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);

            if (eTarget != null && useE && E.IsReady())
            {
                if (Player.Distance(eTarget) < E.Range * 0.4f)
                    E.Cast(eTarget);
                else if ((!useW || !W.IsReady()))
                    E.Cast(eTarget);
            }

            if (useQ && Q.IsReady() && qTarget != null)
            {
                if (Q.IsCharging)
                {
                    Q.Cast(qTarget);
                }
                else if (!useW || !W.IsReady() || Player.Distance(qTarget) > W.Range)
                {
                    Q.StartCharging();
                }
            }

            if (wTarget != null && useW && W.IsReady())
                W.Cast(wTarget);
        }

        private static void OnGapCloser(AIHeroClient sender, Gapcloser.GapCloserEventArgs e)
        {
            //Anti Gap closer

            if (Player.Distance(sender) < E.Range)
            {
                E.Cast(sender);
            }
        }

        private static void OnPossibleToInterrupt(Obj_AI_Base sender, InterruptableSpellEventArgs interruptableSpellEventArgs)
        {
            //interrupt skills

            if (Player.Distance(sender) < E.Range)
            {
                E.Cast(sender);
            }
        }

        private static void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "XerathLocusOfPower2")
                {
                    RCharge.CastT = 0;
                    RCharge.Index = 0;
                    RCharge.Position = new Vector3();
                    RCharge.TapKeyPressed = false;
                }
                else if (args.SData.Name == "xerathlocuspulse")
                {
                    RCharge.CastT = Environment.TickCount;
                    RCharge.Index++;
                    RCharge.Position = args.End;
                    RCharge.TapKeyPressed = false;
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {

        }

        private static void CastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            LastCastedSpell = sender.GetSpell(args.Slot).Name;
            LastCastedSpellTime = DateTime.Now;
        }
    }
}

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
using System.Diagnostics;
using Color1 = System.Drawing.Color;
using System.Threading;

namespace SummonersTimer
{
    public class Program
    {

        private static TimeSpan ClockNow, DownTime, CDTime;
        public static List<Messages> MessageList;
        private static Stopwatch sw;
        public static Menu SummonersMenu;
        public static string Message = "#enemyname used the #spell and will works on #time.";

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);
            sw = new Stopwatch();
            MessageList = new List<Messages>();
            MyThreads thrd = new MyThreads();
            Thread mythread = new Thread(new ThreadStart(thrd.MessageChecker));
            mythread.IsBackground = true;
            mythread.Name = "Checker";
            mythread.Start();
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
            sw.Start();

            SummonersMenu = MainMenu.AddMenu("SummonersTimer", "mm");
            SummonersMenu.AddGroupLabel("Summoners Timer");
            SummonersMenu.AddSeparator();
            SummonersMenu.Add("active", new CheckBox("Enabled"));
            SummonersMenu.AddSeparator();
            SummonersMenu.Add("toAll", new CheckBox("Tell my Team", false));
            SummonersMenu.AddSeparator();
            SummonersMenu.AddLabel("You can change the message to whatever you want! you can set it by");
            SummonersMenu.AddLabel("chat message starting with \"..say\" and the message.");
            SummonersMenu.AddLabel("Example : ..say #enemyname used the #spell and will works on #time.");
            SummonersMenu.AddSeparator();
            SummonersMenu.AddLabel("#enemyname ==> Enemy Champion name, #spell ==> Summoner Spell,");
            SummonersMenu.AddLabel("#time ==> The time which the spell is ready.");
            SummonersMenu.AddSeparator();
            SummonersMenu.Add("message", new CheckBox(Message));
            SummonersMenu.AddSeparator();
            SummonersMenu.AddSeparator();
            SummonersMenu.AddLabel("Made By GameHackerPM.");
            Chat.OnInput += Chat_OnInput;
        }

        private static void Chat_OnInput(ChatInputEventArgs args)
        {
            if (!args.Input.StartsWith("..say "))
                return;

            args.Process = false;
            Message = args.Input.Replace("..say ", "");
            SummonersMenu["message"].DisplayName = Message;
        }

        private static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!args.SData.Name.ToLower().StartsWith("summoner"))
                return;

            string SpellS = "";
            int SpellCD = 0;
            if (sender.IsEnemy)//sender.IsEnemy
            {
                switch (args.SData.Name.ToLower())
                {
                    case "summonerheal":
                        {
                            SpellCD = 240;
                            SpellS = "Heal";
                            break;
                        }
                    case "summonerdot"://ignite
                        {
                            SpellCD = 180;
                            SpellS = "Ignite";
                            break;
                        }
                    case "summonerexhaust":
                        {
                            SpellCD = 210;
                            SpellS = "Exhaust";
                            break;
                        }
                    case "summonerflash":
                        {
                            SpellCD = 300;
                            SpellS = "Flash";
                            break;
                        }
                    case "summonerhaste"://Ghost
                        {
                            SpellCD = 210;
                            SpellS = "Ghost";
                            break;
                        }
                    case "summonermana"://Clarity
                        {
                            SpellCD = 180;
                            SpellS = "Clarity";
                            break;
                        }
                    case "summonerbarrier":
                        {
                            SpellCD = 210;
                            SpellS = "Barrier";
                            break;
                        }
                    case "summonerteleport":
                        {
                            SpellCD = 300;
                            SpellS = "Teleport";
                            break;
                        }
                    case "summonerboost"://Cleanse
                        {
                            SpellCD = 210;
                            SpellS = "Cleanse";
                            break;
                        }
                    case "summonerclairvoyance"://Clairvoyance
                        {
                            SpellCD = 55;
                            SpellS = "Clairvoyance";
                            break;
                        }
                }

                ClockNow = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds + 3000);
                CDTime = TimeSpan.FromMilliseconds(SpellCD * 1000);
                DownTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds + 3000 + SpellCD * 1000);
                Chat.Print("[SummonersTimer]{0} used {1} in {2:D2}:{3:D2}, down for {4:D2}:{5:D2}, back up at {6:D2}:{7:D2}.", Color1.White, sender.BaseSkinName, SpellS,
                   ClockNow.Minutes, ClockNow.Seconds, CDTime.Minutes, CDTime.Seconds, DownTime.Minutes, DownTime.Seconds);
                if (SummonersMenu["toAll"].Cast<CheckBox>().CurrentValue)
                {
                    //Chat.Say(Message.Replace("#enemyname", sender.BaseSkinName).Replace("#spell", SpellS).Replace("#time", string.Format("{0:D2}:{1:D2}", DownTime.Minutes, DownTime.Seconds)));
                    Messages msg = new Messages();
                    msg.Name = sender.BaseSkinName;
                    msg.Spell = SpellS;
                    msg.Time = string.Format("{0:D2}:{1:D2}", DownTime.Minutes, DownTime.Seconds);
                    MessageList.Add(msg);
                }
            }
        }
    }
    public class MyThreads
    {
        public void MessageChecker()
        {
            while (true)
            {
                if (Program.MessageList.Count > 0)
                {
                    Random rnd = new Random();
                    int delay = rnd.Next(3000, 6000);
                    string ms;
                    Messages msg;
                    msg = Program.MessageList.FirstOrDefault<Messages>();
                    ms = Program.Message.Replace("#enemyname", msg.Name).Replace("#spell", msg.Spell).Replace("#time", msg.Time);
                    Program.MessageList.Remove(msg);
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(delay);
                        Chat.Say(ms);
                    });
                }
                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}

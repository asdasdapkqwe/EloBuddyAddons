using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using EloBuddy.SDK.Rendering;
using System.Diagnostics;
using Color1 = System.Drawing.Color;
using System.Threading;
using System.IO;

namespace SummonersTimer
{
    public class Program
    {
        public static string WorkingPath = System.IO.Path.Combine(EloBuddy.Sandbox.SandboxConfig.DataDirectory, "Addons");
        private static TimeSpan ClockNow, DownTime, CDTime;
        public static DateTime LastAttack;
        public static List<Messages> MessageList;
        public static Stopwatch sw;
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
            sw.Start();
            LastAttack = DateTime.Now;

            if (File.Exists(WorkingPath + "\\MessageST.txt"))
            {
                StreamReader reader = new StreamReader(WorkingPath + "\\MessageST.txt");
                Message = reader.ReadLine();
            }
            else
            {
                File.WriteAllText(WorkingPath + "\\MessageST.txt", Message);
            }

            SummonersMenu = MainMenu.AddMenu("SummonersTimer", "mm");
            SummonersMenu.AddGroupLabel("Summoners Timer");
            SummonersMenu.AddSeparator();
            SummonersMenu.Add("active", new CheckBox("Enabled"));
            SummonersMenu.Add("toAll", new CheckBox("Tell my Team", false));
            SummonersMenu.AddSeparator();
            SummonersMenu.Add("toWait", new Slider("Time after cost spell (Miliseconds)", 4000, 1000, 10000));
            SummonersMenu.AddSeparator();
            SummonersMenu.AddLabel("You can change the message to whatever you want! you can set it by");
            SummonersMenu.AddLabel("chat message starting with \"..say\" and the message.");
            SummonersMenu.AddLabel("Example : ..say #enemyname used the #spell and will works on #time.");
            SummonersMenu.AddSeparator();
            SummonersMenu.AddLabel("#enemyname ==> Enemy Champion name, #spell ==> Summoner Spell,");
            SummonersMenu.AddLabel("#time ==> The time which the spell is ready.");
            SummonersMenu.AddSeparator();
            SummonersMenu.Add("message", new Label("Message Set to : " + Message));
            SummonersMenu.AddSeparator();
            SummonersMenu.AddLabel("Made By GameHackerPM.");

            Chat.OnInput += Chat_OnInput;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
            AIHeroClient.OnSpellCast += AIHeroClient_OnAttack;
            AIHeroClient.OnBasicAttack += AIHeroClient_OnAttack;
        }

        private static void AIHeroClient_OnAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;

            LastAttack = DateTime.Now;
            }

        private static void Chat_OnInput(ChatInputEventArgs args)
        {
            if (!args.Input.StartsWith("..say "))
                return;

            args.Process = false;
            Message = args.Input.Replace("..say ", "");
            SummonersMenu["message"].DisplayName = "Message Set to : " + Message;
            File.WriteAllText(WorkingPath + "\\MessageST.txt", Message);
        }

        private static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!args.SData.Name.ToLower().StartsWith("summoner"))
                return;

            string SpellS = "";
            int SpellCD = 0;
            if (true)
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
                Console.WriteLine("Sent : " + SpellS);
                
            }
        }
        public static bool InTeamFight()
        {
            int enemies = EntityManager.Heroes.Enemies.Where(it => !it.IsMe && !it.IsDead && it.Distance(Player.Instance) <= 700).Count();
            return (enemies >= 3) ? true : false;
            
        }
    }
    public class MyThreads
    {
        static string slangName(string champName)
        {
            switch (champName)
            {
                case "Alistar": return "ali";
                case "Blitzcrank": return "blitz";
                case "Caitlyn": return "cait";
                case "Cassiopeia": return "cass";
                case "Cho'Gath": return "cho";
                case "Dr.Mundo": return "mundo";
                case "Evelynn": return "eve";
                case "Ezreal": return "ez";
                case "Fiddlesticks": return "fiddles";
                case "Gangplank": return "gp";
                case "Hecarim": return "hec";
                case "Heimerdinger": return "heimer";
                case "Jarvan IV": return "j4";
                case "Katarina": return "kat";
                case "Kha'Zix": return "khazix";
                case "Kog'Maw": return "kog";
                case "LeBlanc": return "lb";
                case "LeeSin": return "lee";
                case "Lissandra": return "liss";
                case "Malphite": return "malph";
                case "Malzahar": return "malz";
                case "MasterYi": return "yi";
                case "MissFortune": return "mf";
                case "MonkeyKing": return "wk";
                case "Mordekaiser": return "mord";
                case "Morgana": return "morg";
                case "Nautilus": return "naut";
                case "Nidalee": return "nid";
                case "Nocturne": return "noct";
                case "Orianna": return "ori";
                case "Rek'Sai": return "reksai";
                case "Sejuani": return "sej";
                case "TahmKench": return "tahm";
                case "Tristana": return "trist";
                case "Tryndamere": return "trynd";
                case "TwistedFate": return "tf";
                case "Vel'Koz": return "velkoz";
                case "Vladimir": return "vlad";
                case "Volibear": return "voli";
                case "Warwick": return "ww";
                case "Xin Zhao": return "xin";
                default: return champName.ToLower();
            }
        }
        public void MessageChecker()
        {
            while (true)
            {
                if (Program.MessageList.Count > 0 && !Program.InTeamFight())
                {
                    try
                    {
                        if (new DateTime(DateTime.Now.Ticks - Program.LastAttack.Ticks).Second >= 3)
                        {
                            //Random rnd = new Random();
                            int delay = Program.SummonersMenu["toWait"].Cast<Slider>().CurrentValue;
                            string ms;
                            Messages msg;
                            msg = Program.MessageList.FirstOrDefault<Messages>();
                            ms = Program.Message.Replace("#enemyname", slangName(msg.Name)).Replace("#spell", msg.Spell).Replace("#time", msg.Time);
                            Program.MessageList.Remove(msg);
                            Task.Factory.StartNew(() =>
                            {
                                Thread.Sleep(delay);
                                Chat.Say(ms);
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        //Ignored!
                    }
                }
                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}

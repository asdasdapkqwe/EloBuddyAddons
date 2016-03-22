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

namespace SharedExp
{
    class Program
    {
        public static Text ChampionName = new EloBuddy.SDK.Rendering.Text("", new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 9, System.Drawing.FontStyle.Bold));

        public static Text InvCount = new EloBuddy.SDK.Rendering.Text("", new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 9, System.Drawing.FontStyle.Bold));
        
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static float[] Exp = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] SharingCount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] VisibleCount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] InvisibleCount = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] TimeSharingChange = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] TimeChangedExp = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] TimeUpdateVisible = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int[] TimeMissing = { Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount, Environment.TickCount };
        public static int[] IsNearMe = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int VisibleTotal = 0;
        public static int AliveTotal = 0;
        public static int Invisible = 0;
        public static Color1[] MissingColor = { Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White };
        public static Color1[] VisibleColor = { Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White };

        public static Vector3[] LastMinionPosition = { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
        public static Vector3 MyPos = Vector3.Zero;
        public static float RangedMinonExp = 29.44f;
        public static float MeleeMiniobExp = 58.88f;
        public static float SiegeMinionExp = 92.00f;
        public static Color1[] Cor = { Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White, Color1.White };
        public static Menu SharedExpMenu , ChampionListMenu, DrawMenu; 

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);
            Drawing.OnDraw += Drawing_Settings;
            Game.OnUpdate += Game_OnUpdate;

            SharedExpMenu = MainMenu.AddMenu("SharedExp", "sharedexp");
            SharedExpMenu.AddGroupLabel("SharedExperience");
            SharedExpMenu.AddSeparator();
            SharedExpMenu.Add("active", new CheckBox("Enabled")); 
            SharedExpMenu.AddSeparator();
            SharedExpMenu.AddLabel("Made By GameHackerPM");

            ChampionListMenu = SharedExpMenu.AddSubMenu("ChampList");
            ChampionListMenu.AddGroupLabel("Champion List Settings");
            ChampionListMenu.AddSeparator();
            ChampionListMenu.Add("drawchampionlist", new CheckBox("Draw Champion List"));
            ChampionListMenu.Add("posX", new Slider("Champions List Pos X", Drawing.Width / 2, 0, Drawing.Width));
            ChampionListMenu.Add("posY", new Slider("Champions List Pos Y", Drawing.Height / 2, 0, Drawing.Height));


            DrawMenu = SharedExpMenu.AddSubMenu("Drawings");
            DrawMenu.AddGroupLabel("Drawings Settings");
            DrawMenu.AddSeparator();
            DrawMenu.Add("showEnemies", new CheckBox("Draw Text On Enemy"));
            DrawMenu.Add("onlyShowInv", new CheckBox("Only Draw Text When Not Visible Enemies"));
            DrawMenu.Add("drawPredictionCircle", new CheckBox("Draw Prediction Circle"));
            DrawMenu.Add("positionX", new Slider("OnEnemy Text Position X", 142, -100, 200));
            DrawMenu.Add("positionY", new Slider("OnEnemy Text Position Y", 21, -100, 100));
        }

        public static void Drawing_Settings(EventArgs args)
        {
            int i = -1;
            int c = -1;

            foreach (AIHeroClient hero in ObjectManager.Get<AIHeroClient>())
            {
                i += 1;

                if (hero.IsMe)
                {
                    MyPos = hero.Position;
                    continue;
                }

                if (hero.IsAlly) continue;

                c += 1;

                int x = ChampionListMenu["posX"].Cast<Slider>().CurrentValue;
                int y = ChampionListMenu["posY"].Cast<Slider>().CurrentValue + c * 30;

                if ((Environment.TickCount - TimeChangedExp[i]) < 5000 && InvisibleCount[i] > 0)
                {
                    if (DrawMenu["drawPredictionCircle"].Cast<CheckBox>().CurrentValue) Drawing.DrawCircle(LastMinionPosition[i], 1500, Color1.Red);

                    if (AliveTotal - VisibleTotal == 1)
                    {
                        if (Vector3.Distance(hero.Position, MyPos) <= 3000)
                        {
                            IsNearMe[Invisible] = 1;
                            VisibleColor[Invisible] = Color1.FromArgb(255, 255, 0, 0);
                            MissingColor[Invisible] = Color1.FromArgb(255, 255, 0, 0);
                            TimeMissing[Invisible] = Environment.TickCount;
                        }
                    }
                }

                if (IsNearMe[i] == 1 && !hero.IsDead)
                {
                    if (ChampionListMenu["drawchampionlist"].Cast<CheckBox>().CurrentValue)
                    {
                        Drawing.DrawLine(
                            new Vector2(x - 4.5f, y - 5),
                            new Vector2(x + 85, y - 5), 3, VisibleColor[i]);

                        Drawing.DrawLine(
                            new Vector2(x + 85, y - 4.5f),
                            new Vector2(x + 85, y + 21), 3, VisibleColor[i]);

                        Drawing.DrawLine(
                            new Vector2(x + 85, y + 21),
                            new Vector2(x - 3, y + 21), 3, VisibleColor[i]);

                        Drawing.DrawLine(
                            new Vector2(x - 5, y + 21),
                            new Vector2(x - 5, y - 4.5f), 3, VisibleColor[i]);

                        
                        //Drawing.DrawText(x + 5, y, MissingColor[i], hero.ChampionName);
                        ChampionName.Position = new Vector2(x + 5, y);
                        ChampionName.TextValue = hero.ChampionName;
                        ChampionName.Color = MissingColor[i];
                        ChampionName.Draw();
                    }
                }
                else
                {
                    if (ChampionListMenu["drawchampionlist"].Cast<CheckBox>().CurrentValue)
                    {
                        //Drawing.DrawText(x + 5, y, MissingColor[i], hero.ChampionName);
                        ChampionName.Position = new Vector2(x + 5, y);
                        ChampionName.TextValue = hero.ChampionName;
                        ChampionName.Color = MissingColor[i];
                        ChampionName.Draw();
                    }
                }

                if (!DrawMenu["showEnemies"].Cast<CheckBox>().CurrentValue) continue;

                int textXOffset = DrawMenu["positionX"].Cast<Slider>().CurrentValue;
                int textYOffset = DrawMenu["positionY"].Cast<Slider>().CurrentValue;



                if (SharingCount[i] > 0)
                {
                    if (InvisibleCount[i] > 0)
                    {
                        //Drawing.DrawText(hero.HPBarPosition.X + textXOffset, hero.HPBarPosition.Y + textYOffset, Cor[i], "+" + (SharingCount[i] - 1) + " (" + InvisibleCount[i] + " Inv)");
                        InvCount.Position = new Vector2(hero.HPBarPosition.X + textXOffset, hero.HPBarPosition.Y + textYOffset);
                        InvCount.TextValue = "+" + (SharingCount[i] - 1) + " (" + InvisibleCount[i] + " Inv)";
                        InvCount.Color = Cor[i];
                        InvCount.Draw();
                    }
                    if (!DrawMenu["onlyShowInv"].Cast<CheckBox>().CurrentValue && InvisibleCount[i] == 0)
                    {
                        //Drawing.DrawText(hero.HPBarPosition.X + textXOffset, hero.HPBarPosition.Y + textYOffset, Cor[i], "+" + (SharingCount[i] - 1));
                        InvCount.Position = new Vector2(hero.HPBarPosition.X + textXOffset, hero.HPBarPosition.Y + textYOffset);
                        InvCount.TextValue = "+" + (SharingCount[i] - 1);
                        InvCount.Color = Cor[i];
                        InvCount.Draw();
                    }
                }
            }
        }

        static void Game_OnUpdate(EventArgs args)
        {
            int i = -1;
            float expReceived = 0;

            Invisible = -1;
            VisibleTotal = 0;
            AliveTotal = 0;

            foreach (AIHeroClient hero in ObjectManager.Get<AIHeroClient>())
            {
                i += 1;

                if (hero.IsMe)
                {
                    MyPos = hero.Position;
                }

                if (hero.IsAlly) continue;

                if (hero.IsEnemy && !hero.IsDead)
                {
                    AliveTotal += 1;
                    if (hero.IsVisible)
                    {
                        VisibleTotal += 1;
                    }
                    else
                    {
                        Invisible = i;
                    }

                }

                if (!hero.IsVisible)
                {
                    Exp[i] = (hero.Experience.XP);
                    SharingCount[i] = 0;
                    VisibleCount[i] = 0;
                    InvisibleCount[i] = 0;
                    TimeChangedExp[i] = 0;


                    float t = ((Vector3.Distance(hero.Position, MyPos) * 1000 / hero.MoveSpeed) - (Environment.TickCount - TimeMissing[i]));

                    if (t <= 4000) MissingColor[i] = Color1.FromArgb(255, 255, 80, 0);
                    else if (t <= 6000) MissingColor[i] = Color1.FromArgb(255, 255, 120, 0);
                    else if (t <= 8000) MissingColor[i] = Color1.FromArgb(255, 255, 150, 0);
                    else if (t <= 10000) MissingColor[i] = Color1.FromArgb(255, 255, 180, 0);
                    else if (t <= 12000) MissingColor[i] = Color1.FromArgb(255, 255, 210, 0);
                    else if (t <= 14000) MissingColor[i] = Color1.FromArgb(255, 225, 200, 0);
                    else if (t <= 16000) MissingColor[i] = Color1.FromArgb(255, 200, 190, 0);
                    else if (t <= 18000) MissingColor[i] = Color1.FromArgb(255, 180, 180, 0);
                    else if (t <= 20000) MissingColor[i] = Color1.FromArgb(255, 150, 190, 0);
                    else if (t > 22000) MissingColor[i] = Color1.FromArgb(255, 100, 200, 0);

                    if (Environment.TickCount - TimeMissing[i] >= 5000)
                    {
                        if (IsNearMe[i] == 1 && (Vector3.Distance(hero.Position, MyPos) > 5500)) TimeMissing[i] = Environment.TickCount + 30000;
                        IsNearMe[i] = 0;
                    }

                    if (hero.IsDead)
                    {
                        MissingColor[i] = Color1.FromArgb(255, 200, 200, 200);

                        IsNearMe[i] = 0;
                    }

                    continue;
                }
                else
                {

                    if (Vector3.Distance(hero.Position, MyPos) <= 5500)
                    {
                        IsNearMe[i] = 1;
                    }
                    else if (Vector3.Distance(hero.Position, MyPos) > 5500)
                    {
                        IsNearMe[i] = 0;
                    }

                    float t = (Vector3.Distance(hero.Position, MyPos) * 1000 / hero.MoveSpeed);

                    if (IsNearMe[i] == 1)
                    {

                        if (t <= 4000) MissingColor[i] = Color1.FromArgb(255, 255, 80, 0);
                        else if (t <= 6000) MissingColor[i] = Color1.FromArgb(255, 255, 120, 0);
                        else if (t <= 8000) MissingColor[i] = Color1.FromArgb(255, 255, 150, 0);
                        else if (t <= 10000) MissingColor[i] = Color1.FromArgb(255, 255, 180, 0);
                        else if (t <= 12000) MissingColor[i] = Color1.FromArgb(255, 255, 210, 0);
                        else if (t <= 14000) MissingColor[i] = Color1.FromArgb(255, 225, 200, 0);
                        else if (t <= 16000) MissingColor[i] = Color1.FromArgb(255, 200, 190, 0);
                        else if (t <= 18000) MissingColor[i] = Color1.FromArgb(255, 180, 180, 0);
                        else if (t <= 20000) MissingColor[i] = Color1.FromArgb(255, 150, 190, 0);
                        else if (t > 22000) MissingColor[i] = Color1.FromArgb(255, 100, 200, 0);

                        if ((Environment.TickCount - TimeChangedExp[i]) < 5000 && InvisibleCount[i] > 0)
                        {
                            if (AliveTotal - VisibleTotal == 1)
                            {
                                if (Vector3.Distance(hero.Position, MyPos) <= 3000)
                                {
                                    VisibleColor[Invisible] = Color1.FromArgb(255, 255, 0, 0);
                                    MissingColor[Invisible] = Color1.FromArgb(255, 255, 0, 0);
                                }
                            }
                        }
                    }
                    else
                    {
                        MissingColor[i] = Color1.FromArgb(255, 0, 255, 0);
                    }

                    if (t <= 4000) VisibleColor[i] = Color1.FromArgb(255, 255, 80, 0);
                    else if (t <= 6000) VisibleColor[i] = Color1.FromArgb(255, 255, 120, 0);
                    else if (t <= 8000) VisibleColor[i] = Color1.FromArgb(255, 255, 150, 0);
                    else if (t <= 10000) VisibleColor[i] = Color1.FromArgb(255, 255, 180, 0);
                    else if (t <= 12000) VisibleColor[i] = Color1.FromArgb(255, 255, 210, 0);
                    else if (t <= 14000) VisibleColor[i] = Color1.FromArgb(255, 225, 200, 0);
                    else if (t <= 16000) VisibleColor[i] = Color1.FromArgb(255, 200, 190, 0);
                    else if (t <= 18000) VisibleColor[i] = Color1.FromArgb(255, 180, 180, 0);
                    else if (t <= 20000) VisibleColor[i] = Color1.FromArgb(255, 150, 190, 0);
                    else if (t > 22000) VisibleColor[i] = Color1.FromArgb(255, 100, 200, 0);

                    if (hero.IsDead)
                    {
                        MissingColor[i] = Color1.FromArgb(255, 200, 200, 200);
                    }
                }



                if (hero.IsDead || hero.IsMe || (hero.Level == 18) || hero.IsInvulnerable) continue;

                TimeMissing[i] = Environment.TickCount;

                foreach (Obj_AI_Minion minion in ObjectManager.Get<Obj_AI_Minion>())
                {
                    if (minion.IsAlly && Vector3.Distance(hero.Position, minion.Position) <= 1400)
                    {
                        if (minion.IsDead)
                        {
                            LastMinionPosition[i] = minion.Position;
                            continue;
                        }
                        string MinionName = minion.BaseSkinName;
                    }
                }

                if (Exp[i] != hero.Experience.XP)
                {
                    int rangedMinion = 0;   
                    int siegeMinion = 0;

                    TimeChangedExp[i] = Environment.TickCount;

                    expReceived = (float)Math.Round(hero.Experience.XP - Exp[i], 2);

                    int found = 0;

                    for (int expSharingCount = 1; expSharingCount <= 5; expSharingCount++)
                    {
                        if (expSharingCount == 1)
                        {
                            for (float increasedExp = 1.00f; increasedExp <= 1.14f; increasedExp += 0.02f)
                            {
                                for (rangedMinion = 0; rangedMinion <= 20; rangedMinion += 1)
                                {
                                    if (expReceived == (Math.Round((RangedMinonExp * rangedMinion * increasedExp), 2)))
                                    {
                                        SharingCount[i] = expSharingCount;
                                        TimeSharingChange[i] = Environment.TickCount;
                                        TimeUpdateVisible[i] = 0;
                                        found = 1;
                                        break;
                                    }
                                    for (siegeMinion = 1; siegeMinion <= 3; siegeMinion += 1)
                                    {
                                        if (expReceived == (Math.Round((RangedMinonExp * rangedMinion * increasedExp), 2) + Math.Round((SiegeMinionExp * siegeMinion * increasedExp), 2)))
                                        {
                                            SharingCount[i] = expSharingCount;
                                            TimeSharingChange[i] = Environment.TickCount;
                                            TimeUpdateVisible[i] = 0;
                                            found = 1;
                                            break;
                                        }
                                    }
                                    if (found == 1) break;
                                }
                                if (found == 1) break;
                            }
                        }
                        else
                        {
                            for (float increasedExp = 1.00f; increasedExp <= 1.14f; increasedExp += 0.02f)
                            {
                                for (rangedMinion = 0; rangedMinion <= 27; rangedMinion += 1)
                                {
                                    if (expReceived == (Math.Round(((RangedMinonExp * rangedMinion * 1.30435f / expSharingCount) * increasedExp), 2)))
                                    {
                                        SharingCount[i] = expSharingCount;
                                        TimeSharingChange[i] = Environment.TickCount;
                                        TimeUpdateVisible[i] = 0;
                                        found = 1;
                                        break;
                                    }
                                    for (siegeMinion = 1; siegeMinion <= 3; siegeMinion += 1)
                                    {
                                        if (expReceived == (Math.Round(((RangedMinonExp * rangedMinion * 1.30435f / expSharingCount) * increasedExp), 2) + Math.Round(((SiegeMinionExp * siegeMinion * 1.30435f / expSharingCount) * increasedExp), 2)))
                                        {
                                            SharingCount[i] = expSharingCount;
                                            TimeSharingChange[i] = Environment.TickCount;
                                            TimeUpdateVisible[i] = 0;
                                            found = 1;
                                            break;
                                        }
                                    }
                                    if (found == 1) break;
                                }
                                if (found == 1) break;
                            }
                        }
                        if (found == 1)
                        {
                            break;
                        }
                    }
                    Exp[i] = (hero.Experience.XP);
                }

                int deadCount = 0;
                VisibleCount[i] = 0;
                foreach (AIHeroClient enemy in ObjectManager.Get<AIHeroClient>())
                {
                    if (enemy.IsEnemy && Vector3.Distance(hero.Position, enemy.Position) <= (3000))
                    {
                        if (enemy.IsVisible) VisibleCount[i] += 1;
                        if (enemy.IsDead)
                        {
                            VisibleCount[i] -= 1;
                            deadCount += 1;
                        }
                    }


                }

                if (SharingCount[i] < VisibleCount[i])
                {
                    SharingCount[i] = VisibleCount[i];
                    InvisibleCount[i] = 0;
                    TimeSharingChange[i] = Environment.TickCount;
                }
                else
                {
                    InvisibleCount[i] = (SharingCount[i] - VisibleCount[i] - deadCount);

                    if (deadCount > 0 && InvisibleCount[i] == 0)
                    {
                        SharingCount[i] = VisibleCount[i];
                        TimeSharingChange[i] = Environment.TickCount;
                    }

                    if (InvisibleCount[i] < 0) InvisibleCount[i] = 0;

                }

                TimeUpdateVisible[i] = Environment.TickCount;

                if (InvisibleCount[i] > SharingCount[i] - 1 && SharingCount[i] > 0) InvisibleCount[i] = SharingCount[i] - 1;

                if ((Environment.TickCount - TimeSharingChange[i]) >= 0)
                {
                    Cor[i] = Color1.White;
                }

                if ((Environment.TickCount - TimeSharingChange[i]) >= 20000)
                {
                    SharingCount[i] = VisibleCount[i];
                    TimeSharingChange[i] = Environment.TickCount;
                }

                if (InvisibleCount[i] > 0)
                {
                    if (VisibleTotal == 4)
                    {

                    }
                }
            }
        }
    }
}

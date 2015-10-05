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
    public class GetDamages
    {
        // Nunu'S Q Spell damage.
        public double Nunu(int SpellLevel)
        {
            return new double[] { 400, 550, 700, 850, 1000 }[SpellLevel];
        }

        // Cho'Goth's R Spell damage.
        public double ChoGath()
        {
            return 1000;// Stupid ha? XD
        }

        // Shaco's E Spell damage.
        public double Shaco(int SpellLevel)
        {
            return new double[] { 50, 90, 130, 170, 210 }[SpellLevel]
                + 1 * ObjectManager.Player.FlatPhysicalDamageMod + 1 * ObjectManager.Player.FlatMagicDamageMod;
        }

        // Vi's E Spell damage.
        public double Vi(int SpellLevel)
        {
            return new double[] { 5, 20, 35, 50, 65 }[SpellLevel]
                + 1.15 * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod)
                + 0.7 * ObjectManager.Player.FlatMagicDamageMod;
        }

        //Master's Q Spell damage.
        public double Master(int SpellLevel)
        {
            return new double[] { 25, 60, 95, 130, 165 }[SpellLevel]
                 + 1 * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod)
                 + 0.6 * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod);
        }

        //Rengar's Q Spell damage.
        public double Rengar(int SpellLevel)
        {
            return new double[] { 30, 60, 90, 120, 150 }[SpellLevel]
                 + new double[] { 0, 5, 10, 15, 20 }[SpellLevel] / 100
                 * (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod);
        }

        //Nasus's Q Spell damage.
        public double Nasus(int SpellLevel)
        {
            return (from buff in ObjectManager.Player.Buffs
                    where buff.Name == "nasusqstacks"
                    select buff.Count).FirstOrDefault()
                 + new double[] { 30, 50, 70, 90, 110 }[SpellLevel];
        }

        //Khazix's Q Spell damage.
        public double Khazix(int SpellLevel)
        {
            return new double[] { 70, 95, 120, 145, 170 }[SpellLevel]
                 + 1.2 * ObjectManager.Player.FlatPhysicalDamageMod;
        }

        //Fizz's Q Spell damage.
        public double Fizz(int SpellLevel)
        {
            return new double[] { 10, 25, 40, 55, 70 }[SpellLevel]
                 + 0.35 * ObjectManager.Player.FlatMagicDamageMod;
        }

        //Elise's Q Spell damage.
        public double Elise(int SpellLevel, Obj_AI_Base Monster)
        {
            return new double[] { 40, 75, 110, 145, 180 }[SpellLevel]
                 + (0.08 + 0.03 / 100 * ObjectManager.Player.FlatMagicDamageMod) * Monster.Health;
        }

    }
}

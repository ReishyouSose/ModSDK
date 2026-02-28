
using CoreLib.Data.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CoreFighter.Scripts.Cores
{
    public class AttackSpeedModifier
    {
        private const string Section = "OriginCoolDown";
        public readonly float OriginSpeed;
        public ConfigEntry<bool> Switch;
        public ConfigEntry<float> Tool, Melee, Range, Magic, Throw, OffHand, Consume;
        private const string _Switch = nameof(Switch), _Tool = nameof(Tool), _Melee = nameof(Melee),
                            _Range = nameof(Range), _Magic = nameof(Magic), _Throw = nameof(Throw),
                            _OffHand = nameof(OffHand), _Consume = nameof(Consume);
        public AttackSpeedModifier(float originCD, ConfigFile file)
        {
            OriginSpeed = originCD;
            string section = Section + originCD.ToString("0.0");
            string desc = string.Empty;
            AcceptableValueRange<float> range = new(0.1f, 5f);
            Switch = file.Bind(new(section, _Switch), false, new(desc), new());
            Tool = file.Bind(new(section, _Tool), originCD, new(desc, range), new());
            Melee = file.Bind(new(section, _Melee), originCD, new(desc, range), new());
            Range = file.Bind(new(section, _Range), originCD, new(desc, range), new());
            Magic = file.Bind(new(section, _Magic), originCD, new(desc, range), new());
            Throw = file.Bind(new(section, _Throw), originCD, new(desc, range), new());
            OffHand = file.Bind(new(section, _OffHand), originCD, new(desc, range), new());
            Consume = file.Bind(new(section, _Consume), originCD, new(desc, range), new());
        }
        public ATKSpeedModifer ToStruct()
        {
            return new()
            {
                Switch = Switch.Value,
                Tool = Tool.Value,
                Melee = Melee.Value,
                Range = Range.Value,
                Magic = Magic.Value,
                Throw = Throw.Value,
                OffHand = OffHand.Value,
                Consume = Consume.Value,
            };
        }
    }
}

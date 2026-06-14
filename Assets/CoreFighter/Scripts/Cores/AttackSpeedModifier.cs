using Assets.GeneralConfigMenu.Scripts.ConfigTags;
using CoreLib.Data.Configuration;

namespace Assets.CoreFighter.Scripts.Cores
{
    public class AttackSpeedModifier
    {
        private const string Section = "OriginCoolDown";
        public readonly float OriginSpeed;
        public ConfigEntry<bool> Switch;
        public ConfigEntry<float> Tool, Melee, Range, Magic, Throw, Beam, OffHand, Consume;
        private const string _Switch = nameof(Switch), _Tool = nameof(Tool), _Melee = nameof(Melee),
                            _Range = nameof(Range), _Magic = nameof(Magic), _Throw = nameof(Throw),
                            _Beam = nameof(Beam), _OffHand = nameof(OffHand), _Consume = nameof(Consume);
        public AttackSpeedModifier(string speed, float originCD, ConfigFile file)
        {
            OriginSpeed = originCD;
            string section = Section + speed;
            string desc = string.Empty;
            AcceptableValueRange<float> range = new(0.1f, 5f);
            Switch = file.Bind(new(section, _Switch), false, new(desc, null, GetLocalFix(_Switch)), new());
            Tool = file.Bind(new(section, _Tool), originCD, new(desc, range, GetLocalFix(_Tool)), new());
            Melee = file.Bind(new(section, _Melee), originCD, new(desc, range, GetLocalFix(_Melee)), new());
            Range = file.Bind(new(section, _Range), originCD, new(desc, range, GetLocalFix(_Range)), new());
            Magic = file.Bind(new(section, _Magic), originCD, new(desc, range, GetLocalFix(_Magic)), new());
            Throw = file.Bind(new(section, _Throw), originCD, new(desc, range, GetLocalFix(_Throw)), new());
            Beam = file.Bind(new(section, _Beam), originCD, new(desc, range, GetLocalFix(_Beam)), new());
            OffHand = file.Bind(new(section, _OffHand), originCD, new(desc, range, GetLocalFix(_OffHand)), new());
            Consume = file.Bind(new(section, _Consume), originCD, new(desc, range, GetLocalFix(_Consume)), new());
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
                Beam = Beam.Value,
                OffHand = OffHand.Value,
                Consume = Consume.Value,
            };
        }
        private static LocalizationOverride GetLocalFix(string key) => new() { Key = "CoreFighter_AttackSpeed/" + key };
    }
}

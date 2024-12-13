using CoreLib.Data.Configuration;

namespace Assets.GeneralConfigMenu
{
    public class ModConfig
    {
        public ConfigEntry<bool> testBool;
        public ConfigEntry<int> testInt;
        public ConfigEntry<float> testFloat;
        public ConfigEntry<string> testString;
        public ConfigEntry<TestEnum> testEnum;
        public enum TestEnum
        {
            T1, T2, T3, T4, T5, T6,
        }
        public ModConfig()
        {
            ConfigFile file = new("GeneralConfigMenu/test.cfg", true);
            testBool = file.Bind("General", nameof(testBool), false);
            ConfigDescription desc = new("Just a test.", new AcceptableValueRange<int>(0, 100), "NeedReload");
            testInt = file.Bind("General", nameof(testInt), 0, desc);
            testFloat = file.Bind("General", nameof(testFloat), 0f);
            testString = file.Bind("General", nameof(testString), "empty");
            testEnum = file.Bind("General", nameof(testEnum), TestEnum.T1);
        }
    }
}

namespace Assets.CoreEnhance.Scripts.Configs
{
    public enum EnhanceCategory
    {
        Infinity,
        Accelerate,
        Industry,
        Automation,
    }
    public enum EC_Infinity//无限
    {
        Durability,//耐久 done
        Boulder,//大矿 done
        Arena,//竞技场 done
        Minion,//召唤物时长
    }
    public enum EC_Accelerate//加速
    {
        Merchant,//商人刷新 done
        Titan,//泰坦冷却 done
        Crafting,//所有等待型制作 done
        Casting,//读条物品 
    }
    public enum EC_Industry//工业的加速
    {
        Cook,//烹饪
        Smelting,//冶炼
        Sawmill,//锯木
        Incubator,//孵化
    }
    public enum EC_Automation//自动化 
    {
        Salvage,//拆解 done
    }

}

namespace Assets.CoreEnhance.Scripts.Configs
{
    public enum EnhanceCategory
    {
        Infinity,
        Accelerate,
        Automation,
        Misc
    }
    public enum EC_Infinity//无限
    {
        Durability,//耐久 done
        Boulder,//大矿 done
        Arena,//竞技场 done
        Minion,//召唤物时长 done
    }
    public enum EC_Accelerate//加速
    {
        Merchant,//商人刷新 done
        Titan,//泰坦冷却 done
        Crafting,//所有等待型制作 done
        Casting,//读条物品 done
        Portal,//传送点 done
    }
    public enum EC_Automation//自动化 
    {
        Salvage,//拆解 done
        Plant,//拿来控制神龛范围内是否自动收吧
    }
    public enum EC_Misc//杂项
    {
        QuickStack,//快速堆叠到附近箱子 done
        DeathNoDrop,//死亡不掉落 done
    }
}

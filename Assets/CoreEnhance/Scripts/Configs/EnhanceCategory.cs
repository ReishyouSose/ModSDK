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
        Plant,//神龛范围内是否自动收
        Fish,//钓鱼机是否要求匹配地块与环境
        Door,//自动门 done
        GiveExp,//自动化给予经验 done
    }
    public enum EC_Misc//杂项
    {
        DeathNoDrop,//死亡不掉落 done
        ChainMining,//连锁挖矿 done
        ContainerDisplay,//容器显示 done
    }
}

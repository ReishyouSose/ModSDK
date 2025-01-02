using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CoreEnhance.Scripts.Configs
{
    public enum EnhanceCategory
    {
        Infinity,
        Accelerate,
        Industry,
        Automation,
    }
    public enum EC_Infinity//无限 done
    {
        Durability,//耐久 done
        Boulder,//大矿 done
        Arena,//竞技场 done
    }
    public enum EC_Accelerate//加速
    {
        Merchant,//商人刷新 done
        Titan,//泰坦冷却
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
        Salvage,//拆解
    }

}

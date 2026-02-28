using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.CoreFighter.Scripts.Cores
{
    public enum FighterCategory
    {
        _Infinity,
        Mana,//魔力 done
        Hunger,
        Explosive,//无限炸药（不消耗）
        _Equip,
        NoRecoil,//无后坐力 done
        _Misc,
        //AllPlayerSkill,//玩家全技能 done
        //AllPetSkill,//宠物全技能 done
        //ImmuneExplosion,//拦截爆炸伤害 done
        MapMarkerTeleport,//标记传送
        Vampire,
    }
}

using Assets.THCompass.DataStruct;
using PugConversion;
using Unity.Entities;
using UnityEngine;

namespace Assets.THCompass.Component
{
    public class DropFromBossAuthoring : MonoBehaviour
    {
        public BossID bossID;
    }
    public struct DropFromBossCD : IComponentData
    {
        public BossID bossID;
    }
    public class DropFromBossConverter : SingleAuthoringComponentConverter<DropFromBossAuthoring>
    {
        protected override void Convert(DropFromBossAuthoring authoring)
        {
            BossID boss = authoring.bossID;
            AddComponentData(new DropFromBossCD()
            {
                bossID = boss,
            });
        }
    }
}

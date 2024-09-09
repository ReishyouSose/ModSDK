using CoreLib.Data.Configuration;
using CoreLib.Util.Extensions;
using PugMod;
using UnityEngine;

public class SledgeEnhance : IMod
{
    internal static ConfigEntry<float> range, speed;
    public void EarlyInit()
    {
        SetConfig();
        //API.Authoring.OnObjectTypeAdded += Authoring_OnObjectTypeAdded;
    }
    private void SetConfig()
    {
        ConfigFile config = new("SledgeEnhance/config.cfg", true);
        range = config.Bind("General", "SledgeRange", 2.5f,
             "大锤的攻击范围。\r\n原版是1.4（能攻击3格）。\r\n默认为2.5（能攻击5格）。\r\n不会低于1.4.\r\nThe attack range of the sledge hammer.\r\nThe original version is 1.4 (can attack 3 blocks).\r\nThe default is 2.5 (can attack 5 blocks).\r\nWill not be lower than 1.4.");
        speed = config.Bind("General", "SledgeAttackSpeed", 0.4f,
            "大锤攻击速度。\r\n原版是0.7（1.4次/秒）。\r\n默认是0.4（2.5次/秒）\r\n不会高于0.7.\r\n公式为  攻速 = 1 / 数值。\r\nSledge hammer attack speed.\r\nThe original version is 0.7 (1.4 times/second).\r\nThe default is 0.4 (2.5 times/second).\r\nIt will not be higher than 0.7.\r\nThe formula is attack speed = 1 / value.");
    }
    private void Authoring_OnObjectTypeAdded(Unity.Entities.Entity entity, GameObject authoringData, Unity.Entities.EntityManager entityManager)
    {
        ObjectID id = authoringData.GetEntityObjectID();
        if (id == ObjectID.TinSledge)
        {
            Debug.Log(id);
            var cs = authoringData.GetComponentCount();
            /*for (int i = 0; i < cs; i++)
            {
                var a = authoringData.GetComponentAtIndex(i);
                Debug.Log(a);
            }*/
            if (authoringData.TryGetComponent<GivesConditionsWhenEquippedAuthoring>(out var equip))
            {
                foreach (var condition in equip.givesConditionsWhenEquipped)
                {
                    Debug.Log(condition);
                }
            }
            /*if (authoringData.TryGetComponent<WeaponAuthoring>(out var weaponAuthoringData))
            {
                Debug.Log("tileAOE: " + weaponAuthoringData.tileDamageAOE);
            }
            if (authoringData.TryGetComponent<DurabilityAuthoring>(out var durable))
            {
                Debug.Log("durableMax: " + durable.maxDurability);
                Debug.Log("durableMult: " + durable.durabilityMultiplier);
            }
            if (authoringData.TryGetComponent<DamageObjectStateAuthoring>(out var damageObjectStateAuthoringData))
            {
                Debug.Log(damageObjectStateAuthoringData.damage);
            }*/
        }
    }

    public void Init()
    {
    }

    public void ModObjectLoaded(Object obj)
    {
    }

    public void Shutdown()
    {
    }

    public void Update()
    {
    }
}

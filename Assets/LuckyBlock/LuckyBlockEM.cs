using PugMod;

namespace Assets.LuckyBlock
{
    public class LuckyBlockEM : EntityMonoBehaviour
    {
        public void Use()
        {
            API.Server.World.EntityManager.SetComponentData(entity, new HealthCD() { maxHealth = 1, health = 0 });
        }
    }
}

namespace Assets.LuckyBlock
{
    public class LuckyBlockEM : EntityMonoBehaviour
    {
        public void Use()
        {
            LuckyBlockClient.TriggerLuckyBlock(entity);
        }
    }
}

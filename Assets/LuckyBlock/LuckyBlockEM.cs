namespace Assets.LuckyBlock
{
    public class LuckyBlockEM : EntityMonoBehaviour
    {
        public void Use()
        {
            TriggerLBClient.Trigger(entity);
        }
    }
}

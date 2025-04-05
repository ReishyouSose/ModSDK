namespace Assets.CoreEnhance.Scripts.Tiles
{
    public class AutoFisherEM : Chest
    {
        public override void OnOccupied()
        {
            int variation = DirectionBasedOnVariationCD.GetVariationFromDirection(direction.RoundToInt2());
            int index = (variation + 2) % 4;
            foreach (var sprite in spriteObjects)
            {
                sprite.ApplyVisualChange();
                sprite.SetVariantByIndex(index);
            }
            base.OnOccupied();
        }
    }
}

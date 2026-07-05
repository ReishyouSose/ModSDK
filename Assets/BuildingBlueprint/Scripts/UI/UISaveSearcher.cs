namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class UISaveSearcher : TextInputField
    {
        public override TextAndFormatFields GetHoverTitle()
        {
            return new()
            {
                text = "BuildingBlueprint/SearchDesc"
            };
        }
    }
}

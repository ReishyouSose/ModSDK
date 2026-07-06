using Assets.BuildingBlueprint.Scripts.Core;
using System.Collections.Generic;
using static PugDatabase;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class UIBuildingName : UIInputBox, IOverrideHoverMaterialVariation
    {
        private UIBuildingInfo info;
        private List<int> variations;
        private void Start()
        {
            info = GetComponentInParent<UIBuildingInfo>();
        }
        public override List<MaterialInfo> GetRequiredMaterials(bool isRepairing, bool isReinforcing)
        {
            return info.Info.GetMaterails(out variations);
        }
        public override List<TextAndFormatFields> GetHoverDescription()
        {
            return new()
            {
                new()
                {
                   text = "BuildingBlueprint/MaterialTipDesc",
                },
                new()
                {
                    text = info.Info.Description,
                    dontLocalize = true
                }
            };
        }
        public override bool ShowRequiredMaterialsAmountNumberColor() => true;

        public int GetOverrideVariation(int index) => variations[index];
    }
}

using Assets.BuildingBlueprint.Scripts.Core;
using System.Collections.Generic;
using static PugDatabase;

namespace Assets.BuildingBlueprint.Scripts.UI
{
    public class UIBuildingName : TextInputField, IOverrideHoverMaterialVariation
    {
        private UIBuildingInfo info;
        private List<int> variations;
        private void Start()
        {
            info = GetComponentInParent<UIBuildingInfo>();
        }
        public override List<MaterialInfo> GetRequiredMaterials(bool isRepairing, bool isReinforcing)
        {
            return info.GetMaterails(out variations);
        }
        public override bool ShowRequiredMaterialsAmountNumberColor() => true;

        public int GetOverrideVariation(int index) => variations[index];
    }
}

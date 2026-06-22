using System.Collections.Generic;
using static PugDatabase;

namespace Assets.BuildingBlueprint.Scripts
{
    public class UIBuildingName : TextInputField
    {
        private UIBuildingInfo info;
        private void Start()
        {
            info = GetComponentInParent<UIBuildingInfo>();
        }
        public override List<MaterialInfo> GetRequiredMaterials(bool isRepairing, bool isReinforcing)
        {
            return info.GetMaterails();
        }
        public override bool ShowRequiredMaterialsAmountNumberColor() => true;
    }
}

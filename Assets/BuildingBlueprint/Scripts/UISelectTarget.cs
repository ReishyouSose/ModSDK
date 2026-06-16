using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public class UISelectTarget : ButtonUIElement
    {
        public GameObject Mark;
        public SelectTarget Target;
        public bool State;
        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            State = !State;
            Mark.SetActive(State);
            base.OnLeftClicked(mod1, mod2);
        }
    }
}

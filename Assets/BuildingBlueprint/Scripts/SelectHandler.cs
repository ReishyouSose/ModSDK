using UnityEngine;

namespace Assets.BuildingBlueprint.Scripts
{
    public class SelectHandler : MonoBehaviour
    {
        public Transform ModeMark;
        public Transform BoxMark;
        public Transform ClickMark;
        public GameObject BoxContainer;
        public GameObject ClickContainer;

        [HideInInspector]
        public SelectionMode Mode;
        [HideInInspector]
        public BoxOperator BoxOp;
        [HideInInspector]
        public ClickOperator ClickOp;
        [HideInInspector]
        public SelectTarget Target;
        private void Awake()
        {
            ClickContainer.SetActive(false);
            Target |= SelectTarget.Entity;
        }
        public void ChangeSelectionMode(GameObject button)
        {
            ModeMark.position = button.transform.position;
            var mode = button.GetComponent<UISelectionMode>().Mode;
            Mode = mode;
            switch (mode)
            {
                case SelectionMode.Box:
                    BoxContainer.SetActive(true);
                    ClickContainer.SetActive(false);
                    break;
                case SelectionMode.Click:
                    ClickContainer.SetActive(true);
                    BoxContainer.SetActive(false);
                    break;
                default:
                    ClickContainer.SetActive(false);
                    BoxContainer.SetActive(false);
                    break;
            }
            BuildingSelectSystem.Ins.SetSelecing(Mode != SelectionMode.None);
        }

        public void ChangeBoxOp(GameObject button)
        {
            BoxMark.position = button.transform.position;
            BoxOp = button.GetComponent<UIBoxOperator>().Op;
        }

        public void ChangeClickOp(GameObject button)
        {
            ClickMark.position = button.transform.position;
            ClickOp = button.GetComponent<UIClickOperator>().Op;
        }

        public void ChangeTarget(UISelectTarget target)
        {
            var t = target.Target;
            if (target.State)
                Target |= t;
            else
                Target &= ~t;
        }
    }
}

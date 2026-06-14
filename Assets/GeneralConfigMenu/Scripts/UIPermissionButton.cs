using System.Collections;
using UnityEngine;

namespace Assets.GeneralConfigMenu.Scripts
{
    public class UIPermissionButton : ButtonUIElement
    {
        public SpriteRenderer SR;
        private readonly static WaitForSeconds _waitForSeconds0_1 = new(0.1f);
        private readonly static Color Default = new(1, 1, 1, 150 / 255f);
        public void ShowUnEditableWarning()
        {
            StartCoroutine(UnEditableWarning());
        }
        private IEnumerator UnEditableWarning()
        {
            for (int i = 0; i < 3; i++)
            {
                SR.color = Color.red;
                yield return _waitForSeconds0_1;
                SR.color = Default;
                yield return _waitForSeconds0_1;
            }
        }
    }
}

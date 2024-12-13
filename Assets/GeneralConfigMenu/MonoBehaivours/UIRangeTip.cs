using CoreLib.Data.Configuration;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.GeneralConfigMenu.MonoBehaivours
{
    public class UIRangeTip : MonoBehaviour
    {
        public PugText Min;
        public PugText Max;
        private const string Match = @"From (?<min>.+?) to (?<max>.+)";
        public void TryRange(AcceptableValueBase range)
        {
            if (range == null)
            {
                SetShow(false);
                return;
            }

            var match = Regex.Match(range.ToDescriptionString(), Match);
            if (match.Success)
            {
                Min.Render("Min: " + match.Groups["min"].Value.Trim());
                Max.Render("Max: " + match.Groups["max"].Value.Trim());
                SetShow(true);
                return;
            }
            SetShow(false);
        }

        public void SetShow(bool show)
        {
            Min.gameObject.SetActive(show);
            Max.gameObject.SetActive(show);
        }
    }
}

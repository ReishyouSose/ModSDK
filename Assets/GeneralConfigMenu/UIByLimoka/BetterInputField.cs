using System;
using UnityEngine;

namespace Assets.GeneralConfigMenu.UIByLimoka
{
    public class BetterInputField : UIelement, InputManager.TextInputInterface
    {
        public Action<GameObject, string> onTextChanged;

        public string hintString;

        public float maxWidth;
        public float maxHeight;

        public bool dontAllowNewLines;
        public bool trim = true;
        public string characterWhiteList = "";

        public bool triggerOnInputFieldDoneWhenCanceling;
        public bool dontDeactivateOnDeselect;

        public PugText pugText;
        public PugText hintText;

        public SpriteRenderer background;
        public SpriteRenderer selectedMarker;

        public CharacterMarkBlinker characterMarkBlinker;

        private int currentCharIndex;

        public bool WasAutoActivated
        {
            get => wasAutoActivated;
            set => wasAutoActivated = value;
        }

        public int MaxCharactersForOnScreenKeyboard => 255;

        protected virtual string ValidateInput(string text)
        {
            return text;
        }

        public bool inputIsActive { get; private set; }

        protected void Awake()
        {
            if (characterMarkBlinker != null)
                characterMarkBlinker.gameObject.SetActive(false);
            if (selectedMarker != null)
                selectedMarker.gameObject.SetActive(false);

            if (pugText != null)
                pugText.maxWidth = maxWidth;
        }

        private void OnValidate()
        {
            if (pugText != null)
            {
                pugText.maxWidth = maxWidth;
                pugText.transform.localPosition = new Vector3(-maxWidth / 2f, 0, 0);

            }

            if (hintText != null)
            {
                hintText.maxWidth = maxWidth;
                hintText.transform.localPosition = new Vector3(-maxWidth / 2f, 0, 0);
            }

            if (background != null)
                background.size = new Vector2(maxWidth + 0.375f, maxHeight);

            if (selectedMarker != null)
                selectedMarker.size = new Vector2(maxWidth + 0.375f, maxHeight);

            var collider = GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.size = new Vector3(maxWidth, maxHeight, 1);
            }
        }

        protected void Update()
        {
            UpdateHintText();

            Vector2 vector = new Vector2(pugText.transform.position.x, pugText.transform.position.y);

            float num;

            if (pugText.dimensions.height > 0f)
                num = pugText.dimensions.height / (pugText.displayedTextStringLinesAmount * 2f);
            else
                num = 0f;

            int num2 = currentCharIndex;

            Vector2 vector2 = vector + new Vector2(pugText.dimensions.min.x, pugText.dimensions.max.y) + new Vector2(0.03125f, -num);
            vector2 += ((num2 > 0 && num2 <= pugText.localCharacterEndPositions.Count) ? pugText.localCharacterEndPositions[num2 - 1] : Vector2.zero);

            characterMarkBlinker.transform.position = new Vector3(vector2.x, vector2.y, characterMarkBlinker.transform.position.z);
            pugText.Render();

            while (pugText.displayedTextString.Length > 0 && ((maxWidth > 0f && pugText.dimensions.width > maxWidth) ||
                                                                   (maxHeight > 0f && pugText.dimensions.height > maxHeight)))
            {
                pugText.textString = pugText.displayedTextString.Substring(0, pugText.displayedTextString.Length - 1);
                currentCharIndex--;
                pugText.Render(false);
            }

            currentCharIndex = Mathf.Clamp(currentCharIndex, 0, pugText.displayedTextString.Length);
        }

        private void UpdateHintText()
        {
            if (pugText.textString == "" && hintText.textString == "")
            {
                hintText.Render(hintString);
                return;
            }

            if (pugText.textString != "" && hintText.textString != "")
            {
                hintText.Render("");
            }
        }

        public void AppendString(string s)
        {
            if (trim)
            {
                s = s.Trim();
            }

            for (int i = s.Length - 1; i >= 0; i--)
            {
                if (dontAllowNewLines && (s[i] == '\n' || s[i] == '\r'))
                {
                    s = s.Remove(i, 1);
                }
                else
                {
                    int num = 0;
                    while (num < characterWhiteList.Length && s[i] != characterWhiteList[num])
                    {
                        num++;
                    }

                    if (characterWhiteList.Length > 0 && num == characterWhiteList.Length)
                    {
                        s = s.Remove(i, 1);
                    }
                }
            }

            string displayedTextString = pugText.displayedTextString;
            if (currentCharIndex > pugText.displayedTextString.Length)
            {
                Debug.LogError("currentCharIndex > pugText.displayedTextString.Length");
                currentCharIndex = pugText.displayedTextString.Length;
            }

            if (currentCharIndex == pugText.displayedTextString.Length)
            {
                pugText.textString = pugText.displayedTextString + s;
            }
            else
            {
                pugText.textString = pugText.displayedTextString.Insert(currentCharIndex, s);
            }

            bool flag = currentCharIndex == pugText.displayedTextString.Length;
            currentCharIndex += s.Length;
            pugText.Render(false);
            if (flag)
            {
                currentCharIndex = pugText.displayedTextString.Length;
            }

            if ((maxWidth > 0f && pugText.dimensions.width > maxWidth) ||
                (maxHeight > 0f && pugText.dimensions.height > maxHeight))
            {
                pugText.textString = displayedTextString;
                currentCharIndex -= s.Length;
                pugText.Render(false);
            }
        }

        public void MoveCharMarker(int relativeChange)
        {
            currentCharIndex += relativeChange;
            currentCharIndex = Mathf.Clamp(currentCharIndex, 0, pugText.displayedTextString.Length);
        }

        public string GetHintString()
        {
            return hintText.ProcessText(hintString);
        }

        public bool IsHidden()
        {
            return pugText.isHidden;
        }

        public void RemoveCharAtMarker()
        {
            if (pugText.displayedTextString.Length > currentCharIndex)
            {
                pugText.textString = pugText.displayedTextString.Remove(currentCharIndex, 1);
                pugText.Render(false);
            }
        }

        public void RemoveCharBehindMarker()
        {
            if (currentCharIndex > 0 && pugText.displayedTextString.Length >= currentCharIndex)
            {
                pugText.textString = pugText.displayedTextString.Remove(currentCharIndex - 1, 1);
                currentCharIndex--;
                pugText.Render(false);
            }
        }

        public override void OnSelected()
        {
            base.OnSelected();
            selectedMarker.gameObject.SetActive(true);
        }

        public override void OnDeselected(bool playEffect = true)
        {
            base.OnDeselected(playEffect);
            selectedMarker.gameObject.SetActive(false);
            if (!dontDeactivateOnDeselect)
            {
                Deactivate(false);
            }
        }

        public override void OnLeftClicked(bool mod1, bool mod2)
        {
            Manager.input.SetActiveInputField(this);
            Manager.input.DisableInput();
            characterMarkBlinker.EnableAndResetBlink();
            inputIsActive = true;
        }

        public void ResetText()
        {
            SetInputText("");
        }

        public string GetInputText()
        {
            return pugText.textString;
        }

        public void SetInputText(string text)
        {
            pugText.textString = ValidateInput(text);
            pugText.Render(false);
            currentCharIndex = text.Length;
            UpdateHintText();
        }

        public void Deactivate(bool commit)
        {
            Manager.input.SetActiveInputField(null);
            Manager.input.EnableInput();
            characterMarkBlinker.gameObject.SetActive(false);
            if (commit || triggerOnInputFieldDoneWhenCanceling)
            {
                pugText.textString = ValidateInput(pugText.textString);
                OnCommit();
                pugText.Render();
            }

            inputIsActive = false;
        }

        protected virtual void OnCommit()
        {
            onTextChanged?.Invoke(gameObject, pugText.textString);
        }
    }
}
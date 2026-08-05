using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Combat
{
    public class HeroTabUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text heroNameText;
        [SerializeField] private Image background;

        [Header("Visuals")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(0.82f, 0.82f, 0.82f, 1f);

        [Header("Text Layout")]
        [SerializeField] private bool stackLettersVertically = true;

        private int heroIndex;
        private Action<int> onClicked;

        public void Bind(int index, string heroName, Action<int> clickHandler)
        {
            heroIndex = index;
            onClicked = clickHandler;

            SetName(heroName);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClicked);
            }
        }

        public void SetName(string heroName)
        {
            if (heroNameText == null)
                return;

            string safeName = string.IsNullOrWhiteSpace(heroName) ? "Hero" : heroName;
            heroNameText.text = stackLettersVertically ? BuildVerticalName(safeName) : safeName;
        }

        public void SetSelected(bool isSelected)
        {
            if (background != null)
                background.color = isSelected ? selectedColor : normalColor;
        }

        private string BuildVerticalName(string value)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                sb.Append(value[i]);

                if (i < value.Length - 1)
                    sb.Append('\n');
            }

            return sb.ToString();
        }

        private void HandleClicked()
        {
            onClicked?.Invoke(heroIndex);
        }
    }
}
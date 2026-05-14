using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceMining
{
    public class RewardTrackerUI : MonoBehaviour
    {
        public Image chestIcon;
        public Image keyIcon;
        public Image modifierIcon;
        public TMP_Text modifierLabel;
        public Color tintedColor = new Color(0.25f, 0.25f, 0.25f, 0.6f);
        public Color foundColor = Color.white;

        void Awake()
        {
            if (chestIcon != null) chestIcon.color = tintedColor;
            if (keyIcon != null) keyIcon.color = tintedColor;
        }

        public void MarkChestFound()
        {
            if (chestIcon != null) chestIcon.color = foundColor;
        }

        public void MarkKeyFound()
        {
            if (keyIcon != null) keyIcon.color = foundColor;
        }

        public void ShowModifier(Sprite bookSprite, string label)
        {
            if (modifierIcon != null)
            {
                modifierIcon.sprite = bookSprite;
                modifierIcon.gameObject.SetActive(true);
            }
            if (modifierLabel != null)
                modifierLabel.text = label;
        }
    }
}

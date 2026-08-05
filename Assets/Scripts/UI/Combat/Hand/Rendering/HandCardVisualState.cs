using UnityEngine;

namespace UI.Combat
{
    public class HandCardVisualState : MonoBehaviour
    {
        public int OriginalSiblingIndex { get; private set; }

        public void SetOriginalSiblingIndex(int index)
        {
            OriginalSiblingIndex = index;
        }
    }
}
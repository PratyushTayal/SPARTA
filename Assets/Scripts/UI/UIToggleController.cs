using UnityEngine;
using System.Collections.Generic;

namespace OrbitGuard.UI
{
    public class UIToggleController : MonoBehaviour
    {
        public List<GameObject> uiRoots = new List<GameObject>();
        public bool startVisible = true;
        public bool IsVisible { get; private set; }

        private void Start()
        {
            IsVisible = startVisible;
            ApplyVisibility();
        }

        public void Toggle()
        {
            IsVisible = !IsVisible;
            ApplyVisibility();
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            foreach (var root in uiRoots)
            {
                if (root != null) root.SetActive(IsVisible);
            }
        }
    }
}
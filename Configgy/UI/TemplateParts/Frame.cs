using UnityEngine;
using UnityEngine.UI;

namespace Configgy.UI.Template
{
    public class Frame : MonoBehaviour
    {
        [SerializeField] private Image border;
        [SerializeField] private Image background;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform content;

        public RectTransform Content => content;
        public RectTransform RectTransform => rectTransform;

        public void SetBorderColor(Color color)
        {
            border.color = color;
        }

        public void SetBackgroundColor(Color color) 
        {
            background.color = color;
        }
    }
}

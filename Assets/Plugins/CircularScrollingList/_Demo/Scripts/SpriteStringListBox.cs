using UnityEngine;
using UnityEngine.UI;

namespace AirFishLab.ScrollingList.Demo
{
    public class SpriteStringListBox : ListBox
    {
        [SerializeField]
        private Image _image;
        [SerializeField]
        private TMPro.TMP_Text _title;

        protected override void UpdateDisplayContent(object content)
        {
            var data = (SpriteStringData)content;
            _image.sprite = data.sprite;
            if (_title != null) _title.text = data.title;
        }
    }
}

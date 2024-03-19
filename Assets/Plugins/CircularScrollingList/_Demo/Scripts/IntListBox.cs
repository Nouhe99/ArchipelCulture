using UnityEngine;

namespace AirFishLab.ScrollingList.Demo
{
    public class IntListBox : ListBox
    {
        [SerializeField]
        private TMPro.TMP_Text _contentText;

        protected override void UpdateDisplayContent(object content)
        {
            if (_contentText != null) _contentText.text = ((int)content).ToString();
        }
    }
}

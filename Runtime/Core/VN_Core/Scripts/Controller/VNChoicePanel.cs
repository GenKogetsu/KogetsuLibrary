using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genoverrei.Library.Core
{
    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : แผงควบคุมปุ่มตัวเลือก VN ทำหน้าที่สร้าง/ลบปุ่มตัวเลือกและส่งคำตอบที่เลือกกลับไปยัง VNSceneReader</para>
    /// <para>(EN) : VN choice button panel. Spawns and removes choice buttons, then forwards the selected answer to VNSceneReader.</para>
    /// </summary>
    public class VNChoicePanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _buttonContainer;
        [SerializeField] private VNChoiceButton _buttonPrefab;

        [Header("Animation (optional)")]
        [SerializeField] private Animator _panelAnimator;
        [SerializeField] private AnimationClip _showClip;
        [SerializeField] private AnimationClip _hideClip;

        private readonly List<VNChoiceButton> _activeButtons = new();
        private Action<int> _onSelected;

        /// <summary>
        /// <para>(TH) : เปิดแผงและสร้างปุ่มตัวเลือกจากรายการ choices</para>
        /// <para>(EN) : Activates the panel and instantiates a button for each choice.</para>
        /// </summary>
        public void ShowChoices(List<VNChoiceAnswer> choices, Action<int> onSelected)
        {
            _onSelected = onSelected;
            ClearButtons();
            gameObject.SetActive(true);

            if (_panelAnimator != null && _showClip != null)
                _panelAnimator.Play(_showClip.name);

            foreach (var choice in choices)
            {
                var btn = Instantiate(_buttonPrefab, _buttonContainer);
                btn.Setup(choice, OnButtonClicked);
                _activeButtons.Add(btn);
            }
        }

        /// <summary>
        /// <para>(TH) : ปิดแผงและล้างปุ่มทั้งหมด</para>
        /// <para>(EN) : Hides the panel and destroys all spawned buttons.</para>
        /// </summary>
        public void HideChoices()
        {
            if (_panelAnimator != null && _hideClip != null)
                _panelAnimator.Play(_hideClip.name);

            ClearButtons();
            gameObject.SetActive(false);
        }

        private void OnButtonClicked(int answerNumber) => _onSelected?.Invoke(answerNumber);

        private void ClearButtons()
        {
            foreach (var btn in _activeButtons)
                if (btn != null) Destroy(btn.gameObject);
            _activeButtons.Clear();
        }
    }
}

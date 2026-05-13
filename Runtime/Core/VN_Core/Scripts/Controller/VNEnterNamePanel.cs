using System;
using TMPro;
using UnityEngine.UI;
using NaughtyAttributes;

namespace Genoverrei.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : Panel กลางหน้าจอสำหรับกรอกชื่อผู้เล่น — เปิดผ่าน VNSceneReader เมื่อ phase มี UseEnterName </para>
    /// <para> (EN) : Center-screen panel for player name input. Opened by VNSceneReader when a phase has UseEnterName. </para>
    /// </summary>
    public class VNEnterNamePanel : MonoBehaviour
    {
        [Header("UI References")]
        [Required]
        [SerializeField] private TMP_InputField _inputField;

        [Required]
        [SerializeField] private Button _confirmButton;

        [Header("Data")]
        [Required]
        [SerializeField] private VNPlayerDataSO _playerData;

        private Action _onConfirmed;

        private void Start()
        {
            gameObject.SetActive(false);
            _confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// <para> (TH) : เปิด panel พร้อมโหลดชื่อปัจจุบัน — callback จะถูกเรียกเมื่อผู้เล่นกด Confirm </para>
        /// <para> (EN) : Shows the panel with the current name pre-filled. Callback fires when player confirms. </para>
        /// </summary>
        public void Show(Action onConfirmed)
        {
            _onConfirmed        = onConfirmed;
            _inputField.text    = _playerData.PlayerName;
            gameObject.SetActive(true);
            _inputField.ActivateInputField();
            _inputField.Select();
        }

        /// <summary>
        /// <para> (TH) : ซ่อน panel </para>
        /// <para> (EN) : Hides the panel. </para>
        /// </summary>
        public void Hide() => gameObject.SetActive(false);

        // ── Internal ────────────────────────────────────────────────────────────

        private void OnConfirmClicked()
        {
            var name = _inputField.text.Trim();
            if (string.IsNullOrEmpty(name)) return; // ไม่ยอมให้ชื่อว่าง

            _playerData.SetName(name);
            Hide();

            var callback = _onConfirmed;
            _onConfirmed = null;
            callback?.Invoke();
        }
    }
}

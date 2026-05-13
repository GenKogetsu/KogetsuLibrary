using System;

namespace Genoverrei.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ข้อมูลของตัวละครและรายการคำสั่งทั้งหมดในเฟสนั้น </para>
    /// <para> (EN) : Character data and their list of action commands for the phase. </para>
    /// </summary>
    [Serializable]
    public class VNSpeakerData
    {
        [Required]
        public VNCharacterSO Character;

        public VNNameDisplayMode NameDisplayMode = VNNameDisplayMode.Text;

        /// <summary>
        /// <para> (TH) : ใช้เมื่อ NameDisplayMode = None — แสดง icon นี้แทนชื่อ, ถ้า null จะ fallback เป็น Text </para>
        /// <para> (EN) : Used when NameDisplayMode = None. Shows this icon instead of name; falls back to Text if null. </para>
        /// </summary>
        public Sprite NameIconSprite;

        public List<VNAction> Actions = new();
    }
}
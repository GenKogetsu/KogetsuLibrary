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

        public bool ShowName = true;
        public List<VNAction> Actions;
    }
}
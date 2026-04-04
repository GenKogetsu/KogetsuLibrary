namespace Genoverrei.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ไฟล์ข้อมูลบทสนทนาที่เก็บรวบรวมโหนดเนื้อเรื่องทั้งหมดในบทนั้น </para>
    /// <para> (EN) : Conversation data file storing all story nodes for the chapter. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "Chapter_01", menuName = "GenoverreiLibrary/Core/VN/Chapter")]
    public class VNChapterSO : ScriptableObject
    {
        public string ChapterName;
        public ushort Index;

        public List<VNConversationNode> Conversations = new();
    }
}
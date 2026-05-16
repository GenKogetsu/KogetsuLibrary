#if UNITY_EDITOR

namespace Genoverrei.Library.Assistant
{
    [CreateAssetMenu(fileName = "TagSettings", menuName = "GenoverreiLibrary/Assistant/Tag Settings")]
    public class TagSettingsSO : ScriptableObject
    {
        [Header("Settings")]
        public List<string> RequiredTags = new()
        {
           "SFX_Channel",
            "BMG_Channel",
            "Voiceover_Channel"
        };
    }
}
#endif
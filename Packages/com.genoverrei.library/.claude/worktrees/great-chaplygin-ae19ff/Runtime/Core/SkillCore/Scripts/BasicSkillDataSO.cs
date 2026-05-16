namespace Genoverrei.Library.Core
{
    [CreateAssetMenu(fileName = "BasicSkillData", menuName = "GenoverreiLibrary/Core/BasicSkillData")]
    public class BasicSkillDataSO : ScriptableObject
    {
        [SerializeField] protected ushort Id;
        [SerializeField] protected string Name;
        [SerializeField] protected string Tag;
        [SerializeField] protected float CooldownDuration;

        public ushort GetID() => Id;
        public string GetName() => Name;
        public string GetSkillTag() => Tag;
        public float GetCooldownDuration() => CooldownDuration;
    }
}


namespace Genoverrei.Library.Database
{
    [CreateAssetMenu(fileName = "ObjectPathsDatabase", menuName = "GenoverreiLibrary/Database/ObjectPathsDatabase")]
    public class ObjectPathsDatabaseSO : ScriptableObject
    {
        public List<Object> ObjectPathsDatabase = new();
    }
}


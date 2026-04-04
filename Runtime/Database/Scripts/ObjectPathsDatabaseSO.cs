
namespace Genoverrei.Library.Database
{
    [CreateAssetMenu(fileName = "ObjectPathsDatabase", menuName = "GenoverreiLibrary/Database/ObjectPathsDatabaseSO")]
    public class ObjectPathsDatabaseSO : ScriptableObject
    {
        public List<Object> ObjectPathsDatabase = new();
    }
}


using System;

namespace Genoverrei.Library.DesignPatternCore
{
    [CreateAssetMenu(fileName = "PoolTable_", menuName = "GenoverreiLibrary/DesignPattern/PoolTable")]
    public sealed class PoolTableDataSO : ScriptableObject
    {
        [Serializable]
        public struct PoolEntry
        {
            public GameObject Prefab;
            public int InitialSize;
            public int MaxSize;
            public int LimitOverPercent;
        }

        public List<PoolEntry> Entries = new();
    }
}
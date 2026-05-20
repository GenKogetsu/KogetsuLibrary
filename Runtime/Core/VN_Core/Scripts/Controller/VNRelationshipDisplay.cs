using System.Collections.Generic;
using Kogetsu.Library.Attribute;
using UnityEngine.UI;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para> (TH) : แสดงค่าความสัมพันธ์ของตัวละครด้วย Image รูปหัวใจ — active/inactive ตามค่า RelationshipValue (0–10) </para>
    /// <para> (EN) : Displays a character's relationship value via a list of heart Images, activating N hearts out of the total. </para>
    /// </summary>
    [CreateHierarchyMenu("KogetsuLibrary/Core")]
    public class VNRelationshipDisplay : MonoBehaviour
    {
        [Required]
        [SerializeField] private VNCharacterSO _character;

        [SerializeField] private List<Image> _hearts = new();

        private void OnEnable()
        {
            if (_character == null) return;
            _character.OnRelationshipChanged += UpdateHearts;
            UpdateHearts(_character.RelationshipValue);
        }

        private void OnDisable()
        {
            if (_character == null) return;
            _character.OnRelationshipChanged -= UpdateHearts;
        }

        private void UpdateHearts(int value)
        {
            for (int i = 0; i < _hearts.Count; i++)
            {
                if (_hearts[i] == null) continue;
                _hearts[i].gameObject.SetActive(i < value);
            }
        }
    }
}

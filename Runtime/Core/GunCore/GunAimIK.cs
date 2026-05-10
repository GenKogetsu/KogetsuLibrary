namespace Genoverrei.Library.Core;

/// <summary>
/// ขยับ IK Target ของแขนให้ชี้ไปทิศเมาส์ตลอดเวลา
///
/// Setup:
/// 1. ใส่ Component นี้บน Player GameObject
/// 2. _ikTarget = GunIKTarget (Empty GO ที่เป็น Target ของ Limb Solver 2D)
/// 3. _shoulder = Transform ของ bone หัวไหล่ (จุดเริ่มต้นแขน)
/// 4. _aimRange = ระยะห่างจาก shoulder ไปยัง IK target (ความยาวแขน)
/// 5. _gunHandBone = bone มือที่ gun เป็น child — script จะ rotate ให้ตรงเมาส์
/// </summary>
public class GunAimIK : MonoBehaviour
{
    [Header("IK References")]
    [Tooltip("Empty GO ที่ตั้งเป็น Target ของ Limb Solver 2D")]
    [SerializeField] private Transform _ikTarget;

    [Tooltip("Bone หัวไหล่ — ใช้เป็นจุดอ้างอิงทิศทาง")]
    [SerializeField] private Transform _shoulder;

    [Tooltip("Bone มือ/wrist ที่ gun attach อยู่ — หมุนให้ตรงเมาส์")]
    [SerializeField] private Transform _gunHandBone;

    [Header("Settings")]
    [Tooltip("ระยะจาก shoulder ถึง IK target (ปรับตามความยาวแขนใน pixel)")]
    [SerializeField] private float _aimRange = 1.5f;

    [Tooltip("Offset มุม (องศา) ถ้า sprite ปืนไม่ชี้ตรง +X")]
    [SerializeField] private float _gunRotationOffset = 0f;

    [Tooltip("ถ้า true = flip แกน Y เมื่อเมาส์อยู่ซ้ายตัวละคร (2D side-view)")]
    [SerializeField] private bool  _flipWithCharacter = true;

    private Camera _cam;

    private void Awake() => _cam = Camera.main;

    private void LateUpdate()
    {
        // LateUpdate เพื่อ override หลัง Animation ทำงาน
        if (_ikTarget == null || _shoulder == null || _cam == null) return;

        Vector2 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 origin     = _shoulder.position;
        Vector2 dir        = (mouseWorld - origin).normalized;

        // วาง IK target ตามทิศเมาส์ ห่างจาก shoulder เท่า _aimRange
        _ikTarget.position = (Vector3)(origin + dir * _aimRange);

        // หมุน bone มือให้ตรงเมาส์
        if (_gunHandBone != null)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + _gunRotationOffset;
            _gunHandBone.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_shoulder == null || _cam == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_shoulder.position, _aimRange);

        if (_ikTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_shoulder.position, _ikTarget.position);
            Gizmos.DrawWireSphere(_ikTarget.position, 0.08f);
        }
    }
}

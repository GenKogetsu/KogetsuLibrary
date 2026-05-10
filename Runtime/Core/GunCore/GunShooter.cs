using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core;

/// <summary>
/// Shotgun ไฟฟ้า — ยิง PelletCount ลูกในแพทเทิร์น spread
/// ใช้ EnergySystem ร่วมกับ FlashlightController
/// Debug cone วาดใน Scene view โดยอัตโนมัติเมื่อ select
/// </summary>
public class GunShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform             _muzzlePoint;
    [SerializeField] private PlayerInputObserverSO _playerObserver;
    [SerializeField] private EnergySystem          _energy;
    [SerializeField] private BulletPoolManager     _bulletPool;

    [Header("Shotgun Settings")]
    [SerializeField] private int   _pelletCount = 7;
    [Tooltip("มุม spread รวม เช่น 20 = ±10°")]
    [SerializeField] private float _spreadAngle = 20f;
    [SerializeField] private float _bulletSpeed = 22f;
    [SerializeField] private float _shootRange  = 28f;

    [Header("Energy Cost")]
    [SerializeField] private float _energyPerShot = 8f;

    [Header("Debug — Shooting Range (Scene view only)")]
    [SerializeField] private bool  _showDebugCone  = true;
    [SerializeField] private float _debugConeAngle = 25f;
    [SerializeField] private Color _coneColor      = new(1f, 0.55f, 0f, 0.22f);
    [SerializeField] private Color _dashColor      = Color.yellow;
    [SerializeField] private int   _dashSegments   = 12;

    private void Awake()
    {
        if (_playerObserver != null)
            _playerObserver.OnShootChannel += TryShoot;
    }

    private void OnDestroy()
    {
        if (_playerObserver != null)
            _playerObserver.OnShootChannel -= TryShoot;
    }

    private void TryShoot()
    {
        if (_energy == null || !_energy.TrySpend(_energyPerShot)) return;

        FirePellets();

        if (EventBus.Instance)
            EventBus.Instance.Publish(new PlayerShootEvent(
                _muzzlePoint ? (Vector2)_muzzlePoint.position : Vector2.zero,
                _pelletCount));
    }

    private void FirePellets()
    {
        if (_muzzlePoint == null || _bulletPool == null) return;

        float baseAngle  = _muzzlePoint.eulerAngles.z;
        float halfSpread = _spreadAngle * 0.5f;

        for (int i = 0; i < _pelletCount; i++)
        {
            float t     = _pelletCount <= 1 ? 0f : (float)i / (_pelletCount - 1);
            float angle = baseAngle + Mathf.Lerp(-halfSpread, halfSpread, t);
            float rad   = angle * Mathf.Deg2Rad;

            Vector2 dir = new(Mathf.Cos(rad), Mathf.Sin(rad));

            ElectricBullet bullet = _bulletPool.Get();
            if (bullet == null) continue;

            bullet.transform.SetPositionAndRotation(_muzzlePoint.position, Quaternion.Euler(0f, 0f, angle));
            bullet.gameObject.SetActive(true);
            bullet.Launch(dir, _bulletSpeed, _shootRange, _bulletPool);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!_showDebugCone || _muzzlePoint == null) return;

        Vector3 origin  = _muzzlePoint.position;
        Vector3 forward = _muzzlePoint.right;
        Vector3 left    = Quaternion.Euler(0, 0,  _debugConeAngle * 0.5f) * forward;
        Vector3 right   = Quaternion.Euler(0, 0, -_debugConeAngle * 0.5f) * forward;

        UnityEditor.Handles.color = _coneColor;
        UnityEditor.Handles.DrawLine(origin, origin + left  * _shootRange);
        UnityEditor.Handles.DrawLine(origin, origin + right * _shootRange);
        UnityEditor.Handles.DrawWireArc(origin, Vector3.back, left, _debugConeAngle, _shootRange);

        Gizmos.color = _dashColor;
        for (int i = 0; i < _dashSegments; i++)
        {
            float t0 = (i * 2f)      / (_dashSegments * 2f);
            float t1 = (i * 2f + 1f) / (_dashSegments * 2f);
            Gizmos.DrawLine(origin + forward * (t0 * _shootRange),
                            origin + forward * (t1 * _shootRange));
        }
    }
#endif
}

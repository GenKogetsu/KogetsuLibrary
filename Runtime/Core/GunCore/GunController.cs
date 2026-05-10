namespace Genoverrei.Library.Core;

/// <summary>
/// หมุน GunPivot ไปทางเมาส์ และ flip PlayerSprite ตามทิศ
///
/// Hierarchy:
///   Player
///   └─ SpritePivot       ← assign PlayerSprite (flip ที่นี่)
///       └─ GunPivot      ← assign GunPivot (หมุนที่นี่)
///           ├─ GunSprite
///           ├─ MuzzlePoint
///           └─ Light2D
/// </summary>
public class GunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform              GunPivot;
    [SerializeField] private Transform              PlayerSprite;
    [SerializeField] private PlayerInputObserverSO  PlayerObserver;

    [Header("Settings")]
    [Tooltip("Offset มุมองศา — ปรับถ้า sprite ปืนไม่ชี้ตาม +X")]
    [SerializeField] private float AimAngleOffset = 0f;

    private Vector2 _mouseWorld;
    private Camera  _cam;

    private void Awake()
    {
        _cam = Camera.main;
        if (PlayerObserver != null)
            PlayerObserver.OnMousePositionChannel += OnMouseMoved;
    }

    private void OnDestroy()
    {
        if (PlayerObserver != null)
            PlayerObserver.OnMousePositionChannel -= OnMouseMoved;
    }

    private void OnMouseMoved(Vector2 screenPos)
    {
        if (_cam != null)
            _mouseWorld = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _cam.nearClipPlane));
    }

    private void Update()
    {
        AimGun();
        FlipSprite();
    }

    private void AimGun()
    {
        if (GunPivot == null) return;
        Vector2 dir   = _mouseWorld - (Vector2)GunPivot.position;
        float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + AimAngleOffset;
        GunPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void FlipSprite()
    {
        if (PlayerSprite == null || GunPivot == null) return;
        bool    left  = _mouseWorld.x < GunPivot.position.x;
        Vector3 scale = PlayerSprite.localScale;
        scale.x       = left ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        PlayerSprite.localScale = scale;
    }
}

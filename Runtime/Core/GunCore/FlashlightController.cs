using Genoverrei.Library.DesignPatternCore;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
namespace Genoverrei.Library.Core;

/// <summary>
/// ไฟฉาย URP Light2D ที่ปลายปืน — toggle เปิด/ปิด ดูด energy ต่อเนื่อง
/// ถ้า energy หมดไฟดับอัตโนมัติ และห้ามเปิดถ้ายัง energy = 0
/// </summary>
[RequireComponent(typeof(Light2DBase))]
public class FlashlightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputObserverSO _playerObserver;
    [SerializeField] private EnergySystem          _energy;

    [Header("Settings")]
    [SerializeField] private float _energyDrainPerSecond = 3f;
    [Tooltip("Offset ตามแกน +X ของ parent (gun forward)")]
    [SerializeField] private float _lightOffset = 0f;

    private Light2DBase _light;
    private bool    _isOn = true;

    private void Awake()
    {
        _light = GetComponent<Light2DBase>();
        if (_playerObserver != null)
            _playerObserver.OnFlashlightToggleChannel += Toggle;
    }

    private void OnDestroy()
    {
        if (_playerObserver != null)
            _playerObserver.OnFlashlightToggleChannel -= Toggle;
    }

    private void Update()
    {
        if (!_isOn || _energy == null) return;

        _energy.Drain(_energyDrainPerSecond * Time.deltaTime);

        if (_energy.Current <= 0f)
            SetLight(false);
    }

    private void LateUpdate()
    {
        if (_lightOffset != 0f)
            transform.localPosition = new Vector3(_lightOffset, 0f, 0f);
    }

    private void Toggle()
    {
        if (!_isOn && _energy is { Current: <= 0f }) return;
        SetLight(!_isOn);
    }

    private void SetLight(bool on)
    {
        _isOn          = on;
        _light.enabled = _isOn;

        if (EventBus.Instance)
            EventBus.Instance.Publish(new FlashlightToggleEvent(_isOn));
    }
}

using System;
using System.Collections;
using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

namespace Kogetsu.Library.Core;

[Serializable]
public abstract class BaseMoveAbility3D : BaseMoveAbility<IMoveContext3D>
{
    // ─── Damage Flash ─────────────────────────────────────────────────────
    [Header("Damage Flash")]
    [Tooltip("ถ้าว่างจะหา MeshRenderer บน Context Transform แล้ว fallback ไป children อัตโนมัติ")]
    [SerializeField] private MeshRenderer _flashTarget;

    [Tooltip("Property name ของสีใน shader\nURP Lit = _BaseColor | Built-in Standard = _Color")]
    [SerializeField] private string _colorPropertyName = "_BaseColor";
    [SerializeField] private Color  _flashColor         = Color.red;
    [Min(0.01f)]
    [SerializeField] private float  _flashDuration      = 0.2f;

    private Color             _originalColor;
    private Coroutine         _flashCoroutine;
    private MonoBehaviour     _flashRunner;
    private MaterialPropertyBlock _mpb;
    private int               _colorId;

    public override void Initialize(IMoveContext3D context)
    {
        base.Initialize(context);

        _flashRunner = context as MonoBehaviour;

        if (_flashTarget == null)
        {
            if (!context.Transform.TryGetComponent(out _flashTarget))
                _flashTarget = context.Transform.GetComponentInChildren<MeshRenderer>();
        }

        _colorId = Shader.PropertyToID(_colorPropertyName);
        _mpb     = new MaterialPropertyBlock();

        _originalColor = _flashTarget != null && _flashTarget.sharedMaterial != null
            ? _flashTarget.sharedMaterial.GetColor(_colorPropertyName)
            : Color.white;
    }

    /// <summary>Trigger damage flash — เรียกจาก damage handler หรือ ability ใดก็ได้</summary>
    protected void Flash()
    {
        if (_flashRunner == null || _flashTarget == null) return;
        if (_flashCoroutine != null) _flashRunner.StopCoroutine(_flashCoroutine);
        _flashCoroutine = _flashRunner.StartCoroutine(Flash3DRoutine());
    }

    private void ApplyColor(Color color)
    {
        _flashTarget.GetPropertyBlock(_mpb);
        _mpb.SetColor(_colorId, color);
        _flashTarget.SetPropertyBlock(_mpb);
    }

    private IEnumerator Flash3DRoutine()
    {
        ApplyColor(_flashColor);

        float elapsed = 0f;
        while (elapsed < _flashDuration)
        {
            elapsed += Time.deltaTime;
            ApplyColor(Color.Lerp(_flashColor, _originalColor, elapsed / _flashDuration));
            yield return null;
        }

        ApplyColor(_originalColor);
        _flashCoroutine = null;
    }
}

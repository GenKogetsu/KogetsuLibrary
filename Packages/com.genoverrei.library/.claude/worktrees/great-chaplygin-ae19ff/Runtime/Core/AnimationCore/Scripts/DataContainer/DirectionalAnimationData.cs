using System;

namespace Genoverrei.Library.Core;

[Serializable]
public struct DirectionalAnimationData
{
    public Vector3 Direction;
    public GameObject Model;
    public AnimationClip Clip;
}

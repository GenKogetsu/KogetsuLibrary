namespace Genoverrei.Library.Core;

/// <summary>
/// ประเภทของ rig ที่ตัวละครใช้
/// ใช้โดย MoveController2D เพื่อบอก Editor + StatsController / RiggedHurtFlash
/// </summary>
public enum RigMode2D
{
    /// <summary>
    /// SpriteRenderer เดียว + Animator แบบ sprite sheet frames
    /// (ทั้ง body อยู่ใน texture เดียว)
    /// </summary>
    Spritesheet,

    /// <summary>
    /// หลาย SpriteRenderer แยกชิ้น + Unity 2D Animation rig (SpriteSkin / bone)
    /// ใช้กับ PSD Importer หรือ cutout rig
    /// </summary>
    Cutout2D,
}

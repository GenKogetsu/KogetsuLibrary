using System;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.Extension
{
    public static class DirectionModeExtension
    {
        public static byte ToByte(this DirectionMode mode) => mode switch
        {
            DirectionMode.OneDiraction => 1,
            DirectionMode.TwoDiraction => 2,
            DirectionMode.FourDiraction => 4,
            DirectionMode.EightDiraction => 8,
            _ => 0
        };
    }
}
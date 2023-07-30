﻿namespace CherukPhotoEditor.Domain;

public enum ColorSpace
{
    Rgb,
    Hsl, // H обычно содержит значения от 0 до 360, но в данном случае их нужно нормировать до максимум 255
    Hsv, // H обычно содержит значения от 0 до 360, но в данном случае их нужно нормировать до максимум 255
    YCbCr601,
    YCbCr709,
    YCoCg,
    Cmy
}
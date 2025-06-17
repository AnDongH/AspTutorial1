using System;
using MemoryPack;

namespace AspTutorial1.Formatters;

public static class MemoryPackFormatterHelper
{
    public static bool IsMemoryPackable(Type type)
    {
        if (type == null) return false;
        return type.GetCustomAttributes(typeof(MemoryPackableAttribute), true).Length > 0;
    }
}
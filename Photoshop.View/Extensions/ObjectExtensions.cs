using System.Collections.Generic;

namespace Photoshop.View.Extensions;

public static class ObjectExtensions
{
    public static void AddTo<T>(this T obj, List<T> collection)
    {
        collection.Add(obj);
    }
}
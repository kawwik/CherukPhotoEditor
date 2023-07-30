﻿using System.Collections.Generic;

namespace CherukPhotoEditor.View.Utils.Extensions;

public static class ObjectExtensions
{
    public static void AddTo<T>(this T obj, List<T> collection)
    {
        collection.Add(obj);
    }
}
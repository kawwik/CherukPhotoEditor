using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Photoshop.View.Converters;

public abstract class ConverterBase<TSource, TResult> : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TSource source || !typeof(TResult).IsAssignableTo(targetType))
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        return ConvertInternal(source);
    }

    public virtual object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TResult source || !typeof(TSource).IsAssignableTo(targetType))
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);

        return ConvertBackInternal(source);
    }

    protected virtual TSource ConvertBackInternal(TResult source)
    {
        throw new NotSupportedException();
    }

    protected abstract TResult ConvertInternal(TSource source);
}
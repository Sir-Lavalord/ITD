using System;
using System.Linq.Expressions;

namespace ITD.Utilities;

public static class TypeUtils
{
    public static Func<T, V> GetFieldAccessor<T, V>(string fieldName)
    {
        var param = Expression.Parameter(typeof(T), "arg");
        var member = Expression.Field(param, fieldName);
        var lambda = Expression.Lambda(typeof(Func<T, V>), member, param);

        return lambda.Compile() as Func<T, V>;
    }

    public static Action<T, V> SetFieldAccessor<T, V>(string fieldName)
    {
        var param = Expression.Parameter(typeof(T), "arg");
        var valueParam = Expression.Parameter(typeof(V), "value");
        var member = Expression.Field(param, fieldName);
        var assign = Expression.Assign(member, valueParam);
        var lambda = Expression.Lambda(typeof(Action<T, V>), assign, param, valueParam);

        return lambda.Compile() as Action<T, V>;
    }
}
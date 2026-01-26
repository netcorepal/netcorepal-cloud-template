using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Dm;
using Microsoft.EntityFrameworkCore.Dm.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace ABC.Template.Infrastructure;

// see https://www.cnblogs.com/pains/p/18605329
public class MyDmDateTimeOffsetTypeMapping : DmDateTimeOffsetTypeMapping
{
    public MyDmDateTimeOffsetTypeMapping(string storeType,
        DbType? dbType = System.Data.DbType.DateTimeOffset)
        : base(storeType, dbType)
    {
    }

    protected MyDmDateTimeOffsetTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        return new MyDmDateTimeOffsetTypeMapping(parameters);
    }

    public override MethodInfo GetDataReaderMethod()
    {
        return typeof(DmDataReader).GetRuntimeMethod(nameof(DmDataReader.GetDateTimeOffset), new[] { typeof(int) })!;
    }
}

public class MyDmTimeSpanTypeMapping : DmTimeSpanTypeMapping
{
    public MyDmTimeSpanTypeMapping(string storeType, DbType? dbType = null)
        : base(storeType, dbType)
    {
    }

    protected MyDmTimeSpanTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        return new MyDmTimeSpanTypeMapping(parameters);
    }

    public override MethodInfo GetDataReaderMethod()
    {
        return typeof(DmDataReader).GetRuntimeMethod(nameof(DmDataReader.GetDouble), new[] { typeof(int) })!;
    }

    public override Expression CustomizeDataReaderExpression(Expression expression)
    {
        return Expression.Call(GetTimeSpanMethod, expression);
    }

    private static readonly MethodInfo GetTimeSpanMethod
        = typeof(MyDmTimeSpanTypeMapping).GetMethod(nameof(GetTimeSpan), new[] { typeof(double) })!;

    public static TimeSpan GetTimeSpan(double value)
    {
        return TimeSpan.FromDays(value);
    }
}

public class MyDmTypeMappingSource : DmTypeMappingSource
{
    private MyDmDateTimeOffsetTypeMapping _datetimeoffset =
        new MyDmDateTimeOffsetTypeMapping("DATETIME WITH TIME ZONE", DbType.DateTimeOffset);

    private MyDmDateTimeOffsetTypeMapping _datetimeoffset3 =
        new MyDmDateTimeOffsetTypeMapping("DATETIME(3) WITH TIME ZONE", DbType.DateTimeOffset);

    private MyDmTimeSpanTypeMapping _intervaldt = new MyDmTimeSpanTypeMapping("INTERVAL DAY TO SECOND");
    private Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
    private Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

    public MyDmTypeMappingSource(TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
        _storeTypeMappings = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
        {
            { "datetime with time zone", _datetimeoffset },
            { "timestamp with time zone", _datetimeoffset },
            { "datetime(3) with time zone", _datetimeoffset3 },
            { "timestamp(3) with time zone", _datetimeoffset3 },
            { "interval day to second", _intervaldt },
        };
        _clrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
        {
            { typeof(DateTimeOffset), _datetimeoffset },
            { typeof(TimeSpan), _intervaldt }
        };
    }

    protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        Type? clrType = mappingInfo.ClrType;
        string? storeTypeName = mappingInfo.StoreTypeName;
        string? storeTypeNameBase = mappingInfo.StoreTypeNameBase;
        if (storeTypeName != null && _storeTypeMappings.ContainsKey(storeTypeName))
            return _storeTypeMappings.GetValueOrDefault(storeTypeName)?.Clone(mappingInfo)!;
        if (clrType != null && _clrTypeMappings.ContainsKey(clrType))
            return _clrTypeMappings.GetValueOrDefault(clrType)?.Clone(mappingInfo)!;

        return base.FindMapping(mappingInfo);
    }
}

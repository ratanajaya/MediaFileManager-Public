using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Helpers;

public static class ObjectCloner
{
    public static T Clone<T>(this T source) {
        var bytes = Utf8Json.JsonSerializer.Serialize(source);
        return Utf8Json.JsonSerializer.Deserialize<T>(bytes);
    }
}
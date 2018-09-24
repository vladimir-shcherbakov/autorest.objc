using AutoRest.Core.Model;
using System.Collections.Generic;

namespace AutoRest.ObjC.Model
{
    public interface IModelTypeObjC : IModelType
    {
        IEnumerable<string> Imports { get; }
        
        IModelTypeObjC ResponseVariant { get; }
        IModelTypeObjC ParameterVariant { get; }

        IModelTypeObjC NonNullableVariant { get; }
    }

    public static class ModelTypeExtensions
    {
        public static IModelType ServiceResponseVariant(this IModelType original, bool wantNullable = false)
        {
            if (wantNullable)
            {
                return original; // the original is always nullable
            }
            return (original as IModelTypeObjC)?.ResponseVariant?.NonNullableVariant ?? original;
        }

        public static string GetDefaultValue(this IModelType type, Method parent)
        {
            return type is PrimaryTypeObjC && type.Name == "RequestBody"
                ? "RequestBody.create(MediaType.parse(\"" + parent.RequestContentType + "\"), new byte[0])"
                : type.DefaultValue;
        }
    }
}

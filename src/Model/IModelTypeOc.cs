using AutoRest.Core.Model;
using System.Collections.Generic;

namespace AutoRest.ObjectiveC.Model
{
    public interface IModelTypeOc : IModelType
    {
        IEnumerable<string> Imports { get; }
        
        IModelTypeOc ResponseVariant { get; }
        IModelTypeOc ParameterVariant { get; }

        IModelTypeOc NonNullableVariant { get; }
    }

    public static class IModelTypeExtensions
    {
        public static IModelType ServiceResponseVariant(this IModelType original, bool wantNullable = false)
        {
            if (wantNullable)
            {
                return original; // the original is always nullable
            }
            return (original as IModelTypeOc)?.ResponseVariant?.NonNullableVariant ?? original;
        }

        public static string GetDefaultValue(this IModelType type, Method parent)
        {
            return type is PrimaryTypeOc && type.Name == "RequestBody"
                ? "RequestBody.create(MediaType.parse(\"" + parent.RequestContentType + "\"), new byte[0])"
                : type.DefaultValue;
        }
    }
}

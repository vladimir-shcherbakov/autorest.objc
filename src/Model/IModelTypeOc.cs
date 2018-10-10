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

        string NameForMethod { get; }
    }

    public static class ModelTypeExtensions
    {
        public static IModelTypeOc ServiceResponseVariant(this IModelType original, bool wantNullable = false)
        {
            return (IModelTypeOc)original;
//            if (wantNullable)
//            {
//                return (IModelTypeOc)original; // the original is always nullable
//            }
//            return (IModelTypeOc) ((original as IModelTypeOc)?.ResponseVariant?.NonNullableVariant ?? original);
        }

        public static string GetDefaultValue(this IModelType type, Method parent)
        {
            return type is PrimaryTypeOc && type.Name == "RequestBody"
                ? "RequestBody.create(MediaType.parse(\"" + parent.RequestContentType + "\"), new byte[0])"
                : type.DefaultValue;
        }
    }
}

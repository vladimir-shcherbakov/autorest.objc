using System.Collections.Generic;
using System.Linq;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using Newtonsoft.Json;

namespace AutoRest.ObjC.Model
{
    public class SequenceTypeObjC : SequenceType, IModelTypeObjC
    {
        public SequenceTypeObjC()
        {
            Name.OnGet += v => "NSArray*";
        }

        [JsonIgnore]
        public IModelTypeObjC ResponseVariant
        {
            get
            {
                var respvariant = (ElementType as IModelTypeObjC)?.ResponseVariant;
                if (respvariant != ElementType && (respvariant as PrimaryTypeObjC)?.Nullable != false)
                {
                    return new SequenceTypeObjC { ElementType = respvariant };
                }
                return this;
            }
        }

        [JsonIgnore]
        public IModelTypeObjC ParameterVariant
        {
            get
            {
                var respvariant = (ElementType as IModelTypeObjC)?.ParameterVariant;
                if (respvariant != ElementType && (respvariant as PrimaryTypeObjC)?.Nullable != false)
                {
                    return new SequenceTypeObjC { ElementType = respvariant };
                }
                return this;
            }
        }

        [JsonIgnore]
        public IEnumerable<string> Imports
        {
            get
            {
                var imports = new List<string> { "java.util.List" };
                return imports.Concat(((IModelTypeObjC) this.ElementType).Imports);
            }
        }

        [JsonIgnore]
        public IModelTypeObjC NonNullableVariant => this;
    }
}

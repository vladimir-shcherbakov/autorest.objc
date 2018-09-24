using System.Collections.Generic;
using System.Linq;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using System;
using Newtonsoft.Json;
    
namespace AutoRest.ObjC.Model
{
    public class DictionaryTypeObjC : DictionaryType, IModelTypeObjC
    {
        public DictionaryTypeObjC()
        {
            Name.OnGet += value => $"Map<String, {ValueType.Name}>";
        }

        [JsonIgnore]
        public IEnumerable<string> Imports
        {
            get
            {
                List<string> imports = new List<string> { "java.util.Map" };
                return imports.Concat((this.ValueType as IModelTypeObjC)?.Imports ?? Enumerable.Empty<string>());
            }
        }

        [JsonIgnore]
        public IModelTypeObjC ResponseVariant
        {
            get
            {
                var respvariant = (ValueType as IModelTypeObjC).ResponseVariant;
                if (respvariant != ValueType && (respvariant as PrimaryTypeObjC)?.Nullable != false)
                {
                    return new DictionaryTypeObjC { ValueType = respvariant };
                }
                return this;
            }
        }

        [JsonIgnore]
        public IModelTypeObjC ParameterVariant
        {
            get
            {
                var respvariant = (ValueType as IModelTypeObjC).ParameterVariant;
                if (respvariant != ValueType && (respvariant as PrimaryTypeObjC)?.Nullable != false)
                {
                    return new DictionaryTypeObjC { ValueType = respvariant };
                }
                return this;
            }
        }

        [JsonIgnore]
        public IModelTypeObjC NonNullableVariant => this;
    }
}

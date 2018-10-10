using System.Collections.Generic;
using System.Linq;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using System;
using Newtonsoft.Json;

namespace AutoRest.ObjectiveC.Model
{
    public class DictionaryTypeOc : DictionaryType, IModelTypeOc
    {
        public DictionaryTypeOc()
        {
            //Name.OnGet += value => $"Map<String, {ValueType.Name}>";
            Name.OnGet += value => $"NSDictionary<NSString*, {ValueType.Name}>";
        }

        [JsonIgnore]
        public IEnumerable<string> Imports
        {
            get
            {
                var imports = new List<string> ();
                return imports.Concat((this.ValueType as IModelTypeOc)?.Imports ?? Enumerable.Empty<string>());
            }
        }

        [JsonIgnore]
        public IModelTypeOc ResponseVariant
        {
            get
            {
                var respvariant = (ValueType as IModelTypeOc)?.ResponseVariant;
                if (respvariant != ValueType && (respvariant as PrimaryTypeOc)?.Nullable != false)
                {
                    return new DictionaryTypeOc { ValueType = respvariant };
                }
                return this;
            }
        }

        [JsonIgnore]
        public IModelTypeOc ParameterVariant
        {
            get
            {
                var respvariant = (ValueType as IModelTypeOc)?.ParameterVariant;
                if (respvariant != ValueType && (respvariant as PrimaryTypeOc)?.Nullable != false)
                {
                    return new DictionaryTypeOc { ValueType = respvariant };
                }
                return this;
            }
        }

        [JsonIgnore]
        public IModelTypeOc NonNullableVariant => this;

        public string NameForMethod => $"{Name}";
    }
}

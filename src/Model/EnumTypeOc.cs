using System.Collections.Generic;
using System.Globalization;
using AutoRest.Core.Model;
using Newtonsoft.Json;
using AutoRest.Core;

namespace AutoRest.ObjectiveC.Model
{
    public class EnumTypeOc : EnumType, IModelTypeOc
    {
        public EnumTypeOc()
        {
            //Name.OnGet += name => string.IsNullOrEmpty(name) || name == "enum" ? "String" : CodeNamer.Instance.GetTypeName(name);
            Name.OnGet += name => string.IsNullOrEmpty(name) || name == "enum" ? "NSString" : CodeNamer.Instance.GetTypeName(name);
        }

        [JsonIgnore]
        public virtual string ModelsPackage => (this.CodeModel as CodeModelOc).ModelsPackage;

        [JsonIgnore]
        public virtual IEnumerable<string> Imports
        {
            get
            {
                //                if (Name != "String")
                //                {
                //                    yield return string.Join(".",
                //                        CodeModel?.Namespace.ToLowerInvariant(),
                //                        "models", Name);
                //                }
                if (Name != "NSString")
                    yield return $"{Name}";
            }
        }

        [JsonIgnore]
        public IModelTypeOc ResponseVariant => this;

        [JsonIgnore]
        public IModelTypeOc ParameterVariant => this;

        [JsonIgnore]
        public IModelTypeOc NonNullableVariant => this;

        public string NameForMethod => $"{Name}*";
    }
}

using System.Collections.Generic;
using System.Globalization;
using AutoRest.Core.Model;
using Newtonsoft.Json;
using AutoRest.Core;

namespace AutoRest.ObjC.Model
{
    public class EnumTypeObjC : EnumType, IModelTypeObjC
    {
        public EnumTypeObjC()
        {
            Name.OnGet += name => string.IsNullOrEmpty(name) || name == "enum" ? "String" : CodeNamer.Instance.GetTypeName(name);
        }

        [JsonIgnore]
        public virtual string ModelsPackage => (this.CodeModel as CodeModelObjC)?.ModelsPackage;

        [JsonIgnore]
        public virtual IEnumerable<string> Imports
        {
            get
            {
                if (Name != "String")
                {
                    yield return string.Join(".",
                        CodeModel?.Namespace.ToLowerInvariant(),
                        "models", Name);
                }
            }
        }

        [JsonIgnore]
        public IModelTypeObjC ResponseVariant => this;

        [JsonIgnore]
        public IModelTypeObjC ParameterVariant => this;

        [JsonIgnore]
        public IModelTypeObjC NonNullableVariant => this;
    }
}

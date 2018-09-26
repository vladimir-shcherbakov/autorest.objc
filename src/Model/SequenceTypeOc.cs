﻿using System.Collections.Generic;
using System.Linq;
using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using Newtonsoft.Json;

namespace AutoRest.ObjectiveC.Model
{
    public class SequenceTypeOc : SequenceType, IModelTypeOc
    {
        public SequenceTypeOc()
        {
            Name.OnGet += v => $"List<{ElementType.Name}>";
        }

        [JsonIgnore]
        public IModelTypeOc ResponseVariant
        {
            get
            {
                var respvariant = (ElementType as IModelTypeOc).ResponseVariant;
                if (respvariant != ElementType && (respvariant as PrimaryTypeOc)?.Nullable != false)
                {
                    return new SequenceTypeOc { ElementType = respvariant };
                }
                return this;
            }
        }

        [JsonIgnore]
        public IModelTypeOc ParameterVariant
        {
            get
            {
                var respvariant = (ElementType as IModelTypeOc).ParameterVariant;
                if (respvariant != ElementType && (respvariant as PrimaryTypeOc)?.Nullable != false)
                {
                    return new SequenceTypeOc { ElementType = respvariant };
                }
                return this;
            }
        }

        [JsonIgnore]
        public IEnumerable<string> Imports
        {
            get
            {
                List<string> imports = new List<string> { "java.util.List" };
                return imports.Concat(((IModelTypeOc) this.ElementType).Imports);
            }
        }

        [JsonIgnore]
        public IModelTypeOc NonNullableVariant => this;
    }
}

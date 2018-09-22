// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using AutoRest.Core.Model;
using AutoRest.Core.Utilities;

namespace AutoRest.ObjC.Model
{
    public class EnumTypeObjC : EnumType, IVariableType
    {
        public bool HasUniqueNames { get; set; }

        public EnumTypeObjC UnNamedEnumRelatedType { get; set; }

        public EnumTypeObjC()
        {
            // the default value for unnamed enums is "enum"
            Name.OnGet += (v) => v == "enum" ? "string" : v;

            // Assume members have unique names
            HasUniqueNames = true;
        }

        public bool IsRequired { get; set; }

        public EnumTypeObjC(EnumType source) : this()
        {
            this.LoadFrom(source);
        }

        public bool IsNamed => Name != "string" && Values.Any();

        public IDictionary<string, string> Constants
        {
            get
            {
                var constants = new Dictionary<string, string>();
                Values
                    .ForEach(v =>
                    {
                        constants.Add(HasUniqueNames ? v.Name : Name + v.Name, v.SerializedName);
                    });

                return constants;
            }
        }

        public string Documentation { get; set; }

        public string DecodeTypeDeclaration(bool isRequired)
        {
            var retVal = this.IsNamed ? this.Name.FixedValue + "Enum" : "String";
            return this.UnNamedEnumRelatedType != null ? this.UnNamedEnumRelatedType.DecodeTypeDeclaration(isRequired) : ObjCNameHelper.GetTypeName(retVal, isRequired);
        }

        public string EncodeTypeDeclaration(bool isRequired)
        {
            var retVal = this.IsNamed ? this.Name.FixedValue+ "Enum" : "String";
            return this.UnNamedEnumRelatedType != null ? this.UnNamedEnumRelatedType.EncodeTypeDeclaration(isRequired) : ObjCNameHelper.GetTypeName(retVal, isRequired);
        }

        public string VariableTypeDeclaration(bool isRequired)
        {
            var retVal = this.IsNamed ? this.Name.FixedValue+ "Enum" : "String";
            return this.UnNamedEnumRelatedType != null ? this.UnNamedEnumRelatedType.VariableTypeDeclaration(isRequired) : ObjCNameHelper.GetTypeName(retVal, isRequired);
        }

        public string TypeName
        {
            get
            {
                var retVal = this.IsNamed ? this.Name.FixedValue + "Enum" : "String";
                return this.UnNamedEnumRelatedType != null ? this.UnNamedEnumRelatedType.TypeName : retVal;
            }
        }

        public string VariableName => ObjCNameHelper.ConvertToVariableName(this.Name);
    }
}

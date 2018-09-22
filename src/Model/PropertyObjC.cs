// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Utilities;
using AutoRest.Core.Model;

namespace AutoRest.ObjC.Model
{
    public class PropertyObjC : Property, IVariableType
    {
        public PropertyObjC()
        {
            Name.OnGet += value =>
            {
                return value;
            };
        }

        public string VariableName
        {
            get
            {
                return ObjCNameHelper.ConvertToVariableName(this.Name);
            }
        }

        public string VariableTypeDeclaration(bool isRequired)
        {
            if (this.ModelType is IVariableType)
            {
                return ((IVariableType)this.ModelType).VariableTypeDeclaration(isRequired);
            }

            return ObjCNameHelper.GetTypeName(this.ModelType.Name, isRequired);
        }

        public string DecodeTypeDeclaration(bool isRequired)
        {
            if (this.ModelType is IVariableType)
            {
                return ((IVariableType)this.ModelType).VariableTypeDeclaration(isRequired);
            }

            return ObjCNameHelper.GetTypeName(this.ModelType.Name, isRequired);
        }

        public string EncodeTypeDeclaration(bool isRequired)
        {
            if (this.ModelType is IVariableType)
            {
                return ((IVariableType)this.ModelType).EncodeTypeDeclaration(isRequired);
            }

            return ObjCNameHelper.GetTypeName(this.ModelType.Name, isRequired);
        }
    }
}

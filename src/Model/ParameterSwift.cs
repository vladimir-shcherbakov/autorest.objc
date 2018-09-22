// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Utilities;
using AutoRest.Core.Model;
using AutoRest.Extensions;
using AutoRest.Extensions.Azure;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoRest.ObjC.Model
{
    public class ParameterObjC : Parameter, IVariableType
    {
        public const string APIVersionName = "APIVersion";
        
        public ParameterObjC()
        {

        }

        public virtual bool IsAPIVersion => SerializedName.IsApiVersion();

        public string VariableName => ObjCNameHelper.ConvertToVariableName(this.Name);

        public string VariableTypeDeclaration(bool isRequired)
        {
                var retVal = this.ModelType.Name;
                return this.ModelType is IVariableType type 
                    ? type.VariableTypeDeclaration(isRequired) 
                    : ObjCNameHelper.GetTypeName(retVal, isRequired);
        }

        public string EncodeTypeDeclaration(bool isRequired)
        {
                var retVal = this.ModelType.Name;
                return this.ModelType is IVariableType type 
                    ? type.EncodeTypeDeclaration(isRequired) 
                    : ObjCNameHelper.GetTypeName(retVal, isRequired);
        }

        public string DecodeTypeDeclaration(bool isRequired)
        {
                var retVal = this.ModelType.Name;
                return this.ModelType is IVariableType type 
                    ? type.DecodeTypeDeclaration(isRequired) 
                    : ObjCNameHelper.GetTypeName(retVal, isRequired);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Model;

namespace AutoRest.ObjC.Model
{
    public class ArrayTypeObjC : SequenceType, IVariableType
    {
        public ArrayTypeObjC() : base()
        {
        }

        public string VariableTypeDeclaration(bool isRequired)
        {
            var retVal = $"NSArray*";
            if (ElementType is IVariableType)
            {
                retVal = $"NSArray (ElementType is IVariableType)";
            }

            return ObjCNameHelper.GetTypeName(retVal, isRequired);
        }

        public string DecodeTypeDeclaration(bool isRequired)
        {
//            var retVal = $"[{ElementType.Name}]";
//            if (ElementType is IVariableType)
//            {
//                retVal = $"[{((IVariableType)ElementType).DecodeTypeDeclaration(isRequired)}]";
//            }
//
//            return ObjCNameHelper.GetTypeName(retVal, isRequired);
            return VariableTypeDeclaration(isRequired);
        }

        public string EncodeTypeDeclaration(bool isRequired)
        {
//            var retVal = $"[{ElementType.Name}]";
//            if (ElementType is IVariableType)
//            {
//                retVal = $"[{((IVariableType)ElementType).EncodeTypeDeclaration(isRequired)}]";
//            }
//
//            return ObjCNameHelper.GetTypeName(retVal, isRequired);
            return VariableTypeDeclaration(isRequired);
        }

        public string VariableName => ObjCNameHelper.ConvertToVariableName(this.Name);
    }
}

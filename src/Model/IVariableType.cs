using AutoRest.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRest.ObjC.Model
{
    public interface IVariableType
    {
        string VariableTypeDeclaration(bool isRequired);

        string VariableName { get; }

        string DecodeTypeDeclaration(bool isRequired);

        string EncodeTypeDeclaration(bool isRequired);
    }
}

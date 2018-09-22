using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRest.ObjC.Model
{
    internal static class ObjCNameHelper
    {

        internal static string ConvertToVariableName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && name.Length > 1)
            {
                name = name.Replace(" ", "").Replace("-", "");
                name = name.Substring(0, 1).ToLower() + name.Substring(1);

            }

            if(CodeNamerObjC.reservedWords.Contains(name)) {
                name = name + "SuffixToAvoidReservedWord";
            }

//            if(name.Equals("body", StringComparison.OrdinalIgnoreCase)) {
//                name = "_body";
//            }

            return name;
        }

        internal static string ConvertToValidObjCTypeName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name) && name.Length > 1)
            {
                name = name.Replace(" ", "").Replace("-", "");
                
                name = name.Substring(0, 1).ToUpper() + name.Substring(1);

            }

            if(CodeNamerObjC.reservedWords.Contains(name)) {
                name =name + "SuffixToAvoidReservedWord";
            }
            
            return name;
        }

        internal static string GetTypeName(string name, bool isRequired)
        {
            //return name + (isRequired || name.EndsWith("?") ? "" : "?");
            return name;
        }
    }
}

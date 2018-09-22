// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using AutoRest.Core;
using AutoRest.ObjC.Model;

namespace AutoRest.ObjC
{
    public class CodeNamerObjC : CodeNamer
    {
        public new static CodeNamerObjC Instance => (CodeNamerObjC)CodeNamer.Instance;

        public virtual IEnumerable<string> AutorestImports => new string[] { PrimaryTypeObjC.GetImportLine(package: "ObjCAutorest") };

        private HashSet<string> CommonInitialisms => new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                                                            "Acl",
                                                            "Api",
                                                            "Ascii",
                                                            "Cpu",
                                                            "Css",
                                                            "Dns",
                                                            "Eof",
                                                            "Guid",
                                                            "Html",
                                                            "Http",
                                                            "Https",
                                                            "Id",
                                                            "Ip",
                                                            "Json",
                                                            "Lhs",
                                                            "Qps",
                                                            "Ram",
                                                            "Rhs",
                                                            "Rpc",
                                                            "Sla",
                                                            "Smtp",
                                                            "Sql",
                                                            "Ssh",
                                                            "Tcp",
                                                            "Tls",
                                                            "Ttl",
                                                            "Udp",
                                                            "Ui",
                                                            "Uid",
                                                            "Uuid",
                                                            "Uri",
                                                            "Url",
                                                            "Utf8",
                                                            "Vm",
                                                            "Xml",
                                                            "Xsrf",
                                                            "Xss",
                                                        };

        public string[] UserDefinedNames => new string[] {
                                                            "UserAgent",
                                                            "Version",
                                                            "APIVersion",
                                                            "DefaultBaseURI",
                                                            "ManagementClient",
                                                            "NewWithBaseURI",
                                                            "New",
                                                            "NewWithoutDefaults",
                                                        };

        public static HashSet<string> reservedWords = new HashSet<string>(new string[] {
            "if", "else", "switch", "case", "default", "break", "int", "float", "char", "double", "long", "for", "while", "do",
            "void", "goto", "auto", "signed", "const", "extern", "register", "unsigned", "return", "continue", "enum", "sizeof",
            "struct", "typedef", "union", "volatile"
            
        });

        /// <summary>
        /// Formats a string
        /// </summary>
        /// <param name="name"></param>
        /// <param name="packageName"></param>
        /// <param name="nameInUse"></param>
        /// <param name="attachment"></param>
        /// <returns>The formatted string</returns>
        public static string AttachTypeName(string name, string packageName, bool nameInUse, string attachment)
        {
            if(reservedWords.Contains(name)) 
            {
                name = name + "SuffixToAvoidReservedWord";
            }

            name = nameInUse
                ? name.Equals(packageName, StringComparison.OrdinalIgnoreCase)
                    ? name
                    : name + attachment
                : name;

            return name;
        }

        public override string GetEnumMemberName(string member) {
            var retVal = ObjCNameHelper.ConvertToValidObjCTypeName(base.GetEnumMemberName(member));
            retVal = retVal.Replace(" ", "");
            return retVal;
        }
    }
}
﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using AutoRest.Core;
using AutoRest.Core.Utilities;
using AutoRest.Extensions;
using AutoRest.Core.Model;
using Newtonsoft.Json;
using AutoRest.Core.Utilities.Collections;

namespace AutoRest.ObjC.Model
{
    public class MethodObjC : Method
    {
        [JsonIgnore]
        public virtual IEnumerable<ParameterObjC> RetrofitParameters
        {
            get
            {
                var parameters = LogicalParameters.OfType<ParameterObjC>();
//                    .Where(p => p.Location != ParameterLocation.None)
//                    .Where(p => !p.Extensions.ContainsKey("hostParameter"))
//                    .ToList();
                
//                if (IsParameterizedHost)
//                {
//                    parameters.Add(new ParameterObjC
//                    {
//                        Name = "parameterizedHost",
//                        SerializedName = "x-ms-parameterized-host",
//                        Location = ParameterLocation.Header,
//                        IsRequired = true,
//                        ModelType = new PrimaryTypeObjC(KnownPrimaryType.String)
//                    });
//                }
                return parameters;
            }
        }

        [JsonIgnore]
        public IEnumerable<ParameterObjC> OrderedRetrofitParameters
        {
            get
            {
                return RetrofitParameters.Where(p => p.Location == ParameterLocation.Path)
                    .Union(RetrofitParameters.Where(p => p.Location != ParameterLocation.Path));
            }
        }

        /// <summary>
        /// Generate the method parameter declarations for a method
        /// </summary>
        [JsonIgnore]
        public virtual string MethodParameterApiDeclaration
        {
            get
            {
                List<string> declarations = new List<string>();
                foreach (ParameterObjC parameter in OrderedRetrofitParameters)
                {
                    bool alreadyEncoded = parameter.Extensions.ContainsKey(SwaggerExtensions.SkipUrlEncodingExtension) &&
                        (bool) parameter.Extensions[SwaggerExtensions.SkipUrlEncodingExtension] == true;

                    StringBuilder declarationBuilder = new StringBuilder();
                    if (Url.Contains("{" + parameter.Name + "}"))
                    {
                        parameter.Location = ParameterLocation.Path;
                    }

                    if ((parameter.Location == ParameterLocation.Path || parameter.Location == ParameterLocation.Query) && alreadyEncoded)
                    {
                        declarationBuilder.Append(string.Format(CultureInfo.InvariantCulture,
                            "@{0}(value = \"{1}\", encoded = true) ",
                            parameter.Location.ToString(),
                            parameter.SerializedName));
                    }
                    else if (parameter.Location == ParameterLocation.Path ||
                        parameter.Location == ParameterLocation.Query ||
                        parameter.Location == ParameterLocation.Header)
                    {
                        declarationBuilder.Append(string.Format(CultureInfo.InvariantCulture,
                            "@{0}(\"{1}\") ",
                            parameter.Location.ToString(),
                            parameter.SerializedName));
                    }
                    else if (parameter.Location == ParameterLocation.Body)
                    {
                        declarationBuilder.Append(string.Format(CultureInfo.InvariantCulture,
                            "@{0} ",
                            parameter.Location.ToString()));
                    }
                    else if (parameter.Location == ParameterLocation.FormData)
                    {
                        declarationBuilder.Append(string.Format(CultureInfo.InvariantCulture,
                            "@Part(\"{0}\") ",
                            parameter.SerializedName));
                    }
                    var declarativeName = parameter.ClientProperty != null ? parameter.ClientProperty.Name : parameter.Name;
                    declarationBuilder.Append(parameter.WireType.Name);
                    declarationBuilder.Append(" " + declarativeName);
                    declarations.Add(declarationBuilder.ToString());
                }

                var declaration = string.Join(", ", declarations);
                return declaration;
            }
        }

        [JsonIgnore]
        public virtual string MethodParameterDeclaration
        {
            get
            {
                List<string> declarations = new List<string>();
                foreach (var parameter in LocalParameters.Where(p => !p.IsConstant))
                {
                    declarations.Add(parameter.ClientType.ParameterVariant.Name + " " + parameter.Name);
                }

                var declaration = string.Join(", ", declarations);
                return declaration;
            }
        }

        [JsonIgnore]
        public virtual string MethodRequiredParameterDeclaration
        {
            get
            {
                List<string> declarations = new List<string>();
                foreach (var parameter in LocalParameters.Where(p => !p.IsConstant && p.IsRequired))
                {
                    declarations.Add(parameter.ClientType.ParameterVariant.Name + " " + parameter.Name);
                }

                var declaration = string.Join(", ", declarations);
                return declaration;
            }
        }

        [JsonIgnore]
        public string MethodParameterInvocation
        {
            get
            {
                List<string> invocations = new List<string>();
                foreach (var parameter in LocalParameters.Where(p => !p.IsConstant))
                {
                    invocations.Add(parameter.Name);
                }

                var declaration = string.Join(", ", invocations);
                return declaration;
            }
        }

        [JsonIgnore]
        public string MethodDefaultParameterInvocation
        {
            get
            {
                List<string> invocations = new List<string>();
                foreach (var parameter in LocalParameters)
                {
                    if (parameter.IsRequired)
                    {
                        invocations.Add(parameter.Name);
                    }
                    else
                    {
                        invocations.Add("null");
                    }
                }

                var declaration = string.Join(", ", invocations);
                return declaration;
            }
        }

        [JsonIgnore]
        public string MethodRequiredParameterInvocation
        {
            get
            {
                List<string> invocations = new List<string>();
                foreach (var parameter in LocalParameters)
                {
                    if (parameter.IsRequired && !parameter.IsConstant)
                    {
                        invocations.Add(parameter.Name);
                    }
                }

                var declaration = string.Join(", ", invocations);
                return declaration;
            }
        }

        [JsonIgnore]
        public string MethodParameterApiInvocation
        {
            get
            {
                List<string> invocations = new List<string>();
                foreach (var parameter in OrderedRetrofitParameters)
                {
                    invocations.Add(parameter.WireName);
                }

                var declaration = string.Join(", ", invocations);
                return declaration;
            }
        }

        [JsonIgnore]
        public string MethodRequiredParameterApiInvocation
        {
            get
            {
                List<string> invocations = new List<string>();
                foreach (var parameter in OrderedRetrofitParameters)
                {
                    invocations.Add(parameter.WireName);
                }

                var declaration = string.Join(", ", invocations);
                return declaration;
            }
        }

        [JsonIgnore]
        public virtual bool IsParameterizedHost => CodeModel.Extensions.ContainsKey(SwaggerExtensions.ParameterizedHostExtension);

        /// <summary>
        /// Generate a reference to the ServiceClient
        /// </summary>
        [JsonIgnore]
        public string ClientReference => Group.IsNullOrEmpty() ? "this" : "this.client";

        [JsonIgnore]
        public string ParameterConversion
        {
            get
            {
                IndentedStringBuilder builder = new IndentedStringBuilder();
                foreach (var p in RetrofitParameters)
                {
                    if (p.NeedsConversion)
                    {
                        builder.Append(p.ConvertToWireType(p.Name, ClientReference));
                    }
                }
                return builder.ToString();
            }
        }

        [JsonIgnore]
        public string RequiredParameterConversion
        {
            get
            {
                IndentedStringBuilder builder = new IndentedStringBuilder();
                foreach (var p in RetrofitParameters.Where(p => p.IsRequired))
                {
                    if (p.NeedsConversion)
                    {
                        builder.Append(p.ConvertToWireType(p.Name, ClientReference));
                    }
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Generates input mapping code block.
        /// </summary>
        /// <returns></returns>
        public virtual string BuildInputMappings(bool filterRequired = false)
        {
            var builder = new IndentedStringBuilder();
            foreach (var transformation in InputParameterTransformation)
            {
                var outParamName = transformation.OutputParameter.Name;
                while (Parameters.Any(p => p.Name == outParamName))
                {
                    outParamName += "1";
                }
                transformation.OutputParameter.Name = outParamName;
                var nullCheck = BuildNullCheckExpression(transformation);
                bool conditionalAssignment = !string.IsNullOrEmpty(nullCheck) && !transformation.OutputParameter.IsRequired && !filterRequired;
                if (conditionalAssignment)
                {
                    builder.AppendLine("{0} {1} = null;",
                            ((ParameterObjC) transformation.OutputParameter).ClientType.ParameterVariant.Name,
                            outParamName);
                    builder.AppendLine("if ({0}) {{", nullCheck).Indent();
                }

                if (transformation.ParameterMappings.Any(m => !string.IsNullOrEmpty(m.OutputParameterProperty)) &&
                    transformation.OutputParameter.ModelType is CompositeType)
                {
                    builder.AppendLine("{0}{1} = new {2}();",
                        !conditionalAssignment ? ((ParameterObjC)transformation.OutputParameter).ClientType.ParameterVariant.Name + " " : "",
                        outParamName,
                        transformation.OutputParameter.ModelType.Name);
                }

                foreach (var mapping in transformation.ParameterMappings)
                {
                    builder.AppendLine("{0}{1}{2};",
                        !conditionalAssignment && !(transformation.OutputParameter.ModelType is CompositeType) ?
                            ((ParameterObjC)transformation.OutputParameter).ClientType.ParameterVariant.Name + " " : "",
                        outParamName,
                        GetMapping(mapping, filterRequired));
                }

                if (conditionalAssignment)
                {
                    builder.Outdent()
                       .AppendLine("}");
                }
            }

            return builder.ToString();
        }

        private static string GetMapping(ParameterMapping mapping, bool filterRequired = false)
        {
            string inputPath = mapping.InputParameter.Name;
            if (mapping.InputParameterProperty != null)
            {
                inputPath += "." + CodeNamer.Instance.CamelCase(mapping.InputParameterProperty) + "()";
            }
            if (filterRequired && !mapping.InputParameter.IsRequired)
            {
                inputPath = "null";
            }

            string outputPath = "";
            if (mapping.OutputParameterProperty != null)
            {
                outputPath += ".with" + CodeNamer.Instance.PascalCase(mapping.OutputParameterProperty);
                return string.Format(CultureInfo.InvariantCulture, "{0}({1})", outputPath, inputPath);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} = {1}", outputPath, inputPath);
            }
        }

        private static string BuildNullCheckExpression(ParameterTransformation transformation)
        {
            if (transformation == null)
            {
                throw new ArgumentNullException("transformation");
            }

            return string.Join(" || ",
                transformation.ParameterMappings
                    .Where(m => !m.InputParameter.IsRequired)
                    .Select(m => m.InputParameter.Name + " != null"));
        }

        [JsonIgnore]
        public IEnumerable<ParameterObjC> RequiredNullableParameters
        {
            get
            {
                foreach (var parameter in Parameters)
                {
                    var param = (ParameterObjC) parameter;
                    if (!param.ModelType.IsPrimaryType(KnownPrimaryType.Int) &&
                        !param.ModelType.IsPrimaryType(KnownPrimaryType.Double) &&
                        !param.ModelType.IsPrimaryType(KnownPrimaryType.Boolean) &&
                        !param.ModelType.IsPrimaryType(KnownPrimaryType.Long) &&
                        !param.ModelType.IsPrimaryType(KnownPrimaryType.UnixTime) &&
                        !param.IsConstant && param.IsRequired)
                    {
                        yield return param;
                    }
                }
            }
        }

        [JsonIgnore]
        public IEnumerable<ParameterObjC> ParametersToValidate
        {
            get
            {
                foreach (var parameter in Parameters)
                {
                    var param = (ParameterObjC) parameter;
                    if (param.ModelType is PrimaryType ||
                        param.ModelType is EnumType ||
                        param.IsConstant)
                    {
                        continue;
                    }
                    yield return param;
                }
            }
        }

        /// <summary>
        /// Gets the expression for response body initialization
        /// </summary>
        [JsonIgnore]
        public virtual string InitializeResponseBody => string.Empty;

        [JsonIgnore]
        public virtual string MethodParameterDeclarationWithCallback
        {
            get
            {
                var parameters = MethodParameterDeclaration;
                if (!parameters.IsNullOrEmpty())
                {
                    parameters += ", ";
                }
                parameters += string.Format(CultureInfo.InvariantCulture, "final ServiceCallback<{0}> serviceCallback",
                    ReturnTypeObjC.GenericBodyClientTypeString);
                return parameters;
            }
        }

        [JsonIgnore]
        public virtual string MethodRequiredParameterDeclarationWithCallback
        {
            get
            {
                var parameters = MethodRequiredParameterDeclaration;
                if (!parameters.IsNullOrEmpty())
                {
                    parameters += ", ";
                }
                parameters += string.Format(CultureInfo.InvariantCulture, "final ServiceCallback<{0}> serviceCallback",
                    ReturnTypeObjC.GenericBodyClientTypeString);
                return parameters;
            }
        }

        [JsonIgnore]
        public virtual string MethodParameterInvocationWithCallback
        {
            get
            {
                var parameters = MethodParameterInvocation;
                if (!parameters.IsNullOrEmpty())
                {
                    parameters += ", ";
                }
                parameters += "serviceCallback";
                return parameters;
            }
        }

        [JsonIgnore]
        public virtual string MethodRequiredParameterInvocationWithCallback
        {
            get
            {
                var parameters = MethodDefaultParameterInvocation;
                if (!parameters.IsNullOrEmpty())
                {
                    parameters += ", ";
                }
                parameters += "serviceCallback";
                return parameters;
            }
        }

        /// <summary>
        /// Get the parameters that are actually method parameters in the order they appear in the method signature
        /// exclude global parameters
        /// </summary>
        [JsonIgnore]
        public IEnumerable<ParameterObjC> LocalParameters
        {
            get
            {
                //Omit parameter-group properties for now since Java doesn't support them yet
                var par = Parameters
                    .OfType<ParameterObjC>()
                    .Where(p => p != null && !p.IsClientProperty && !string.IsNullOrWhiteSpace(p.Name))
                    .OrderBy(item => !item.IsRequired)
                    .ToList();
                return par;
            }
        }

        [JsonIgnore]
        public string HostParameterReplacementArgs
        {
            get
            {
                var args = new List<string>();
                foreach (var param in Parameters.Where(p => p.Extensions.ContainsKey("hostParameter")))
                {
                    args.Add("\"{" + param.SerializedName + "}\", " + param.Name);
                }
                return string.Join(", ", args);
            }
        }

        /// <summary>
        /// Get the type for operation exception
        /// </summary>
        [JsonIgnore]
        public virtual string OperationExceptionTypeString
        {
            get
            {
                if (this.DefaultResponse.Body is CompositeType)
                {
                    var type = this.DefaultResponse.Body as CompositeTypeObjC;
                    return type.ExceptionTypeDefinitionName;
                }
                else
                {
                    return "RestException";
                }
            }
        }

        [JsonIgnore]
        public virtual IEnumerable<string> Exceptions
        {
            get
            {
                yield return OperationExceptionTypeString;
                yield return "IOException";
                if (RequiredNullableParameters.Any())
                {
                    yield return "IllegalArgumentException";
                }
            }
        }

        [JsonIgnore]
        public virtual string ExceptionString => string.Join(", ", Exceptions);

        [JsonIgnore]
        public virtual List<string> ExceptionStatements
        {
            get
            {
                var exceptions = new List<string>
                {
                    OperationExceptionTypeString + " exception thrown from REST call",
                    "IOException exception thrown from serialization/deserialization"
                };
                if (RequiredNullableParameters.Any())
                {
                    exceptions.Add("IllegalArgumentException exception thrown from invalid parameters");
                }
                return exceptions;
            }
        }

        [JsonIgnore]
        public string CallType => this.HttpMethod == HttpMethod.Head ? "Void" : "ResponseBody";

        [JsonIgnore]
        public virtual string ResponseBuilder => "ServiceResponseBuilder";

        [JsonIgnore]
        public virtual string RuntimeBasePackage => "com.microsoft.rest";

        [JsonIgnore]
        public ResponseObjC ReturnTypeObjC => ReturnType as ResponseObjC;

        [JsonIgnore]
        public virtual string ReturnTypeResponseName => ReturnTypeObjC?.BodyClientType?.ServiceResponseVariant()?.Name;

        public virtual string ResponseGeneration(bool filterRequired = false)
        {
            if (!ReturnTypeObjC.NeedsConversion) return "";
            IndentedStringBuilder builder= new IndentedStringBuilder();
            builder.AppendLine("ServiceResponse<{0}> response = {1}Delegate(call.execute());",
                ReturnTypeObjC.GenericBodyWireTypeString, this.Name.ToCamelCase());
            builder.AppendLine("{0} body = null;", ReturnTypeObjC.BodyClientType.Name)
                .AppendLine("if (response.body() != null) {")
                .Indent().AppendLine("{0}", ReturnTypeObjC.ConvertBodyToClientType("response.body()", "body"))
                .Outdent().AppendLine("}");
            return builder.ToString();
        }

        [JsonIgnore]
        public virtual string ReturnValue
        {
            get
            {
                if (ReturnTypeObjC.NeedsConversion)
                {
                    return "new ServiceResponse<" + ReturnTypeObjC.GenericBodyClientTypeString + ">(body, response.response())";
                }
                return this.Name + "Delegate(call.execute())";
            }
        }

        public virtual string ClientResponse(bool filterRequired = false)
        {
            var builder = new IndentedStringBuilder();
            if (ReturnTypeObjC.NeedsConversion)
            {
                builder.AppendLine("ServiceResponse<{0}> result = {1}Delegate(response);", ReturnTypeObjC.GenericBodyWireTypeString, this.Name);
                builder.AppendLine("{0} body = null;", ReturnTypeObjC.BodyClientType.Name)
                    .AppendLine("if (result.body() != null) {")
                    .Indent().AppendLine("{0}", ReturnTypeObjC.ConvertBodyToClientType("result.body()", "body"))
                    .Outdent().AppendLine("}");
                builder.AppendLine("ServiceResponse<{0}> clientResponse = new ServiceResponse<{0}>(body, result.response());",
                    ReturnTypeObjC.GenericBodyClientTypeString);
            }
            else
            {
                builder.AppendLine("{0} clientResponse = {1}Delegate(response);", ReturnTypeObjC.WireResponseTypeString, this.Name);
            }
            return builder.ToString();
        }

        [JsonIgnore]
        public virtual string ServiceFutureFactoryMethod
        {
            get
            {
                string factoryMethod = "fromResponse";
                if (ReturnType.Headers != null)
                {
                    factoryMethod = "fromHeaderResponse";
                }
                return factoryMethod;
            }
        }

        [JsonIgnore]
        public virtual string CallbackDocumentation => " * @param serviceCallback the async ServiceCallback to handle successful and failed responses.";

        [JsonIgnore]
        public virtual List<string> InterfaceImports
        {
            get
            {
                var imports = new HashSet<string>
                {
                    "rx.Observable",
                    "com.microsoft.rest.ServiceFuture",
                    "com.microsoft.rest." + ReturnTypeObjC.ClientResponseType,
                    "com.microsoft.rest.ServiceCallback"
                };
                // static imports
                // parameter types
                this.Parameters.OfType<ParameterObjC>().ForEach(p => imports.AddRange(p.InterfaceImports));
                // return type
                imports.AddRange(this.ReturnTypeObjC.InterfaceImports);
                // exceptions
                this.ExceptionString.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .ForEach(ex =>
                    {
                        string exceptionImport = CodeNamerObjC.GetJavaException(ex, CodeModel);
                        if (exceptionImport != null) imports.Add(CodeNamerObjC.GetJavaException(ex, CodeModel));
                    });                return imports.ToList();
            }
        }

        [JsonIgnore]
        public virtual List<string> ImplImports
        {
            get
            {
                var imports = new HashSet<string> {"rx.Observable", "rx.functions.Func1"};
                // static imports
                if (RequestContentType == "multipart/form-data" || RequestContentType == "application/x-www-form-urlencoded")
                {
                    imports.Add("retrofit2.http.Multipart");
                }
                else
                {
                    imports.Add("retrofit2.http.Headers");
                }
                imports.Add("retrofit2.Response");
                if (this.HttpMethod != HttpMethod.Head)
                {
                    imports.Add("okhttp3.ResponseBody");
                }
                imports.Add("com.microsoft.rest.ServiceFuture");
                imports.Add("com.microsoft.rest." + ReturnTypeObjC.ClientResponseType);
                imports.Add("com.microsoft.rest.ServiceCallback");
                this.RetrofitParameters.ForEach(p => imports.AddRange(p.RetrofitImports));
                // Http verb annotations
                imports.Add(this.HttpMethod.ImportFrom());
                // response type conversion
                if (this.Responses.Any())
                {
                    imports.Add("com.google.common.reflect.TypeToken");
                }
                // validation
                if (!ParametersToValidate.IsNullOrEmpty())
                {
                    imports.Add("com.microsoft.rest.Validator");
                }
                // parameters
                this.LocalParameters.Concat(this.LogicalParameters.OfType<ParameterObjC>())
                    .ForEach(p => imports.AddRange(p.ClientImplImports));
                this.RetrofitParameters.ForEach(p => imports.AddRange(p.WireImplImports));
                // return type
                imports.AddRange(this.ReturnTypeObjC.ImplImports);
                if (ReturnType.Body.IsPrimaryType(KnownPrimaryType.Stream))
                {
                    imports.Add("retrofit2.http.Streaming");
                }
                // response type (can be different from return type)
                this.Responses.ForEach(r => imports.AddRange((r.Value as ResponseObjC).ImplImports));
                // exceptions
                this.ExceptionString.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .ForEach(ex =>
                    {
                        string exceptionImport = CodeNamerObjC.GetJavaException(ex, CodeModel);
                        if (exceptionImport != null) imports.Add(CodeNamerObjC.GetJavaException(ex, CodeModel));
                    });
                // parameterized host
                if (IsParameterizedHost)
                {
                    imports.Add("com.google.common.base.Joiner");
                }
                return imports.ToList();
            }
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
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

namespace AutoRest.ObjectiveC.Model
{
    public class MethodOc : Method
    {
        [JsonIgnore]
        public virtual IEnumerable<ParameterOc> RetrofitParameters
        {
            get
            {
                var parameters = LogicalParameters.OfType<ParameterOc>().Where(p => p.Location != ParameterLocation.None)
                    .Where(p => !p.Extensions.ContainsKey("hostParameter")).ToList();
                if (IsParameterizedHost)
                {
                    parameters.Add(new ParameterOc
                    {
                        Name = "parameterizedHost",
                        SerializedName = "x-ms-parameterized-host",
                        Location = ParameterLocation.Header,
                        IsRequired = true,
                        ModelType = new PrimaryTypeOc(KnownPrimaryType.String)
                    });
                }
                return parameters;
            }
        }

        [JsonIgnore]
        public IEnumerable<ParameterOc> OrderedRetrofitParameters
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
                foreach (var parameter in OrderedRetrofitParameters)
                {
                    var alreadyEncoded = parameter.Extensions.ContainsKey(SwaggerExtensions.SkipUrlEncodingExtension) &&
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
                var declarations = LocalParameters
                    .Where(p => !p.IsConstant)
                    .Select(parameter => 
                        $"with{parameter.Name.ToPascalCase()}:({parameter.ClientType.Name} *){parameter.Name}")
                    .ToList();

                var declaration = string.Join(" ", declarations);
                var declarationWithCallback = string.IsNullOrEmpty(declaration)
                    ? CallbackParameterDeclaration
                    : $"{declaration} {CallbackParameterDeclaration}";
                return declarationWithCallback.StartWithUppercase();
            }
        }

        [JsonIgnore]
        public virtual string MethodRequiredParameterDeclaration
        {
            get
            {
                var declarations = LocalParameters
                    .Where(p => !p.IsConstant && p.IsRequired)
                    .Select(parameter => 
                        $"with{parameter.Name.ToPascalCase()}:({parameter.ClientType.Name} *){parameter.Name}")
                    .ToList();

                var declaration = string.Join(" ", declarations);
                var declarationWithCallback = string.IsNullOrEmpty(declaration)
                    ? CallbackParameterDeclaration
                    : $"{declaration} {CallbackParameterDeclaration}";

                return declarationWithCallback.StartWithUppercase();
            }
        }

        [JsonIgnore]
        public string MethodBodyParameterCreationTestCase
        {
            get
            {
                var declarations = LocalParameters
                    .Where(p => !p.IsConstant)
                    .Select(parameter =>
                        //$"with{parameter.Name.ToPascalCase()} : ({parameter.ClientType.Name} *) {parameter.Name}") 
                        $"{parameter.ClientType.Name} *{parameter.Name} = [{parameter.ClientType.Name} new]")
                    .ToList();

                if (declarations.Count == 0) return null;

                var declaration = string.Join("\n", declarations);
                return declaration.StartWithUppercase() + ";";
            }
        }

        public virtual string CallbackParameterInvokationTestcase => ReturnTypeResponseName == "void"
            ? $"withCallback:^(AZOperationError* error)"
            : $"withCallback:^({ReturnTypeResponseName}* result, AZOperationError* error)";

        [JsonIgnore]
        public string MethodParameterInvocationTestCase
        {
            get
            {
                var invocations = LocalParameters
                    .Where(p => !p.IsConstant)
                    .Select(parameter =>
                        $"with{parameter.Name.ToPascalCase()}:{parameter.Name}")
                    .ToList();

                var invocation = string.Join(" ", invocations);
                var invocationsWithCallback = string.IsNullOrEmpty(invocation)
                    ? CallbackParameterInvokationTestcase
                    : $"{invocation} {CallbackParameterInvokationTestcase}";

                return invocationsWithCallback.StartWithUppercase();
            }
        }

        [JsonIgnore]
        public string MethodParameterInvocation
        {
            get
            {
                var invocations = LocalParameters
                    .Where(p => !p.IsConstant)
                    .Select(parameter =>
                        $"with{parameter.Name.ToPascalCase()}:{parameter.Name}")
                    .ToList();

                var invocation = string.Join(" ", invocations);
                var invocationsWithCallback = string.IsNullOrEmpty(invocation)
                    ? CallbackParameterInvocation
                    : $"{invocation} {CallbackParameterInvocation}";

                return invocationsWithCallback.StartWithUppercase();
            }
        }

        [JsonIgnore]
        public string MethodDefaultParameterInvocation
        {
            get
            {
                var invocations = new List<string>();
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
                var invocations = new List<string>();
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
                var invocations = OrderedRetrofitParameters.Select(parameter => parameter.WireName).ToList();

                var declaration = string.Join(", ", invocations);
                return declaration;
            }
        }

        [JsonIgnore]
        public string MethodRequiredParameterApiInvocation
        {
            get
            {
                var invocations = OrderedRetrofitParameters.Select(parameter => parameter.WireName).ToList();

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
        public string ClientReference => Group.IsNullOrEmpty() ? "self" : "self.service";

        [JsonIgnore]
        public string ParameterConversion
        {
            get
            {
                var builder = new IndentedStringBuilder();
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
                var builder = new IndentedStringBuilder();
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
        public virtual string BuildInputMappings()
        {
            var builder = new IndentedStringBuilder();
            foreach (var transformation in InputParameterTransformation)
            {
                var compositeOutputParameter = transformation.OutputParameter.ModelType as CompositeType;
                if (transformation.OutputParameter.IsRequired && compositeOutputParameter != null)
                {
                    builder.AppendLine("{0}* {1} = [{0} new];",
                        transformation.OutputParameter.ModelTypeName,
                        transformation.OutputParameter.Name);
                }
                else
                {
                    builder.AppendLine("{0}* {1} = nil;",
                        transformation.OutputParameter.ModelTypeName,
                        transformation.OutputParameter.Name);
                }
                var nullCheck = BuildNullCheckExpression(transformation);
                if (!string.IsNullOrEmpty(nullCheck))
                {
                    builder.AppendLine("if ({0}) {{", nullCheck)
                       .Indent();
                }

                if (transformation.ParameterMappings.Any(m => !string.IsNullOrEmpty(m.OutputParameterProperty)) &&
                    compositeOutputParameter != null && !transformation.OutputParameter.IsRequired)
                {
                    builder.AppendLine("{0}* {1} = [{0} new];",
                        transformation.OutputParameter.Name,
                        transformation.OutputParameter.ModelType.Name);
                }

                foreach (var mapping in transformation.ParameterMappings)
                {
                    builder.AppendLine("{0};", mapping.CreateCode(transformation.OutputParameter));
                }

                if (!string.IsNullOrEmpty(nullCheck))
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
                inputPath = "nil";
            }

            var outputPath = "";
            if (mapping.OutputParameterProperty != null)
            {
                outputPath += "." + mapping.OutputParameterProperty;
                return $"{outputPath} = {inputPath}";
            }
            else
            {
                return $"{outputPath} = {inputPath}";
            }
        }

        private static string BuildNullCheckExpression(ParameterTransformation transformation)
        {
            if (transformation == null)
            {
                throw new ArgumentNullException(nameof(transformation));
            }

            return string.Join(" || ",
                transformation.ParameterMappings
                    .Where(m => !m.InputParameter.IsRequired)
                    .Select(m => m.InputParameter.Name + " != null"));
        }

        [JsonIgnore]
        public IEnumerable<ParameterOc> RequiredNullableParameters
        {
            get
            {
                return Parameters.Cast<ParameterOc>()
                    .Where(param =>
                        !param.IsConstant 
                        && param.IsRequired);
            }
        }

        [JsonIgnore]
        public IEnumerable<ParameterOc> ParametersToValidate
        {
            get
            {
                return Parameters.Cast<ParameterOc>()
                    .Where(param => 
                        !(param.ModelType is PrimaryType) 
                        && !(param.ModelType is EnumType) 
                        && !param.IsConstant);
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
                    ReturnTypeOc.GenericBodyClientTypeString);
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
                    ReturnTypeOc.GenericBodyClientTypeString);
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
        public IEnumerable<ParameterOc> LocalParameters
        {
            get
            {
                //Omit parameter-group properties for now since Java doesn't support them yet
                var par = Parameters
                    .OfType<ParameterOc>()
                    .Where(p => !p.IsClientProperty && !string.IsNullOrWhiteSpace(p.Name))
                    .OrderBy(item => !item.IsRequired);
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
        public virtual string OperationErrorTypeName
        {
            get
            {
                if (this.DefaultResponse.Body is CompositeType)
                {
                    var type = this.DefaultResponse.Body as CompositeTypeOc;
                    return type?.ErrorTypeName;
                }
                else
                {
                    return "AZDefaultErrorModel";
                }
            }
        }

        [JsonIgnore]
        public virtual IEnumerable<string> Exceptions
        {
            get
            {
                yield return OperationErrorTypeName;
                yield return "IOException";
                if (RequiredNullableParameters.Any())
                {
                    yield return "IllegalArgumentException";
                }
            }
        }

        [JsonIgnore]
        public virtual string ExceptionString => string.Join(", ", Exceptions);

        // [JsonIgnore]
        // public virtual List<string> ExceptionStatements
        // {
        //     get
        //     {
        //         var exceptions = new List<string>
        //         {
        //             OperationErrorTypeName + " exception thrown from REST call",
        //             "IOException exception thrown from serialization/deserialization"
        //         };

        //         if (RequiredNullableParameters.Any())
        //         {
        //             exceptions.Add("IllegalArgumentException exception thrown from invalid parameters");
        //         }

        //         return exceptions;
        //     }
        // }

        [JsonIgnore]
        public string CallType => this.HttpMethod == HttpMethod.Head ? "id" : "ResponseBody";

        [JsonIgnore]
        public virtual string ResponseBuilder => "ServiceResponseBuilder";

        [JsonIgnore]
        public virtual string RuntimeBasePackage => "com.microsoft.rest";

        [JsonIgnore]
        public ResponseOc ReturnTypeOc => ReturnType as ResponseOc;

        [JsonIgnore]
        public virtual string ReturnTypeResponseName => ReturnTypeOc?.BodyClientType?.ServiceResponseVariant()?.Name;

        public virtual string CallbackParameterDeclaration => ReturnTypeResponseName == "void"
            ? $"withCallback:(void(^)(AZOperationError*))callback"
            : $"withCallback:(void(^)({ReturnTypeResponseName}*, AZOperationError*))callback";
        
        public virtual string CallbackParameterInvocation => $"withCallback:callback";

        public virtual string CallbackParameterDescription => ReturnTypeResponseName == "void"
            ? $"@param callback A block where AZOperationError is nil if the operation is successful"
            : $"@param callback A block where {ReturnTypeResponseName} is a result object and AZOperationError is nil, if the operation is successful";

        

        public virtual string ResponseGeneration(bool filterRequired = false)
        {
            if (ReturnTypeOc.NeedsConversion)
            {
                var builder= new IndentedStringBuilder();
                builder.AppendLine("ServiceResponse<{0}> response = {1}Delegate(call.execute());",
                    ReturnTypeOc.GenericBodyWireTypeString, this.Name.ToCamelCase());
                builder.AppendLine("{0} body = null;", ReturnTypeOc.BodyClientType.Name)
                    .AppendLine("if (response.body() != null) {")
                    .Indent().AppendLine("{0}", ReturnTypeOc.ConvertBodyToClientType("response.body()", "body"))
                    .Outdent().AppendLine("}");
                return builder.ToString();
            }
            return "";
        }

        [JsonIgnore]
        public virtual string ReturnValue
        {
            get
            {
                if (ReturnTypeOc.NeedsConversion)
                {
                    return "new ServiceResponse<" + ReturnTypeOc.GenericBodyClientTypeString + ">(body, response.response())";
                }
                return this.Name + "Delegate(call.execute())";
            }
        }

        public virtual string ClientResponse(bool filterRequired = false)
        {
            IndentedStringBuilder builder = new IndentedStringBuilder();
            if (ReturnTypeOc.NeedsConversion)
            {
                builder.AppendLine("ServiceResponse<{0}> result = {1}Delegate(response);", ReturnTypeOc.GenericBodyWireTypeString, this.Name);
                builder.AppendLine("{0} body = null;", ReturnTypeOc.BodyClientType.Name)
                    .AppendLine("if (result.body() != null) {")
                    .Indent().AppendLine("{0}", ReturnTypeOc.ConvertBodyToClientType("result.body()", "body"))
                    .Outdent().AppendLine("}");
                builder.AppendLine("ServiceResponse<{0}> clientResponse = new ServiceResponse<{0}>(body, result.response());",
                    ReturnTypeOc.GenericBodyClientTypeString);
            }
            else
            {
                builder.AppendLine("{0} clientResponse = {1}Delegate(response);", ReturnTypeOc.WireResponseTypeString, this.Name);
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
        public virtual string CallbackDocumentation => 
            " * @param serviceCallback the async ServiceCallback to handle successful and failed responses.";

        [JsonIgnore]
        public virtual List<string> InterfaceImports
        {
            get
            {
                var imports = new HashSet<string>();
                // static imports
//                imports.Add("rx.Observable");
//                imports.Add("com.microsoft.rest.ServiceFuture");
//                imports.Add("com.microsoft.rest." + ReturnTypeOc.ClientResponseType);
//                imports.Add("com.microsoft.rest.ServiceCallback");
                // parameter types
                this.Parameters.OfType<ParameterOc>().ForEach(p => imports.AddRange(p.InterfaceImports));
                // return type
                imports.AddRange(this.ReturnTypeOc.InterfaceImports);
                // exceptions
//                this.ExceptionString.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
//                    .ForEach(ex =>
//                    {
//                        string exceptionImport = CodeNamerOc.GetJavaException(ex, CodeModel);
//                        if (exceptionImport != null) imports.Add(CodeNamerOc.GetJavaException(ex, CodeModel));
//                    });
                return imports.ToList();
            }
        }

        [JsonIgnore]
        public virtual List<string> ImplImports
        {
            get
            {
                var imports = new HashSet<string>
                {
                    OperationErrorTypeName
                };
                // static imports
//                imports.Add("rx.Observable");
//                imports.Add("rx.functions.Func1");
//                if (RequestContentType == "multipart/form-data" || RequestContentType == "application/x-www-form-urlencoded")
//                {
//                    imports.Add("retrofit2.http.Multipart");
//                }
//                else
//                {
//                    imports.Add("retrofit2.http.Headers");
//                }
//                imports.Add("retrofit2.Response");
//                if (this.HttpMethod != HttpMethod.Head)
//                {
//                    imports.Add("okhttp3.ResponseBody");
//                }
//                imports.Add("com.microsoft.rest.ServiceFuture");
//                imports.Add("com.microsoft.rest." + ReturnTypeOc.ClientResponseType);
//                imports.Add("com.microsoft.rest.ServiceCallback");
//                this.RetrofitParameters.ForEach(p => imports.AddRange(p.RetrofitImports));
//                // Http verb annotations
//                imports.Add(this.HttpMethod.ImportFrom());
//                // response type conversion
//                if (this.Responses.Any())
//                {
//                    imports.Add("com.google.common.reflect.TypeToken");
//                }
//                // validation
//                if (!ParametersToValidate.IsNullOrEmpty())
//                {
//                    imports.Add("com.microsoft.rest.Validator");
//                }
//                // parameters
//                this.LocalParameters.Concat(this.LogicalParameters.OfType<ParameterOc>())
//                    .ForEach(p => imports.AddRange(p.ClientImplImports));
                InputParameterTransformation.ForEach(p => imports.Add(p.OutputParameter.ModelType.Name));
//                this.RetrofitParameters.ForEach(p => imports.AddRange(p.WireImplImports));
//                // return type
//                imports.AddRange(this.ReturnTypeOc.ImplImports);
//                if (ReturnType.Body.IsPrimaryType(KnownPrimaryType.Stream))
//                {
//                    imports.Add("retrofit2.http.Streaming");
//                }
//                // response type (can be different from return type)
//                this.Responses.ForEach(r => imports.AddRange((r.Value as ResponseOc).ImplImports));
//                // exceptions
//                this.ExceptionString.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
//                    .ForEach(ex =>
//                    {
//                        string exceptionImport = CodeNamerOc.GetJavaException(ex, CodeModel);
//                        if (exceptionImport != null) imports.Add(CodeNamerOc.GetJavaException(ex, CodeModel));
//                    });
//                // parameterized host
//                if (IsParameterizedHost)
//                {
//                    imports.Add("com.google.common.base.Joiner");
//                }
                return imports.ToList();
            }
        }
    }
}
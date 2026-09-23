using Alkami.Contracts;
using Alkami.Data.Validations;
using Alkami.Security;
using Alkami.Utilities.LegacyIdentity;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using Alkami.Monitoring;
#if NET6_0_OR_GREATER
using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;
#else
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
#endif
using System.Text;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    public partial class BaseErrorHandler : IOperationInvoker
    {
        private const string UnauthorizedAccessErrorMessage = "You do not have the proper permissions to make this call";

        private static readonly ILog Logger = LogManager.GetLogger<BaseErrorHandler>();
        private static readonly DataContractSerializer Serializer;

        private readonly Func<BaseResponse> _faultedOutput;
        private readonly IOperationInvoker _innerInvoker;
        private readonly string _operationName;

        static BaseErrorHandler()
        {
            Serializer = new DataContractSerializer(typeof(ClaimsIdentity), new[] { typeof(GenericIdentity), typeof(WindowsIdentity) });
#if NET6_0_OR_GREATER
            Serializer.SetSerializationSurrogateProvider(Utilities.Rpc.SerializationSurrogateProvider.GetDefault());
#endif
        }

        public BaseErrorHandler(IOperationInvoker innerInvoker, OperationDescription operationDescription, string operationName)
        {
            _innerInvoker = innerInvoker;
            _operationName = operationName;

            if (operationDescription.Messages.Count != 2)
                throw new InvalidOperationException("You must only have a single input and single output of types BaseRequest and BaseResponse respectively");

            var input = operationDescription.Messages.First(x => x.Direction == MessageDirection.Input);
            if (input.Body.Parts.Count != 1)
                throw new InvalidOperationException("You must only have a single input and single output of types BaseRequest and BaseResponse respectively");

            if (!typeof(BaseRequest).IsAssignableFrom(input.Body.Parts[0].Type))
                throw new InvalidOperationException("Message must send in a single object of type BaseRequest and return a single value of BaseResponse");
            var oput = operationDescription.Messages.First(x => x.Direction == MessageDirection.Output);

            var outputType = oput.Body.ReturnValue.Type;

            if (!typeof(BaseResponse).IsAssignableFrom(outputType))
                throw new InvalidOperationException("Message must send in a single object of type BaseRequest and return a single value of BaseResponse");

            _faultedOutput = CreateFactory(outputType);

            this.PermissionsToValidate = GetPermissionToValidate(operationDescription);
        }

        private Permission[] GetPermissionToValidate(OperationDescription operationDescription)
        {
            var requiresAuthentication = false;
            var permissions = new List<Permission>();

            if (operationDescription.TaskMethod != null)
            {
                var methodPermissions = operationDescription.TaskMethod.GetCustomAttributes(typeof(RequiresPermissionsAttribute)).OfType<RequiresPermissionsAttribute>();

                if (methodPermissions.Any())
                {
                    requiresAuthentication = true;
                    permissions.AddRange(methodPermissions.SelectMany(x => x.Permissions));
                }

                var contractPermissions = operationDescription.TaskMethod.DeclaringType.GetCustomAttributes(typeof(RequiresPermissionsAttribute)).OfType<RequiresPermissionsAttribute>();

                if (contractPermissions.Any())
                {
                    requiresAuthentication = true;
                    permissions.AddRange(contractPermissions.SelectMany(x => x.Permissions));
                }
            }

            if (requiresAuthentication)
                return permissions.Distinct().ToArray();
            else
                return null;
        }


        public object[] AllocateInputs()
        {
            return _innerInvoker.AllocateInputs();
        }

        private static ClaimsIdentity DeserializeViaDataContractSerializer(byte[] serializedUserToken)
        {
            using (var ms = new MemoryStream(serializedUserToken))
            {
                return (ClaimsIdentity)Serializer.ReadObject(ms);
            }
        }

        private static ClaimsIdentity DeserializeViaJson(byte[] serializedUserToken)
        {
            var serializer = new ClaimsIdentitySerializer();

            return serializer.ToClaimsIdentity(Encoding.UTF8.GetString(serializedUserToken));
        }

        private static ClaimsIdentity DeserializeViaCompressedJson(byte[] serializedUserToken)
        {
            var serializer = new ClaimsIdentitySerializer();

            return serializer.ToClaimsIdentity(serializedUserToken);
        }

        private static bool IsAuthorizedToPerformRequest(BaseRequest request, Permission[] permissionsToValidate)
        {
            // If the array is null, then no permissions (not even authentication is required).
            if (permissionsToValidate == null)
                return true;
            // The request requires an authenticated user.
            else if (request.ClaimsIdentity == null)
                return false;
            else
            {
                var isAuthorized = false;
                var allGrantedPermissions = ClaimsUtility.GetClaimValue(request.ClaimsIdentity, AlkamiClaimTypes.AllGrantedPermissions);

                if (!string.IsNullOrEmpty(allGrantedPermissions))
                {
                    var claimMask = Mask<Permission>.FromString(allGrantedPermissions);
                    isAuthorized = claimMask.HasAllPermission(permissionsToValidate);
                }

                return isAuthorized;
            }
        }

        private static Func<BaseResponse> CreateFactory(Type t)
        {
            var paramTypes = new Type[] { };
            var constructor = GetMatchingConstructor(t, paramTypes);
            var newExpression = Expression.New(constructor);
            var lambda = Expression.Lambda(typeof(Func<BaseResponse>), newExpression);
            var result = (Func<BaseResponse>)lambda.Compile();
            return result;
        }

        private static ConstructorInfo GetMatchingConstructor(Type t, Type[] parameterTypes)
        {
            foreach (var constructor in t.GetConstructors())
            {
                var constructorParameters = constructor.GetParameters();

                if (constructorParameters.Length != parameterTypes.Length)
                    continue;

                var valid = true;

                for (int i = 0; i < parameterTypes.Length; ++i)
                {
                    if (!constructorParameters[i].ParameterType.IsAssignableFrom(parameterTypes[i]))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    return constructor;
            }

            throw (new InvalidOperationException(string.Format("Unable to resolve matching constructor for type {0}!", t.AssemblyQualifiedName)));
        }

        internal BaseResponse PerformValidation(object[] inputs)
        {
            var baseRequest = inputs[0] as BaseRequest;

            if (baseRequest == null)
            {
                var result = _faultedOutput();

                result.HasError = true;
                result.SystemMessage = "The request cannot be null.";
                result.ValidationResults = new List<ValidationResult>()
                {
                    new ValidationResult
                    {
                        ErrorCode = ErrorCode.ValidationError,
                        Field = "Request",
                        Message = "The request cannot be null.",
                        Severity = Severity.Fatal,
                        SubCode = SubCode.BadRequest
                    }
                };

                return result;
            }
            else
            {
                try
                {
                    if ((baseRequest.SerializedUserToken != null) && (baseRequest.SerializedUserToken.Length > 0))
                    {
                        switch (baseRequest.ClaimsIdentitySerializationMethod)
                        {
                            default:
                            case ClaimsIdentitySerializationMethod.DataContractSerializer:
                                baseRequest._claimsIdentity = DeserializeViaDataContractSerializer(baseRequest.SerializedUserToken);
                                break;
                            case ClaimsIdentitySerializationMethod.Json:
                                baseRequest._claimsIdentity = DeserializeViaJson(baseRequest.SerializedUserToken);
                                break;
                            case ClaimsIdentitySerializationMethod.CompressedJson:
                                baseRequest._claimsIdentity = DeserializeViaCompressedJson(baseRequest.SerializedUserToken);
                                break;
                        }
                    }
                    else
                    {
                        baseRequest._claimsIdentity = null;
                    }
                }
                catch (Exception e)
                {
                    var result = _faultedOutput();
                    result.HasError = true;
                    result.ValidationResults = new List<ValidationResult>()
                    {
                        new ValidationResult()
                        {
                            ErrorCode = ErrorCode.ValidationError,
                            Field = "SerializedUserToken",
                            Message = e.Message,
                            Severity = Severity.Fatal,
                            SubCode = SubCode.LoginInvalid
                        }
                    };
                    result.SystemMessage = "The claim could not be deserialized.";

                    return result;
                }

                AddBaseRequestPropertiesToNewRelicContext(baseRequest);

#if NET6_0_OR_GREATER
                using var _ = AddBaseRequestPropertiesToLogContext(baseRequest);
#endif
                if ((baseRequest.ClaimsIdentity != null) && !baseRequest.IsAuthenticated())
                {
                    var result = _faultedOutput();
                    result.HasError = true;
                    result.ValidationResults = new List<ValidationResult>()
                    {
                        new ValidationResult()
                        {
                            ErrorCode = ErrorCode.ValidationError,
                            Field = "SerializedUserToken",
                            Message = "The claims identity is not a valid claims identity.",
                            Severity = Severity.Fatal,
                            SubCode = SubCode.LoginInvalid
                        }
                    };
                    result.SystemMessage = "The claim could not be validated.";

                    return result;
                }

                if (!IsAuthorizedToPerformRequest(baseRequest, this.PermissionsToValidate))
                {
                    var result = _faultedOutput();

                    result.HasError = true;
                    result.ValidationResults = new List<ValidationResult>()
                    {
                        new ValidationResult()
                        {
                            ErrorCode = ErrorCode.PermissionError,
                            SubCode = SubCode.AccessDenied,
                            Severity = Severity.Fatal,
                            Message = UnauthorizedAccessErrorMessage,
                        }
                    };
                    result.SystemMessage = UnauthorizedAccessErrorMessage;

                    return result;
                }

                List<ValidationResult> validationResults;

                if (!baseRequest.Validate(out validationResults))
                {
                    // This means the baserequest failed validation... and we need to just return this message
                    var result = _faultedOutput();

                    result.HasError = true;
                    result.ValidationResults = validationResults;
                    result.SystemMessage = string.Format("{0} validation errors prevented the service from being called", validationResults.Count(x => (x.Severity == Severity.Fatal) || (x.Severity == Severity.Error)));

                    return result;
                }
                else
                {
                    return new BaseResponse()
                    {
                        ValidationResults = validationResults
                    };
                }
            }
        }

        private void AddBaseRequestPropertiesToNewRelicContext(BaseRequest baseRequest)
        {
            if (baseRequest == null)
                return;

            AddCustomProperty(baseRequest, baseRequest.UserIdentifier?.ToString(), nameof(BaseRequest.UserIdentifier), AlkamiClaimTypes.UserIdentifier);
            AddCustomProperty(baseRequest, baseRequest.BankIdentifier?.ToString(), nameof(baseRequest.BankIdentifier), AlkamiClaimTypes.BankIdentifier);
            Metric.AddCustomProperty(nameof(BaseRequest.CorrelationId), baseRequest.CorrelationId);
            AddCustomProperty(baseRequest, baseRequest.BankUri, "BankHostName", AlkamiClaimTypes.BankUrlSignature);
            AddCustomProperty(baseRequest, baseRequest.BankInstanceIdentifier?.ToString(), nameof(baseRequest.BankInstanceIdentifier), AlkamiClaimTypes.BankInstanceIdentifier);
        }

        private void AddCustomProperty(BaseRequest baseRequest, string value, string key, string claimType)
        {
            var claimsValue = baseRequest.GetClaimValue(claimType);
            Metric.AddCustomProperty(key, string.IsNullOrWhiteSpace(claimsValue) ? value : claimsValue);
        }

        public Permission[] PermissionsToValidate { get; set; }
    }
}

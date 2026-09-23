using Alkami.Contracts;
using Alkami.MicroServices.Settings.Contracts;
using Alkami.MicroServices.Settings.Contracts.Requests;
using Alkami.MicroServices.Settings.Service.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Alkami.MicroServices.Settings.Contracts.Filters_And_Mappers;
using SettingDescriptor = Alkami.MicroServices.Settings.ProviderBased.Contracts.SettingDescriptor;

namespace Alkami.TrackableObjects.Plugins
{
	public abstract class Plugin : ConfigurationDrivenObject
	{
		/// <summary>
		/// A lazy-loaded cache-store of certificates as they are read from the store
		/// </summary>
		private static readonly ConcurrentDictionary<Tuple<X509FindType, object>, X509Certificate2> Certificates = new ConcurrentDictionary<Tuple<X509FindType, object>, X509Certificate2>();

		/// <summary>
		/// A factory method to create the utility class...
		/// </summary>
		public static Func<Lazy<ISettingsServiceContract>> ServiceContractFactory = () => new Lazy<ISettingsServiceContract>(() => new ServiceClient());


		protected Plugin(string providerType)
		{
			ProviderType = providerType;
		}

		protected Plugin(string providerType, string providerName)
		{
			ProviderType = providerType;
			Name = providerName;
		}

		public string ProviderType { get; private set; }
		
		protected override async Task<long> GetParentIdAsync(BaseRequest parentRequest)
		{
			//We need to get the provider that implements this guy and his id
			var getProviderRequest = new GetProviderTypeRequest
			{
				Filter = new ProviderTypeFilter()
				{
					PartialName = ProviderType
				},
				Mapping = new ProviderTypeMapper()
				{
					ShouldIncludeProviders = true
				}
			};
			
			getProviderRequest.CopyBaseFrom(parentRequest);
			var result = await ServiceContractFactory().Value.GetProviderTypeAsync(getProviderRequest);
			if (result.HasError)
				throw new Exception(result.SystemMessage);

			// If there isnt a providerType / provider combo, we need to create it

			if (!result.ProviderTypes.Any())
			{
				throw new Exception("No Provider Type found from GetProviderTypeAsync");
			}

			// At this point, you have at least one provider

			var type = GetType();
			var provider =
				result.ProviderTypes.SelectMany(x => x.Providers)
					.FirstOrDefault(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) || x.AssemblyInfo == type.AssemblyQualifiedName);

			if (provider == null)
			{
				// We cant define what the provider type is specifically,so we couldnt create one
				throw new Exception("Couldnt find a unique provider type. Cant auto create a provider");
			}
			return provider.Id;
		}

		protected override async Task<long> GetSecondaryIdAsync(BaseRequest parentRequest)
		{
			// we need to get the bank here
			var bankRequest = new GetBanksRequest();
			bankRequest.CopyBaseFrom(parentRequest);

			var result = await ServiceContractFactory().Value.GetBanksAsync(bankRequest);
			if (result.HasError)
				throw new Exception(result.SystemMessage);

			if (result.Banks != null)
			{
				long secondaryId;

				if (parentRequest.BankIdentifier.HasValue)
				{
					secondaryId = result.Banks.First(x => x.BankIdentifier == parentRequest.BankIdentifier).Id;
					return secondaryId;
				}
				else if (!string.IsNullOrWhiteSpace(parentRequest.BankUri))
				{
				    Uri bankUri;
				    string bankHost;

                    // TODO: This should eventually be removed and use BankUri directly.
				    if (Uri.TryCreate(parentRequest.BankUri, UriKind.Absolute, out bankUri))
				        bankHost = bankUri.Host;
				    else
				        bankHost = parentRequest.BankUri;

					secondaryId = result.Banks.First(x => x.UrlSignature.IndexOf(bankHost, 0, StringComparison.InvariantCultureIgnoreCase) > -1 || 
                                                          x.AdminUrlSignature.IndexOf(bankHost, 0, StringComparison.InvariantCultureIgnoreCase) > -1).Id;
					return secondaryId;
				}
			}
			   
			throw new Exception("Couldnt determine bank!");
		}

		/// <summary>
		/// Finds a certificate by the specified criteria.
		/// </summary>
		/// <param name="name">Store name to search.</param>
		/// <param name="location">Store location to search.</param>
		/// <param name="findType">Type of criteria by which to search.</param>
		/// <param name="findValue">Criteria to which to search.</param>
		/// <returns>An instance of the certificate, otherwise throws InvalidOperationException.</returns>
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public X509Certificate2 GetCertificate(StoreName name, StoreLocation location, X509FindType findType, object findValue)
		{
			if (findValue == null)
				throw new ArgumentNullException("findValue");

			if (findType == X509FindType.FindByThumbprint)
			{
				findValue = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(findValue.ToString()))
					.Replace(" ", string.Empty).ToUpperInvariant();
			}

			var storedCert = Certificates.FirstOrDefault(x => x.Key.Item1 == findType && x.Key.Item2 == findValue).Value;

			// If already have a cert, and it hasn't expired, then lets return the version already have. Otherwise go look for a new version.
			if (storedCert != null && !IsCertExpired(storedCert))
				return storedCert;

			var store = new X509Store(name, location);
			store.Open(OpenFlags.ReadOnly);
			var certificates = store.Certificates;

			try
			{
				// ReSharper disable ConditionIsAlwaysTrueOrFalse
				// ReSharper disable HeuristicUnreachableCode
				if (certificates == null)
					throw new InvalidOperationException("Cannot read the certificates from the store!");
				// ReSharper restore HeuristicUnreachableCode
				// ReSharper restore ConditionIsAlwaysTrueOrFalse

				var matchingCerts = certificates.Find(findType, findValue, false);

				if (matchingCerts.Count == 0)
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
						"No certificate was found using search criteria: store location - '{0}', store name - '{1}', x509 find type - '{2}', find value - '{3}'.",
						location, name, findType, findValue));

				var nonExpiredCertificates = matchingCerts.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
				if (nonExpiredCertificates.Count == 0)
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, @"Certificate(s) '{0}' expired '{1}'.", findValue, matchingCerts[0].NotAfter));

				if (nonExpiredCertificates.Count > 1)
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "There is more than one certificate for find type '{0}' with find value '{1}'.", location, name));

				Certificates.AddOrUpdate(new Tuple<X509FindType, object>(findType, findValue), nonExpiredCertificates[0], (key, oldValue) => oldValue);

				return matchingCerts[0];
			}
			finally
			{
				foreach (var cert in certificates.Cast<X509Certificate2>().Where(cert => cert != null))
					cert.Reset();

				store.Close();
			}
		}

		/// <summary>
		/// Determines whether a certificate is expired
		/// </summary>
		/// <param name="cert">The certificate for which to check the expiration date</param>
		/// <returns>True if the certificate is expired</returns>
		[Pure]
		private static bool IsCertExpired(X509Certificate2 cert)
		{
			// All certificate times are local, so compare to local time NOW
			return cert.NotAfter >= DateTime.Now;
		}

		public override List<SettingDescriptor> SettingDescriptors()
		{
			return new List<SettingDescriptor>();
		}

		public override Dictionary<string, string> DefaultSettings()
		{
			return new Dictionary<string, string>();
		}
	}
}
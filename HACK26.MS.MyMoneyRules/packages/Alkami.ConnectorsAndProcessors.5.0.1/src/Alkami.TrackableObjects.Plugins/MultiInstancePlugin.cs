using Alkami.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alkami.MicroServices.Settings.Contracts.Filters_And_Mappers;
using Alkami.MicroServices.Settings.Contracts.Requests;
using Alkami.MicroServices.Settings.Data;

namespace Alkami.TrackableObjects.Plugins
{
    public abstract class MultiInstancePlugin : Plugin
    {
        protected int NumberOfInstances { get; set; }        

        protected MultiInstancePlugin(string providerType) : base(providerType) { }

        protected MultiInstancePlugin(string providerType, string providerName) : base(providerType, providerName) { }

        public async Task<IEnumerable<TenantSpecificScope>> GetScopesAsync(BaseRequest request)
        {
            
            var tasks = new List<Task<TenantSpecificScope>>();
            var providers = await GetProvidersAsync(request);
            foreach(var provider in providers)
            {
                var scope = new TenantSpecificScope(this, request);

                tasks.Add(scope.CreateScopeAsync(provider.Name));
            }
            return await Task.WhenAll(tasks);

        }

        protected sealed override Task<long> GetParentIdAsync(BaseRequest parentRequest)
        {
            return base.GetParentIdAsync(parentRequest);
        }

        /// <summary>
        /// Gets the secondary Id of an item
        /// </summary>
        /// <param name="parentRequest">The request that needs the parent Id</param>
        /// <returns>The parent ID of the item</returns>
        protected async virtual Task<IEnumerable<Provider>> GetProvidersAsync(BaseRequest parentRequest)
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
            var providers =
                result.ProviderTypes.SelectMany(x => x.Providers)
                    .Where(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) || x.Name.EndsWith(":: " + Name, StringComparison.OrdinalIgnoreCase)).ToList();
            NumberOfInstances = providers.Count();

            if (providers == null || !providers.Any())
            {
                // We cant define what the provider type is specifically,so we couldnt create one
                throw new Exception("Couldnt find a unique provider type. Cant auto create a provider");
            }
            return providers;
        }

        public async override Task<ItemFilter> GetFilterAsync(BaseRequest request, string providerName = null)
        {
            var providersTask = GetProvidersAsync(request);
            var secondaryIdsTask = GetSecondaryIdAsync(request);

            var providers = await providersTask;
            var secondaryIds = await secondaryIdsTask;

            return new ItemFilter()
            {
                ItemType = ItemType,
                ParentIds = providers
                    .Where(x => x.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase))
                    .Select(y => y.Id).ToList(),
                SecondaryIds = new List<long>() { secondaryIds }
            };
        }
    }

}

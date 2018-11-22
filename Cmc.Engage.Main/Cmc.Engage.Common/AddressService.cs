using System;
using System.Collections.Generic;
using System.Linq;
using Cmc.Core.Xrm.ServerExtension.Core;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Common.Utilities.Constants;
using Cmc.Engage.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;


namespace Cmc.Engage.Common
{

    public class AddressService: IAddressService
    {
        private IOrganizationService _orgService;

        private readonly ILogger _tracer;
        private readonly IBingMapService _bingMapDetails;
        private readonly ILanguageService _retrieveMultiLingualValues;
        private readonly IConfigurationService _retrieveConfigurationDetails;
        public AddressService(ILogger tracer, IBingMapService bingMapDetails, IOrganizationService orgService, ILanguageService retrieveMultiLingualValues, IConfigurationService retrieveConfigurationDetails)
        {
            _tracer = tracer;
            _bingMapDetails = bingMapDetails;      
            _retrieveMultiLingualValues = retrieveMultiLingualValues;        
            _orgService = orgService;
            _retrieveConfigurationDetails = retrieveConfigurationDetails;
        }          

        #region GeocodeAddress

        public void GeocodeAddress(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            //_orgService = serviceProvider.CreateOrganizationServiceAsCurrentUser();
            if (!pluginContext.IsValidCall("customeraddress"))
                throw new InvalidPluginExecutionException(_retrieveMultiLingualValues.Get("PluginIsNotConfiguredcorrectly"));
            if (pluginContext.MessageName.ToLower() == "create")
                UpdateLatitudeAndLongitudeForCreate(pluginContext.GetTargetEntity<CustomerAddress>(), context);

            if (pluginContext.MessageName.ToLower() == "update")
            {
                var preImage = pluginContext.GetPreEntityImage<CustomerAddress>("Target");
                var postImage = pluginContext.GetPostEntityImage<CustomerAddress>("PostImage");
                UpdateLatitudeAndLongitudeForEdit(postImage, preImage, context);
            }
        }
        /// <summary>
        ///     Updates the latitude and longitude for create.
        /// </summary>
        /// <param name="address">The address.</param>
        private void UpdateLatitudeAndLongitudeForCreate(CustomerAddress address, IExecutionContext context)
        {
            if (address.ParentId == null)
                _tracer.Trace("Address has no customer. Not copying address fields");
            UpdateLatitudeAndLongitude(address, context);
           

        }

        /// <summary>
        ///     Updates the latitude and longitude for edit.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="previousAddress">The previous address.</param>
        private void UpdateLatitudeAndLongitudeForEdit(CustomerAddress address, CustomerAddress previousAddress, IExecutionContext context)
        {
            if (address.ParentId == null)
                _tracer.Trace("Address has no customer. Not copying address fields");

            if (IsAddressChaged(address, previousAddress))
                UpdateLatitudeAndLongitude(address, context);
        }

        private void UpdateLatitudeAndLongitude(CustomerAddress address, IExecutionContext context)
        {
            _tracer.Trace("UpdateLatitudeAndLongitude Method called");

            var fullAddress = PrepareFullAddress(address);
            _tracer.Trace("Address : " + fullAddress);

            var latitudeAndLongitudeDetails = GetLatitudeAndLongitude(fullAddress, context);


            var updateAddress = new CustomerAddress
            {
                CustomerAddressId = address.CustomerAddressId
            };


            if (latitudeAndLongitudeDetails == null)
            {
                updateAddress.Latitude = null;
                updateAddress.Longitude = null;
            }
            else
            {
                updateAddress.Latitude = latitudeAndLongitudeDetails.Latitude;
                updateAddress.Longitude = latitudeAndLongitudeDetails.Longitude;
                _tracer.Trace("latitude : " + latitudeAndLongitudeDetails.Latitude);
                _tracer.Trace("longitude : " + latitudeAndLongitudeDetails.Longitude);
            }

            if (updateAddress.Latitude.Equals(address.Latitude) &&
                updateAddress.Longitude.Equals(address.Longitude))
                return;
            if (address.ParentId !=null && address.ParentId.LogicalName == Contact.EntityLogicalName)
            {
                _tracer.Trace("Updating Contact");

                var parentDetails = _orgService.Retrieve(address.ParentId.LogicalName, address.ParentId.Id, new ColumnSet(true))
                     .ToEntity<Contact>();
                parentDetails.Address1_Latitude = updateAddress.Latitude;
                parentDetails.Address1_Longitude = updateAddress.Longitude;
                _orgService.Update(parentDetails);
            }


            updateAddress.cmc_geocoordinates = cmc_customeraddress_cmc_geocoordinates.Yes;
            UpdateLatitudeAndLongitudeDetailsForPostOperation(updateAddress);
           
        }


        private string PrepareFullAddress(CustomerAddress address)
        {
            //  var countryName = address.cmc_countryid?.GetName(_orgService, "cmc_countryname");
            var countryName = address.County;
            // var stateName = address.cmc_stateid?.GetName(_orgService, "cmc_statename");
            var stateName = address.StateOrProvince;
            var fullAddress = address.Line1 + " " + address.Line2 + " " + address.Line3 + " " +
                              address.City + " " +
                              stateName + " " + countryName + " " + address.PostalCode;
            return fullAddress;
        }

        private static bool IsAddressChaged(CustomerAddress address, CustomerAddress previousAddress)
        {
            //var countryId = address.cmc_countryid?.Id;
            //var stateId = address.cmc_stateid?.Id;
            //var preCountryId = previousAddress.cmc_countryid?.Id;
            //var preStateId = previousAddress.cmc_stateid?.Id;
            var countryName = address.County;
            var stateName = address.StateOrProvince;
            var preCountryName = previousAddress.County;
            var preStateName = previousAddress.StateOrProvince;

            return address.Line1 != previousAddress.Line1 ||
                   address.Line2 != previousAddress.Line2 ||
                   address.Line3 != previousAddress.Line3 ||
                   address.City != previousAddress.City ||
                   address.PostalCode != previousAddress.PostalCode ||
                   countryName != preCountryName ||
                   stateName != preStateName;
        }

        private void UpdateLatitudeAndLongitudeDetailsForPostOperation(Entity relatedAddress)
        {
            _tracer.Trace("UpdateLatAndLongForPostOperation Method called");
            _orgService.Update(relatedAddress);
        }

        private LatitudeLongitude GetLatitudeAndLongitude(string address, IExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;
            return _bingMapDetails.GetCoordinatesFromAddress(address);
        }
        #endregion

        #region CheckAndValidatePrimaryAddress
        public void CheckAndValidatePrimaryAddress(IExecutionContext context)
        {
            var serviceProvider = context.XrmServiceProvider;         
            var pluginContext = serviceProvider.GetPluginExecutionContext();


            if (pluginContext.MessageName.ToLower() == Constants.Create)
            {
                _tracer.Trace("Create of address");
                var input = pluginContext.GetInputParameter<Entity>("Target").ToEntity<CustomerAddress>();
                if (input.mshied_MailType != mshied_customeraddress_mshied_mailtype.Primary)
                {
                    _tracer.Info("New Address is not primary ");
                    return;
                }
                _tracer.Trace("New AddressNumber of address" + input.AddressNumber);
                if (input.ParentId == null)
                {
                    _tracer.Info("Thre is no Parent Id for New Address");
                    return;
                }
                CreateAddress(input);
            }
            if (pluginContext.MessageName.ToLower() == Constants.Update)
            {
                _tracer.Trace("Update of address");
                var input = pluginContext.GetInputParameter<Entity>("Target").ToEntity<CustomerAddress>();
                if (input.mshied_MailType != mshied_customeraddress_mshied_mailtype.Primary)
                {
                    _tracer.Info("Updated Address is not primary ");
                    return;
                }
                _tracer.Trace("Updated AddressNumber of address" + input.AddressNumber);
                var postImage = pluginContext.GetPostEntityImage("Target").ToEntity<CustomerAddress>();
                if (postImage.ParentId == null)
                {
                    _tracer.Info("Thre is no Parent Id for New Address");
                    return;
                }
                UpdateAddress(postImage);
            }
           
        }


        private void CreateAddress(CustomerAddress newPrimaryAddress)
        {
            _tracer.Info("Entered into Create Address");                     
            var parentId = newPrimaryAddress.ParentId;         
            var existingPrimaryAddress = GetRelatedAddressWhichIsPrimary(parentId);
            UpdatePrimaryAddresses(existingPrimaryAddress, newPrimaryAddress);
            if (newPrimaryAddress.ParentId.LogicalName == "account")
            {
                _tracer.Info("Address Parent is " + newPrimaryAddress.ParentId.LogicalName);
                _orgService.Update(new Account
                {
                    AccountId = newPrimaryAddress.ParentId.Id,
                    cmc_accountaddressupdated = cmc_account_cmc_accountaddressupdated.Yes
                });

            }
            _tracer.Info("Address is now set to primary ");
        }

        private void UpdateAddress(CustomerAddress newPrimaryAddress)
        {
            _tracer.Info("Entered into  Update Address");
            var parentId = newPrimaryAddress.ParentId;
            var existingPrimaryAddress = GetRelatedAddressWhichIsPrimary(parentId);
            UpdatePrimaryAddresses(existingPrimaryAddress, newPrimaryAddress);
            _tracer.Info("Address is now set to primary ");
                if (newPrimaryAddress.ParentId.LogicalName == "account")
                {
                    _tracer.Info("Address Parent is "+ newPrimaryAddress.ParentId.LogicalName);
                    _orgService.Update(new Account
                    {
                        AccountId = newPrimaryAddress.ParentId.Id,
                        cmc_accountaddressupdated = cmc_account_cmc_accountaddressupdated.Yes
                    });
               
               }
        }

        private void UpdatePrimaryAddresses(CustomerAddress existingPrimaryAddresses,CustomerAddress newPrimaryAddress)
        {
            _tracer.Info("Entered into Update Primary Addresses");
            _tracer.Info("New Primary Addresses id - "+newPrimaryAddress.Id +" Old primary address id - "+ existingPrimaryAddresses.Id);
            var newAddressNumber = newPrimaryAddress.AddressNumber;
            _tracer.Info("Address Number is "+ newAddressNumber);
            _orgService.Update(new CustomerAddress
            {
                CustomerAddressId = newPrimaryAddress.CustomerAddressId,
                AddressNumber = 0
            });
            _orgService.Update(new CustomerAddress
            {
                CustomerAddressId = existingPrimaryAddresses.CustomerAddressId,
                AddressNumber = newAddressNumber,
                mshied_MailType = null
                
            });
            _orgService.Update(new CustomerAddress
            {
                CustomerAddressId = newPrimaryAddress.CustomerAddressId,
                AddressNumber = 1
            });
            
        }

        private CustomerAddress GetRelatedAddressWhichIsPrimary(EntityReference parent)
        {          
            _tracer.Trace($"customer Id is {parent.Id}");

            var fetch =
                $@"<fetch>
                    <entity name='customeraddress'>
                        <attribute name='addressnumber'/>
                        <attribute name='mshied_mailtype'/>
                        <attribute name='customeraddressid'/>
                        <filter>
                            <condition attribute='parentid' operator='eq' value='{parent.Id}'/>                         
                            <condition attribute='addressnumber' operator='eq' value='1'/>
                        </filter>
                    </entity>
                </fetch>";         
            var relatedAddresses = _orgService.RetrieveMultiple(new FetchExpression(fetch)).Entities.Select(x => x.ToEntity<CustomerAddress>());
            _tracer.Trace($"found {relatedAddresses.Count()} relatedAddresses");
            return relatedAddresses.FirstOrDefault();
        }
       #endregion

        #region AddressGeoCoderLogic

        public void AddressGeoCoderLogic()
        {
            UpdateAddresses();
        }

        public void UpdateAddresses()
        {
            UpdateInvalidAddresses();
            UpdateValidAddresses();
        }

        private void UpdateInvalidAddresses()
        {
            var invalidAddresses = GetInvalidAddresses();
            if (invalidAddresses?.Any() == true) UpdateAddressEntity(invalidAddresses, false);
        }

        private void UpdateValidAddresses()
        {
            cmc_configuration configurationData = _retrieveConfigurationDetails.GetActiveConfiguration();
            if((!configurationData.Contains("cmc_batchgeocodesize") || configurationData.cmc_batchgeocodesize == null) || 
                (!configurationData.Contains("cmc_bingmapapikey") || configurationData.cmc_bingmapapikey == null))
            {
                _tracer.Error("BatchGeocode size not found in Configuration cmc_batchgeocodesize");
                return;
            }
            if(configurationData.Contains("cmc_batchgeocodesize") && configurationData.Contains("cmc_bingmapapikey"))
            {
                var batchGeocodesizevalue = configurationData.cmc_batchgeocodesize;
                var bingMapApiKeyvalue = configurationData.cmc_bingmapapikey;
                if (batchGeocodesizevalue == null || bingMapApiKeyvalue == null) return;
                BatchUpdateForAddresses(bingMapApiKeyvalue, (int)batchGeocodesizevalue);
            }

        }

        private void BatchUpdateForAddresses(string bingMapApiKey, int batchSize)
        {
            while (true)
            {
                var addressesHavingLatitudeAndLongitudeIsNull = GetAddressesHavingLatitudeAndLongitudeIsNull(batchSize);
                if (addressesHavingLatitudeAndLongitudeIsNull == null)
                    return;

                var responseFromBingApi =
                    BingMapService.GetLatitudeAndLogitudeForAddresses(addressesHavingLatitudeAndLongitudeIsNull,
                        bingMapApiKey);

                if (responseFromBingApi == null)
                    return;
                var responsehavingNoLatitudeAndLongitude = new List<CustomerAddress>();
                var responsehavingLatitudeAndLongitude = new List<CustomerAddress>();
                foreach (var response in responseFromBingApi)
                    if (response.Latitude == null
                        && response.Longitude == null)
                    {
                        responsehavingNoLatitudeAndLongitude.Add(response);
                    }
                    else
                    {
                        responsehavingLatitudeAndLongitude.Add(response);
                    }

                if (responsehavingNoLatitudeAndLongitude.Any())
                {
                    UpdateAddressEntity(responsehavingNoLatitudeAndLongitude, false);
                }


                if (responsehavingLatitudeAndLongitude.Any())
                {
                    UpdateAddressEntity(responsehavingLatitudeAndLongitude, true);
                }

            }
        }


        private void UpdateAddressEntity(List<CustomerAddress> addresses, bool geocodeFlag)
        {
            var geocoordinateFlag = cmc_customeraddress_cmc_geocoordinates.No;
            if (geocodeFlag)
                geocoordinateFlag = cmc_customeraddress_cmc_geocoordinates.Yes;

            foreach (var address in addresses)
            {
                address.cmc_geocoordinates = geocoordinateFlag;
            }

            ExecuteBulkEntities.BulkUpdateBatch(_orgService, addresses.Cast<Entity>().ToList());
        }


        private List<CustomerAddress> GetAddressesHavingLatitudeAndLongitudeIsNull(int batchSize)
        {
            var fetch =
                $@" <fetch top='{batchSize}' >
                   <entity name='customeraddress' >
                    <attribute name='stateorprovince' />
                    <attribute name='line2' />
                    <attribute name='line1' />
                    <attribute name='city' />
                    <attribute name='postalcode' />
                    <attribute name='longitude' />
                    <attribute name='country' />
                    <attribute name='latitude' />
                    <attribute name='customeraddressid' />
                    <attribute name='line3' />
                    <filter type='and' >
                      <condition attribute='latitude' operator='null' />
                      <condition attribute='longitude' operator='null' />
                      <condition attribute='cmc_geocoordinates' operator='null' />
                    </filter>
                  </entity>
                </fetch>";
            var addressDetails = _orgService.RetrieveMultiple(new FetchExpression(fetch));
            var data = addressDetails.Entities.Count <= 0 ? null
                : addressDetails.Entities.Cast<CustomerAddress>().ToList();
            return data;
        }

        private List<CustomerAddress> GetInvalidAddresses()
        {
            var fetch = $@" <fetch> 
                  <entity name='customeraddress' >
                    <filter type='and' >
                      <condition attribute='city' operator='null' />
                     <condition attribute='country' operator='null' />
                      <condition attribute='line1' operator='null' />
                      <condition attribute='stateorprovince' operator='null' />
                     <condition attribute='line2' operator='null' />
                      <condition attribute='postalcode' operator='null' />
                      <condition attribute='line3' operator='null' />
                      <condition attribute='cmc_geocoordinates' operator='neq' value='{
                    (int)cmc_customeraddress_cmc_geocoordinates.No
                }' />
                    </filter>
                  </entity>
               </fetch>";
            var addressDetails = _orgService.RetrieveMultipleAll(fetch);
            var data = addressDetails.Entities.Count <= 0 ? null : addressDetails.Entities.Cast<CustomerAddress>().ToList();
            return data;
        }

       



        #endregion

        #region Copy State/Country to Native field
        /// <summary>
        /// Plugin to set Native attribute  Country or State values. 
        /// </summary>
        /// <param name="context"></param>
        public void SetAddressStateorCountry(IExecutionContext context)
        {
            _tracer.Trace("Inside Function to set Address Country or State values.");
            var serviceProvider = context.XrmServiceProvider;
            var pluginContext = serviceProvider.GetPluginExecutionContext();
            CustomerAddress address = pluginContext.GetTargetEntity<CustomerAddress>();
            
            //Sets value in pre operation stage so not saving record in this plugin execution.
            if (address.cmc_country == null && address.StateOrProvince == null){_tracer.Trace("No Update on Country/State");return;}

            if (address.cmc_country != null)
            {
                address.Country = Enum.GetName(typeof(cmc_customeraddress_cmc_country), address.cmc_country);
                _tracer.Trace($"New Country value is {address.Country}");
            }
            if (address.cmc_stateprovince != null)
            {
                address.StateOrProvince = Enum.GetName(typeof(cmc_customeraddress_cmc_stateprovince), address.cmc_stateprovince);
                _tracer.Trace($"New State value is {address.StateOrProvince}");
            }
            
            _tracer.Trace("End of Function setting address Country/State");
        }

        #endregion Copy State/Country to Native field

    }
}

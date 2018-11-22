using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Cmc.Core.Xrm.ServerExtension.Logging;
using IExecutionContext = Cmc.Core.Xrm.ServerExtension.Core.IExecutionContext;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using BingMapsRESTToolkit;
using Cmc.Engage.Common.Messages;
using Cmc.Engage.Common.Utilities;
using Cmc.Engage.Common.Utilities.Constants;
using Cmc.Engage.Models;


namespace Cmc.Engage.Common
{
    public class BingMapService : IBingMapService, IMapService
    {
        private IOrganizationService _orgService;
        private readonly ILogger _tracer;
        private readonly IConfigurationService _retriveConfigurationDetails;
        public BingMapService(ILogger tracer, IOrganizationService organizationService, IConfigurationService retriveConfigurationDetails)
        {
            _tracer = tracer;                           
            _retriveConfigurationDetails = retriveConfigurationDetails;
            _orgService = organizationService;
        }

        /// <summary>
        ///     Gets the coordinates from address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>LatitudeLongitude.</returns>
        public LatitudeLongitude GetCoordinatesFromAddress(string address)
        {

            var keyValue = _retriveConfigurationDetails.GetActiveConfiguration().cmc_bingmapapikey;
            if (string.IsNullOrEmpty(keyValue))
            {
                _tracer.Trace("Bing Map Api Key is null");
                return null;
            }
            _tracer.Info($"Get Coordinates From Address :{address}");
            var geocode = ServiceManager.GetResponseAsync(new BingMapsRESTToolkit.GeocodeRequest
            {
                BingMapsKey = keyValue,
                Query = address
            }).GetAwaiter().GetResult();

            if (geocode != null && geocode.ResourceSets != null &&
                geocode.ResourceSets.Length > 0 &&
                geocode.ResourceSets[0].Resources != null &&
                geocode.ResourceSets[0].Resources.Length > 0)
            {
                var location = (Location)geocode.ResourceSets.First().Resources.First();
                if (location == null || !location.Point.Coordinates.Any()) return null;
                return new LatitudeLongitude
                {
                    Latitude = location.Point.Coordinates[0],
                    Longitude = location.Point.Coordinates[1]
                };
            }

            return null;
        }

        /// <summary>
        /// Gets the distance in miles.
        /// </summary>
        /// <param name="sourceLatitude">The address lat.</param>
        /// <param name="sourceLongitude">The address long.</param>
        /// <param name="destinationLatitude">The campus lat.</param>
        /// <param name="destinationLongitude">The campus long.</param>
        /// <returns>System.Nullable&lt;System.Double&gt;.</returns>
        public double? GetDistanceInMiles(double sourceLatitude, double sourceLongitude, double destinationLatitude, double destinationLongitude, IExecutionContext context)
        {

            var keyValue = _retriveConfigurationDetails.GetActiveConfiguration().cmc_bingmapapikey;
            if (string.IsNullOrEmpty(keyValue)) return null;

            var sourceWaypoint = new SimpleWaypoint(sourceLatitude, sourceLongitude);
            var destinationWaypoint = new SimpleWaypoint(destinationLatitude, destinationLongitude);

            var routeResponse = ServiceManager.GetResponseAsync(new RouteRequest
            {
                Waypoints = new List<SimpleWaypoint> { sourceWaypoint, destinationWaypoint },

                BingMapsKey = keyValue,
                RouteOptions = new RouteOptions
                {
                    DistanceUnits = DistanceUnitType.Miles,
                    TravelMode = TravelModeType.Driving
                }
            }).GetAwaiter().GetResult();


            if (routeResponse != null && routeResponse.ResourceSets != null &&
                routeResponse.ResourceSets.Length > 0 &&
                routeResponse.ResourceSets[0].Resources != null &&
                routeResponse.ResourceSets[0].Resources.Length > 0)
            {
                var route = (Route)routeResponse.ResourceSets.First().Resources.First();
                return route?.TravelDistance;
            }

            return null;
        }


        public static List<CustomerAddress> GetLatitudeAndLogitudeForAddresses(List<CustomerAddress> addressList,
           string bingMapApiKey)
        {
            try
            {
                if (addressList?.Any() != true || string.IsNullOrEmpty(bingMapApiKey)) return null;
                var dataflowJobLocation = CreateJob(bingMapApiKey, addressList);

                //Continue to check the dataflow job status until the job has completed
                DownloadDetails statusDetails;
                var timer = new Stopwatch();
                timer.Start();
                do
                {
                    statusDetails = CheckStatus(dataflowJobLocation, bingMapApiKey);
                    Console.WriteLine(Constants.DataflowJobStatus + statusDetails.JobStatus);
                    if (statusDetails.JobStatus == Constants.Aborted)
                        throw new Exception(ErrorMessages.ErrorMessgae);
                    Thread.Sleep(30000); //Get status every 30 seconds
                } while (statusDetails.JobStatus.Equals(Constants.Pending) &&
                         timer.Elapsed.TotalMinutes < Constants.TotalMinutesForTimeOut);

                timer.Stop();
                //When the job is completed, get the results
                var results = DownloadResults(statusDetails, bingMapApiKey);

                var addressUpdatedList = new List<CustomerAddress>();

                if (results == null)
                {
                    if (!addressList.Any()) return null;
                    addressUpdatedList.AddRange(addressList.Select(item => new CustomerAddress { CustomerAddressId = item.Id }).ToList());
                    return addressUpdatedList;
                }

                if (results.GeocodeEntity.Count != addressList.Count)
                {
                    var invalidAddressIdList = addressList.Where(x => results.GeocodeEntity.All(y => new Guid(y.Id) != x.Id))
                        .Select(x => x.Id).ToList();

                    if (invalidAddressIdList.Any())
                    {
                        addressUpdatedList.AddRange(invalidAddressIdList.Select(item => new CustomerAddress { CustomerAddressId = item }).ToList());
                    }
                }

                foreach (var item in results.GeocodeEntity)
                {
                    var latitudeandLogitudeDetails = GetLatitudeandLogitudeFromDownloadResults(item);
                    if (latitudeandLogitudeDetails == null)
                        addressUpdatedList.Add(new CustomerAddress
                        {
                            CustomerAddressId = new Guid(item.Id)
                        });
                    else
                        addressUpdatedList.Add(new CustomerAddress
                        {
                            CustomerAddressId = new Guid(item.Id),
                            Latitude = latitudeandLogitudeDetails.Latitude,
                            Longitude = latitudeandLogitudeDetails.Longitude
                        });
                }

                return addressUpdatedList;
            }

            catch (Exception e)
            {
                Console.WriteLine(Constants.Exception + e.Message);
            }

            return null;
        }

        private static LatitudeLongitude GetLatitudeandLogitudeFromDownloadResults(GeocodeEntity entity)
        {
            if (entity.StatusCode != Constants.Success) return null;
            if (entity.GeocodeResponse?.GeocodePoint?.Any() != true)
                return null;
            var geocodePointDetails = entity.GeocodeResponse.GeocodePoint.First();
            if (string.IsNullOrEmpty(geocodePointDetails.Latitude) ||
                string.IsNullOrEmpty(geocodePointDetails.Longitude)) return null;
            return new LatitudeLongitude
            {
                Latitude = Convert.ToDouble(geocodePointDetails.Latitude),
                Longitude = Convert.ToDouble(geocodePointDetails.Longitude)
            };
        }

        private static string PrepareFullAddress(CustomerAddress address)
        {
            //var countryName = string.Empty;
            //if (!string.IsNullOrWhiteSpace(address.cmc_countryid?.Name)) countryName = address.cmc_countryid.Name;
            //var stateName = string.Empty;
            //if (!string.IsNullOrWhiteSpace(address.cmc_stateid?.Name)) stateName = address.cmc_stateid.Name;
            //var fullAddress = address.cmc_street1 + " " + address.cmc_street2 + " " + address.cmc_street3 + " " +
            //                  address.cmc_city + " " +
            //                  stateName + " " + countryName + " " + address.cmc_zip;
            //return fullAddress;
          
            var countryName = address.County;           
            var stateName = address.StateOrProvince;
            var fullAddress = address.Line1 + " " + address.Line2 + " " + address.Line3 + " " +
                              address.City + " " +
                              stateName + " " + countryName + " " + address.PostalCode;
            return fullAddress;
        }

        //Creates a geocode dataflow job and uploads spatial data to process.
        //Parameters: 
        //   dataFilePath: The path to the file that contains the spatial data to geocode.
        //   dataFormat: The format of the input data. Possible values are xml, csv, tab and pipe.
        //   key: The Bing Maps Key to use for this job. The same key is used to get job status and download results.
        //   description: Text that is used to describe the geocode dataflow job. 
        //Return value : A URL that defines the location of the geocode dataflow job that was created.
        private static string CreateJob(string key, List<CustomerAddress> addressList)
        {
            var dataFormat = Constants.Xml;
            var xmlOnMemoryStream = WriteXmlOnMemoryStream(addressList);
            var queryStringBuilder = new StringBuilder();
            queryStringBuilder.Append(Constants.InputEqualTo).Append(Uri.EscapeUriString(dataFormat));
            queryStringBuilder.Append(Constants.AndSymbol);
            queryStringBuilder.Append(Constants.KeyEqualTo).Append(Uri.EscapeUriString(key));

            //Build the HTTP URI that will upload and create the geocode dataflow job
            var uriBuilder =
                new UriBuilder(Constants.GeocodeDataFlowUrl)
                {
                    Path = Constants.UrlPath,
                    Query = queryStringBuilder.ToString()
                };

            var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = Constants.Post;
            request.ContentType = Constants.ContentType;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(xmlOnMemoryStream, 0, xmlOnMemoryStream.Length);
            }

            //Submit the HTTP request and check if the job was created successfully. 
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                //
                // If the job was created successfully, the status code should be
                // 201 (Created) and the 'Location' header should contain a URL
                // that defines the location of the new dataflow job. You use this 
                // URL with the Bing Maps Key to query the status of your job.
                //
                if (response.StatusCode != HttpStatusCode.Created)
                    throw new Exception(ErrorMessages.HttpErrorOnCreatingJob);


                var dataflowJobLocation = response.GetResponseHeader(Constants.Location);
                if (string.IsNullOrEmpty(dataflowJobLocation))
                    throw new Exception(ErrorMessages.LocationHeaderError);


                return dataflowJobLocation;
            }
        }

        private static byte[] WriteXmlOnMemoryStream(List<CustomerAddress> addressList)
        {
            var memoryStream = new MemoryStream();
            var xmlTextWriter =
                new XmlTextWriter(memoryStream, Encoding.UTF8);

            xmlTextWriter.WriteStartDocument(true);
            xmlTextWriter.WriteStartElement(Constants.GeocodeFeed);
            xmlTextWriter.WriteAttributeString(Constants.Version, Constants.VersionNumber);

            foreach (var address in addressList)
            {
                xmlTextWriter.WriteStartElement(Constants.GeocodeEntity);
                xmlTextWriter.WriteAttributeString(Constants.Id, address.Id.ToString());

                xmlTextWriter.WriteStartElement(Constants.GeocodeRequest);
                xmlTextWriter.WriteAttributeString(Constants.Query, PrepareFullAddress(address));
                xmlTextWriter.WriteEndElement();
                xmlTextWriter.WriteEndElement();
            }

            xmlTextWriter.WriteEndElement();
            xmlTextWriter.WriteEndDocument();
            xmlTextWriter.Flush();
            xmlTextWriter.Close();
            return memoryStream.ToArray();
        }

        //Checks the status of a dataflow job and defines the URLs to use to download results when the job is completed.
        //Parameters: 
        //   dataflowJobLocation: The URL to use to check status for a job.
        //   key: The Bing Maps Key for this job. The same key is used to create the job and download results.  
        //Return value: A DownloadDetails object that contains the status of the geocode dataflow job (Completed, Pending, Aborted).
        //              When the status is set to Completed, DownloadDetails also contains the links to download the results
        private static DownloadDetails CheckStatus(string dataflowJobLocation, string key)
        {
            var statusDetails = new DownloadDetails { JobStatus = Constants.Pending };

            //Build the HTTP Request to get job status
            var uriBuilder = new UriBuilder(dataflowJobLocation + Constants.QuestionMarkSymbol + Constants.KeyEqualTo +
                                            key + Constants.OutputEqualToXml);
            var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);

            request.Method = Constants.Get;

            //Submit the request and read the response to get job status and to retrieve the links for 
            //  downloading the job results
            //Note: The following conditional statements make use of the fact that the 'Status' field will  
            //  always appear after the 'Link' fields in the HTTP response.
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(ErrorMessages.HttpErrorOnCheckingJobStatus);

                using (var receiveStream = response.GetResponseStream())
                {
                    var reader = new XmlTextReader(receiveStream);
                    while (reader.Read())
                        if (reader.IsStartElement())
                            if (reader.Name.Equals(Constants.Status))
                            {
                                //return job status
                                statusDetails.JobStatus = reader.ReadString();

                                return statusDetails;
                            }
                            else if (reader.Name.Equals(Constants.Link))
                            {
                                //Set the URL location values for retrieving 
                                // successful and failed job results
                                reader.MoveToFirstAttribute();
                                if (reader.Value.Equals(Constants.Output))
                                {
                                    reader.MoveToNextAttribute();
                                    if (reader.Value.Equals(Constants.Succeeded))
                                        statusDetails.SuceededLink = reader.ReadString();
                                    else if (reader.Value.Equals(Constants.Failed))
                                        statusDetails.FailedLink = reader.ReadString();
                                }
                            }
                }
            }

            return statusDetails;
        }

        //Downloads job results  (successfully geocoded results) and     
        //Parameters: 
        //   statusDetails: Inclues job status and the URLs to use to download all geocoded results.
        //   key: The Bing Maps Key for this job. The same key is used to create the job and get job status.   

        private static GeocodeFeed DownloadResults(DownloadDetails statusDetails, string key)
        {
            //Write the results for data that was geocoded successfully to a file named Success.xml
            if (statusDetails.SuceededLink != null && !statusDetails.SuceededLink.Equals(string.Empty))
            {
                //Create a request to download successfully geocoded data. You must add the Bing Maps Key to the 
                //  download location URL provided in the response to the job status request.
                var successUriBuilder =
                    new UriBuilder(statusDetails.SuceededLink + Constants.QuestionMarkSymbol + Constants.KeyEqualTo +
                                   key);
                var request1 = (HttpWebRequest)WebRequest.Create(successUriBuilder.Uri);

                request1.Method = Constants.Get;

                using (var response = (HttpWebResponse)request1.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(ErrorMessages.HttpErrorOndownloadingResult);

                    using (var receiveStream = response.GetResponseStream())
                    {
                        using (var r = new StreamReader(receiveStream))
                        {
                            var text = r.ReadToEnd().Trim();

                            var ser = new XmlSerializer(typeof(GeocodeFeed));

                            //Deserialize and cast to your type of object
                            return (GeocodeFeed)ser.Deserialize(new StringReader(text));
                        }
                    }
                }
            }

            //If some spatial data could not be geocoded, write the error information to a file called Failed.xml
            if (statusDetails.FailedLink != null && !statusDetails.FailedLink.Equals(string.Empty)) return null;
            return null;
        }
    }
}

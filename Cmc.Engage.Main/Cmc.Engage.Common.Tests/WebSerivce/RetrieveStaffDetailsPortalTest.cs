using System;
using System.Collections.Generic;
using Cmc.Core.Xrm.ServerExtension.Logging;
using Cmc.Engage.Common.Plugins.Tests.Utilities;
using Cmc.Engage.Models.Tests;
using FakeXrmEasy;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using Microsoft.Xrm.Sdk.Query;
using System.Reflection;


namespace Cmc.Engage.Common.Tests.WebSerivce
{
    [TestClass]
    public class RetrieveStaffDetailsPortalTest : XrmUnitTestBase
    {

        [TestMethod]
        [TestCategory("WebService"), TestCategory("Positive")]
        public void PortalStaffAvailable_Retrieve_StaffDetails()
        {
            #region Arrange
            var account = CreateAccount();
            var staff = PrepareStaffEntity();
            var location = CreateUserLocation(account.Id, staff.Id);
            var student = CreateStudentEntity();

            var role = PrepareRoleEntity();
            var successNetwork = CreateSuccessNetworkEntity(staff, role, student);
            var officeHour = CreateOfficeHours(location.Id, staff.Id);           
            var officeHourList = new List<cmc_officehours> {
                officeHour
            };

            location.cmc_userlocation_officehours = officeHourList;
            var userLocationlist = new List<cmc_userlocation>
            {
                location
            };

            account.cmc_account_userlocation_accountid = userLocationlist;

            var xrmFakedContext = new XrmFakedContext();
            var data = new List<Entity>();
            data.AddRange(officeHourList);
            data.AddRange(userLocationlist);
            data.Add(account);
            data.Add(officeHour);
            data.Add(role);
            data.Add(student);
            data.Add(staff);
            data.Add(location);
            data.Add(successNetwork);
            xrmFakedContext.Initialize(data);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, successNetwork, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");
           
            #endregion

            #region Act
            
            var mockLogger = new Mock<ILogger>();
            var mockRetrieveStaffDetails = new RetrieveStaffDetailsLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'SuccessNetworkId':'" + successNetwork.Id + "','StudentId':'" + student.Id + "'}";           

            var mockRetrieveStaffDetailsPortal = new RetrieveStaffDetailsPortalLogic(mockLogger.Object, mockRetrieveStaffDetails);

            var result = mockRetrieveStaffDetailsPortal.DoWork(mockExecutionContext.Object, stringInput);

            #endregion

            #region Assert
            Assert.IsNotNull(result);
            #endregion Assert 
        }

        [TestMethod]
        [TestCategory("WebService"), TestCategory("Negative")]
        public void PortalStaffAvailable_Retrieve_StaffDetails_InputIsNull()
        {
            #region Arrange
            var account = CreateAccount();
            var staff = PrepareStaffEntity();
            var location = CreateUserLocation(account.Id, staff.Id);
            var student = CreateStudentEntity();

            var role = PrepareRoleEntity();
            var successNetwork = CreateSuccessNetworkEntity(staff, role, student);
            var officeHour = CreateOfficeHours(location.Id, staff.Id);
            var officeHourList = new List<cmc_officehours> {
                officeHour
            };

            location.cmc_userlocation_officehours = officeHourList;
            var userLocationlist = new List<cmc_userlocation>
            {
                location
            };

            account.cmc_account_userlocation_accountid = userLocationlist;

            var xrmFakedContext = new XrmFakedContext();
            var data = new List<Entity>();
            data.AddRange(officeHourList);
            data.AddRange(userLocationlist);
            data.Add(account);
            data.Add(officeHour);
            data.Add(role);
            data.Add(student);
            data.Add(staff);
            data.Add(location);
            data.Add(successNetwork);
            xrmFakedContext.Initialize(data);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, successNetwork, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");

            #endregion

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockRetrieveStaffDetails = new RetrieveStaffDetailsLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'SuccessNetworkId':'','StudentId':''}";

            var mockRetrieveStaffDetailsPortal = new RetrieveStaffDetailsPortalLogic(mockLogger.Object, mockRetrieveStaffDetails);

            var result = mockRetrieveStaffDetailsPortal.DoWork(mockExecutionContext.Object, stringInput);

            #endregion

            #region Assert
            Assert.IsNull(result);
            #endregion Assert 
        }

        [TestMethod]
        [TestCategory("WebService"), TestCategory("Negative")]
        public void PortalStaffAvailable_Retrieve_StaffDetails_SuccessNetworkIsNull()
        {
            #region Arrange
            var account = CreateAccount();
            var staff = PrepareStaffEntity();
            var location = CreateUserLocation(account.Id, staff.Id);
            var student = CreateStudentEntity();

            var role = PrepareRoleEntity();
            var successNetwork = CreateSuccessNetworkEntity(staff, role, student);
            var officeHour = CreateOfficeHours(location.Id, staff.Id);
            var officeHourList = new List<cmc_officehours> {
                officeHour
            };

            location.cmc_userlocation_officehours = officeHourList;
            var userLocationlist = new List<cmc_userlocation>
            {
                location
            };

            account.cmc_account_userlocation_accountid = userLocationlist;

            var xrmFakedContext = new XrmFakedContext();
            var data = new List<Entity>();
            data.AddRange(officeHourList);
            data.AddRange(userLocationlist);
            data.Add(account);
            data.Add(officeHour);
            data.Add(role);
            data.Add(student);
            data.Add(staff);
            data.Add(location);
            //data.Add(successNetwork);
            xrmFakedContext.Initialize(data);

            var mockServiceProvider = InitializeMockService(xrmFakedContext, successNetwork, Operation.RetrieveMultiple);
            var mockExecutionContext = GetMockExecutionContext(mockServiceProvider);

            xrmFakedContext.ProxyTypesAssembly = Assembly.Load("Cmc.Engage.Contracts.Tests");

            #endregion

            #region Act

            var mockLogger = new Mock<ILogger>();
            var mockRetrieveStaffDetails = new RetrieveStaffDetailsLogic(mockLogger.Object, xrmFakedContext.GetFakedOrganizationService());
            var stringInput = "{'SuccessNetworkId':'" + successNetwork.Id + "','StudentId':'" + student.Id + "'}";           

            var mockRetrieveStaffDetailsPortal = new RetrieveStaffDetailsPortalLogic(mockLogger.Object, mockRetrieveStaffDetails);

            var result = mockRetrieveStaffDetailsPortal.DoWork(mockExecutionContext.Object, stringInput);

            #endregion

            #region Assert
            Assert.IsNull(result);
            #endregion Assert 
        }
        [TestMethod]
        [TestCategory("WebService"), TestCategory("Negative")]
        public void PortalStaffAvailable_Retrieve_StaffDetails_ArgumentNullException()
        {
            #region Arrange
            
            var xrmFakedContext = new XrmFakedContext();
            
            #endregion

            #region Act

            var mockLogger = new Mock<ILogger>();
            
            #endregion

            #region Assert
            Assert.ThrowsException<ArgumentException>(() => new RetrieveStaffDetailsPortalLogic(null, null));
            Assert.ThrowsException<ArgumentException>(() => new RetrieveStaffDetailsPortalLogic(mockLogger.Object, null));
            Assert.ThrowsException<ArgumentNullException>(() => new RetrieveStaffDetailsLogic(null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new RetrieveStaffDetailsLogic(mockLogger.Object, null));
            #endregion Assert 
        }
        private SystemUser PrepareStaffEntity()
        {
            return new SystemUser()
            {
                Id = Guid.NewGuid(),
            };
        }
        private cmc_title PrepareRoleEntity()
        {
            return new cmc_title()
            { Id = Guid.NewGuid() };
        }
        private cmc_successnetwork CreateSuccessNetworkEntity(Entity staff, Entity role, Entity student)
        {
            return new cmc_successnetwork()
            {
                Id = Guid.NewGuid(),
                cmc_staffmemberid = staff.ToEntityReference(),
                cmc_staffroleid = role.ToEntityReference(),
                cmc_studentid = student.ToEntityReference(),
                statecode = cmc_successnetworkState.Active
            };
        }
        private Cmc.Engage.Models.Tests.Contact CreateStudentEntity()
        {
            return new Cmc.Engage.Models.Tests.Contact()
            {
                Id = Guid.NewGuid(),
            };
        }
        private cmc_userlocation CreateUserLocation(Guid guidAccountId, Guid guidStaffId)
        {
            return new cmc_userlocation()
            {
                Id = Guid.NewGuid(),
                cmc_userlocationname = "Account - Abc",
                statecode = cmc_userlocationState.Active,
                cmc_accountid = new EntityReference("Account", guidAccountId),
                cmc_Details = "Location Details",
                cmc_userid = new EntityReference("SystemUser", guidStaffId),

            };
        }
        private cmc_officehours CreateOfficeHours(Guid guidUserLocation, Guid guidSaffMemberId)
        {
            return new cmc_officehours()
            {
                Id = Guid.NewGuid(),
                cmc_starttime = DateTime.Now.AddYears(-100),
                cmc_endtime = DateTime.Now.AddYears(-100),
                cmc_startdate = DateTime.Now.AddDays(-5),
                cmc_enddate = DateTime.Now.AddDays(15),
                cmc_duration = 5,
                cmc_monday = true,
                cmc_tuesday = true,
                cmc_wednesday = true,
                cmc_thursday = true,
                cmc_friday = false,
                cmc_saturday = false,
                cmc_sunday = true,
                statecode = cmc_officehoursState.Active,
                cmc_userlocationid = new EntityReference("cmc_userlocation", guidUserLocation),
                OwnerId = new EntityReference("SystemUser", guidSaffMemberId)
            };
        }       
        private Account CreateAccount()
        {
            return new Account()
            {
                Id = Guid.NewGuid(),
                StateCode = AccountState.Active,
               mshied_AccountType = mshied_account_mshied_accounttype.Campus

            };
        }

    }
}

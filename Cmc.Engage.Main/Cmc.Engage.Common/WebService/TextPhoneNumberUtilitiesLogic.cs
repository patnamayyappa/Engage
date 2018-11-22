using System;
using System.Linq;
using System.Runtime.Serialization;
using Cmc.Core.Xrm.ServerExtension.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Cmc.Engage.Common
{
    public class TextPhoneNumberUtilitieslogic : WebServiceLogicBase
    {
       
        private IOrganizationService _orgService;
        public TextPhoneNumberUtilitieslogic(IOrganizationService orgService)
        {
            _orgService = orgService ?? throw new ArgumentException(nameof(orgService));
        }
        public override object DoWork(ExecutionContext context, string inputData)
        {
            var serviceProvider = context.XrmServiceProvider;
            
            PhoneNumberUtilityInput deserializedInput = GetInput<PhoneNumberUtilityInput>(inputData);
            PhoneNumberUtilityOutput output = new PhoneNumberUtilityOutput();
            if (deserializedInput.Utility == null || deserializedInput.Utility == "")
                return output;

            switch (deserializedInput.Utility)
            {
                case "GetPrimaryStudentNumber":
                    output.StudentNumber = GetPrimarySmsNumberForStudent(deserializedInput.StudentId);
                    break;
                case "GetValidStudentNumber":
                    output.StudentNumber = GetValidSmsNumberForStudent(deserializedInput.StudentId);
                    break;
                case "GetSenderNumber":
                    output.SenderNumber = GetUsersNumberFromSmsConfig(deserializedInput.SenderId, deserializedInput.StudentId);
                    break;
                case "ValidateStudentNumber":
                    output.IsMatch = DoesNumberMatchStudent(deserializedInput.StudentId, deserializedInput.PhoneNumber);
                    break;
                case "PreferenceCheck":
                    output = InternalGetStudentPhoneData(deserializedInput.StudentId, deserializedInput.PhoneNumber);
                    break;
            }

            return output;
        }

        public bool DoesNumberMatchStudent(Guid studentId, String phoneNumber)
        {
            //QueryExpression query = new QueryExpression(hlx_studentcommunicationmapping.EntityLogicalName);
            //query.ColumnSet = new ColumnSet("hlx_communicationfield", "hlx_preferencefield", "hlx_statusfield");
            //query.Criteria.AddCondition("hlx_fieldtype", ConditionOperator.Equal, (int)hlx_studentcommunicationfieldtypes.Phone);
            //query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            //var mappings = orgService.RetrieveMultiple(query).Entities.Cast<hlx_studentcommunicationmapping>().ToList();

            //List<String> fields = new List<string>();
            //foreach (var mapping in mappings)
            //{
            //    fields.Add(mapping.GetAttributeValue<string>("hlx_communicationfield"));
            //    fields.Add(mapping.GetAttributeValue<string>("hlx_preferencefield"));
            //    fields.Add(mapping.GetAttributeValue<string>("hlx_statusfield"));
            //}

            //var columnSet = new ColumnSet((from f in fields
            //                               where f != null
            //                               select f).ToArray());

            //var student = (Contact)orgService.Retrieve(Contact.EntityLogicalName, studentId, columnSet);

            //foreach (var mapping in mappings)
            //{
            //    var field = mapping.GetAttributeValue<string>("hlx_communicationfield");

            //    if (field != null)
            //    {
            //        var fieldValue = student.GetAttributeValue<String>(field);

            //        if (!String.IsNullOrWhiteSpace(fieldValue))
            //        {
            //            if (System.Text.RegularExpressions.Regex.Replace(fieldValue, "[^0-9]", "")
            //             == System.Text.RegularExpressions.Regex.Replace(phoneNumber, "[^0-9]", ""))
            //                return true;
            //        }
            //    }
            //}
            return false;
        }

        public string GetUsersNumberFromSmsConfig(Guid userId, Guid studentId)
        {
            //QueryExpression query = new QueryExpression(hlx_smsconfiguration.EntityLogicalName);
            //query.ColumnSet = new ColumnSet(true);
            //query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            //var uidLink = query.AddLink(hlx_userinstitutiondetail.EntityLogicalName, "hlx_smsconfigurationid", "hlx_smsconfigurationid");
            //uidLink.LinkCriteria.AddCondition("ownerid", ConditionOperator.Equal, userId);
            //uidLink.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            //var studentLink = uidLink.AddLink(Contact.EntityLogicalName, "hlx_institutionid", "hlx_institutionid");
            //studentLink.LinkCriteria.AddCondition("contactid", ConditionOperator.Equal, studentId);

            //hlx_smsconfiguration result = orgService.RetrieveMultiple(query).Entities.Cast<hlx_smsconfiguration>().FirstOrDefault();

            //if (result == null)
            //{
            //    query = new QueryExpression(hlx_smsconfiguration.EntityLogicalName);
            //    query.ColumnSet = new ColumnSet(true);
            //    query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            //    var userLink = query.AddLink(SystemUser.EntityLogicalName, "hlx_smsconfigurationid", "hlx_smsconfigurationid");
            //    userLink.LinkCriteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);

            //    result = orgService.RetrieveMultiple(query).Entities.Cast<hlx_smsconfiguration>().FirstOrDefault();
            //}

            //if (result != null && !String.IsNullOrEmpty(result.hlx_name))
            //{
            //    //not a typo, phone number is stored in the name field
            //    return result.hlx_name;
            //}

            return string.Empty;
        }

        public PhoneNumberData GetStudentPhoneData(Guid studentId, string phoneNumber)
        {
            PhoneNumberData output = new PhoneNumberData();
            //output.Found = false;
            //QueryExpression query = new QueryExpression(hlx_studentcommunicationmapping.EntityLogicalName);
            //query.ColumnSet = new ColumnSet("hlx_communicationfield", "hlx_preferencefield", "hlx_statusfield");
            //query.Criteria.AddCondition("hlx_fieldtype", ConditionOperator.Equal, (int)hlx_studentcommunicationfieldtypes.Phone);
            //query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            //var mappings = orgService.RetrieveMultiple(query).Entities.Cast<hlx_studentcommunicationmapping>().ToList();

            //List<String> fields = new List<string>();
            //foreach (var mapping in mappings)
            //{
            //    fields.Add(mapping.GetAttributeValue<string>("hlx_communicationfield"));
            //    fields.Add(mapping.GetAttributeValue<string>("hlx_preferencefield"));
            //    fields.Add(mapping.GetAttributeValue<string>("hlx_statusfield"));
            //}

            //var columnSet = new ColumnSet((from f in fields
            //                               where f != null
            //                               select f).ToArray());

            //var student = (Contact)orgService.Retrieve(Contact.EntityLogicalName, studentId, columnSet);

            //foreach (var mapping in mappings)
            //{
            //    var field = mapping.GetAttributeValue<string>("hlx_communicationfield");
            //    var preferenceField = mapping.GetAttributeValue<string>("hlx_preferencefield");
            //    var statusField = mapping.GetAttributeValue<string>("hlx_statusfield");

            //    if (field != null && preferenceField != null && statusField != null)
            //    {
            //        var fieldValue = student.GetAttributeValue<String>(field);
            //        var preferenceValue = student.GetAttributeValue<OptionSetValue>(preferenceField);
            //        var statusValue = student.GetAttributeValue<OptionSetValue>(statusField);

            //        if (!String.IsNullOrWhiteSpace(fieldValue))
            //        {
            //            if (System.Text.RegularExpressions.Regex.Replace(fieldValue, "[^0-9]", "")
            //             == System.Text.RegularExpressions.Regex.Replace(phoneNumber, "[^0-9]", ""))
            //            {
            //                output.Found = true;
            //                if (preferenceValue != null)
            //                    output.Preference = preferenceValue.Value;
            //                if (statusValue != null)
            //                    output.Status = statusValue.Value;
            //            }
            //        }
            //    }
            //}
            return output;
        }

        public PhoneNumberUtilityOutput InternalGetStudentPhoneData(Guid studentId, string phoneNumber)
        {
            PhoneNumberUtilityOutput output = new PhoneNumberUtilityOutput();

            //QueryExpression query = new QueryExpression(hlx_studentcommunicationmapping.EntityLogicalName);
            //query.ColumnSet = new ColumnSet("hlx_communicationfield", "hlx_preferencefield", "hlx_statusfield");
            //query.Criteria.AddCondition("hlx_fieldtype", ConditionOperator.Equal, (int)hlx_studentcommunicationfieldtypes.Phone);
            //query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            //var mappings = orgService.RetrieveMultiple(query).Entities.Cast<hlx_studentcommunicationmapping>().ToList();

            //List<String> fields = new List<string>();
            //foreach (var mapping in mappings)
            //{
            //    fields.Add(mapping.GetAttributeValue<string>("hlx_communicationfield"));
            //    fields.Add(mapping.GetAttributeValue<string>("hlx_preferencefield"));
            //    fields.Add(mapping.GetAttributeValue<string>("hlx_statusfield"));
            //}

            //var columnSet = new ColumnSet((from f in fields
            //                               where f != null
            //                               select f).ToArray());

            //var student = (Contact)orgService.Retrieve(Contact.EntityLogicalName, studentId, columnSet);

            //foreach (var mapping in mappings)
            //{
            //    var field = mapping.GetAttributeValue<string>("hlx_communicationfield");
            //    var preferenceField = mapping.GetAttributeValue<string>("hlx_preferencefield");
            //    var statusField = mapping.GetAttributeValue<string>("hlx_statusfield");

            //    if (field != null && preferenceField != null && statusField != null)
            //    {
            //        var fieldValue = student.GetAttributeValue<String>(field);
            //        var preferenceValue = student.GetAttributeValue<OptionSetValue>(preferenceField);
            //        var statusValue = student.GetAttributeValue<OptionSetValue>(statusField);

            //        if (!String.IsNullOrWhiteSpace(fieldValue))
            //        {
            //            if (System.Text.RegularExpressions.Regex.Replace(fieldValue, "[^0-9]", "")
            //             == System.Text.RegularExpressions.Regex.Replace(phoneNumber, "[^0-9]", ""))
            //            {
            //                output.Preference = preferenceValue.Value;
            //                output.Status = statusValue.Value;
            //            }
            //        }
            //    }
            //}
            return output;
        }

        public string GetValidSmsNumberForStudent(Guid studentId)
        {
            var contact = _orgService.RetrieveMultiple(new FetchExpression(
                $@"<fetch count='1'>
                    <entity name='contact'>
                      <attribute name='mobilephone' />
                      <filter>
                        <condition attribute='contactid' operator='eq' value='{studentId}' />
                        <condition attribute='donotphone' operator='eq' value='0' />
                      </filter>
                    </entity>
                  </fetch>")).Entities.FirstOrDefault();

            if (contact == null)
                return null;

            return contact["mobilephone"].ToString();
        }

        public string GetPrimarySmsNumberForStudent(Guid studentId)
        {
            return _orgService.RetrieveMultiple(new FetchExpression(
                $@"<fetch count='1'>
                    <entity name='contact'>
                      <attribute name='mobilephone' />
                      <filter>
                        <condition attribute='contactid' operator='eq' value='{studentId}' />
                      </filter>
                    </entity>
                  </fetch>")).Entities.First()["mobilephone"].ToString();
        }
    }

    [DataContract]
    public class PhoneNumberUtilityInput
    {
        [DataMember(Name = "studentId")]
        public Guid StudentId { get; set; }
        [DataMember(Name = "senderId")]
        public Guid SenderId { get; set; }
        [DataMember(Name = "phoneNumber")]
        public String PhoneNumber { get; set; }
        [DataMember(Name = "phoneType")]
        public String PhoneType { get; set; }
        [DataMember(Name = "utility")]
        public String Utility { get; set; }
    }

    [DataContract]
    public class PhoneNumberData
    {
        [DataMember(Name = "preference")]
        public int Preference { get; set; }
        [DataMember(Name = "status")]
        public int Status { get; set; }
        [DataMember(Name = "found")]
        public bool Found { get; set; }
    }

    [DataContract]
    public class PhoneNumberUtilityOutput
    {
        [DataMember(Name = "studentNumber")]
        public String StudentNumber { get; set; }
        [DataMember(Name = "senderNumber")]
        public String SenderNumber { get; set; }
        [DataMember(Name = "isMatch")]
        public bool IsMatch { get; set; }
        [DataMember(Name = "preference")]
        public int Preference { get; set; }
        [DataMember(Name = "status")]
        public int Status { get; set; }

    }
}

import { Injectable, Input } from "@angular/core";
import { HttpClient } from "@angular/common/http";

declare let SonomaCmc: any;

@Injectable()
export class InternalConnectionsService {
  constructor(private http: HttpClient) { }

  @Input()
  contactId: string;

  getConnections(input) {
    const rwindow: any = window;
    this.contactId = rwindow.parent.Xrm.Page.data.entity.getId().slice(1, -1);
    let fetchXml;
    if (input.toLowerCase() == "allconnection") {
      fetchXml= [
        "<fetch>",
        "    <entity name='connection' >",
        "        <attribute name='record2id' />",
        "        <attribute name='record2roleid' />",
        "        <attribute name='connectionid' />",
        "        <link-entity name='connectionrole' from='connectionroleid' to='record2roleid' visible='false' link-type='outer' alias='connectionRole' >",
        "            <attribute name='name' />",
        "        </link-entity>",
        "        <link-entity name='account' from='accountid' to='record2id' visible='false' link-type='outer' alias='relatedAccountName' >",
        "            <attribute name='name' />",
        "        </link-entity>",
        "        <link-entity name='contact' from='contactid' to='record2id' visible='false' link-type='outer' alias='relatedContactName' >",
        "            <attribute name='fullname' />",
        "        </link-entity>",
        "        <filter type='and' >", //Fetch Active Connections for this Contact
        "             <condition attribute='statecode' operator='eq' value='0' />",
        `             <condition attribute='record1id' operator='eq' uitype='contact' value='${this.contactId}' />`,        
        "             <filter type='or' >",
        "               <filter type='and' >", //Fetch Connections to Account object of type "Campus"
        "                 <condition attribute='record2objecttypecode' operator='eq' value='1' />",             
        "               </filter>",
        "               <filter type='and' >", //Fetch Connections to Contact object
        "                 <condition attribute='record2objecttypecode' operator='eq' value='2' />",
        "               </filter>",
        "             </filter>",
        "        </filter>",
        "    </entity>",
        "</fetch>"
      ].join("");
    }
    else if (input.toLowerCase() == "internalconnection")
    {
      fetchXml = [
        "<fetch>",
        "    <entity name='connection' >",
        "        <attribute name='record2id' />",
        "        <attribute name='record2roleid' />",
        "        <attribute name='connectionid' />",
        "        <link-entity name='connectionrole' from='connectionroleid' to='record2roleid' visible='false' link-type='outer' alias='connectionRole' >",
        "            <attribute name='name' />",
        "        </link-entity>",
        "        <link-entity name='account' from='accountid' to='record2id' visible='false' link-type='outer' alias='relatedAccountName' >",
        "            <attribute name='name' />",
        "        </link-entity>",
        "        <link-entity name='contact' from='contactid' to='record2id' visible='false' link-type='outer' alias='relatedContactName' >",
        "            <attribute name='fullname' />",
        "        </link-entity>",
        "        <filter type='and' >", //Fetch Active Connections for this Contact
        "             <condition attribute='statecode' operator='eq' value='0' />",
        `             <condition attribute='record1id' operator='eq' uitype='contact' value='${this.contactId}' />`,
        "             <condition entityname='connectionrole' attribute='category' operator='eq' value='175490000' />",
        "             <filter type='or' >",
        "               <filter type='and' >", //Fetch Connections to Account object of type "Campus"
        "                 <condition attribute='record2objecttypecode' operator='eq' value='1' />",
        "                 <condition entityname='account' attribute='mshied_accounttype' operator='eq' value='494280000' />",
        "               </filter>",
        "               <filter type='and' >", //Fetch Connections to Contact object
        "                 <condition attribute='record2objecttypecode' operator='eq' value='2' />",
        "               </filter>",
        "             </filter>",
        "        </filter>",
        "    </entity>",
        "</fetch>"
      ].join("");
    }
    var result: any;
      result= SonomaCmc.Promise.all([
      rwindow.parent.Xrm.WebApi
        .retrieveMultipleRecords("connection", `?fetchXml=${fetchXml}`)
    ]);
    console.log(result);
    return result;
  }
}

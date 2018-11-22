[CmdletBinding(SupportsShouldProcess=$false)]
Param (
    [string]$exportUrl,
    [string]$exportUserName,
    [Parameter(Mandatory = $false)][string]$exportPassword,
    [Parameter(Mandatory = $false)][string]$exportClientId,
    [Parameter(Mandatory = $false)][string]$exportRedirectUri,
    [Parameter(Mandatory = $false)][string]$exportTokenCacheStorePath,
    [Parameter(Mandatory = $false)][bool]$exportOAuth = $false,
    [string]$importUrl,
    [string]$importUserName,
    [Parameter(Mandatory = $false)][string]$importPassword,
    [Parameter(Mandatory = $false)][string]$importClientId,
    [Parameter(Mandatory = $false)][string]$importRedirectUri,
    [Parameter(Mandatory = $false)][string]$importTokenCacheStorePath,
    [Parameter(Mandatory = $false)][bool]$importOAuth = $false,
    [string]$destinationBusinessUnitId,
    [Parameter(Mandatory = $false)][bool]$processQueues = $true,
    [Parameter(Mandatory = $false)][bool]$processTeams = $true,
    [Parameter(Mandatory = $false)][bool]$processDupeDetect = $true,
	[Parameter(Mandatory = $false)][Guid]$processDefaultThemeId = 'bdb52ed7-925d-e811-a957-000d3a18c42e' #CampusNexus Enagage Theme
)

#Business Unit GUIDS
#$destinationBusinessUnitId = "18546793-BA87-E711-8110-C4346BDCF161" #QAPortal
#$destinationBusinessUnitId = "{BF66DBAB-A6AD-E711-A94E-000D3A1A7A9B}" #Dev9, UAT9
#$destinationBusinessUnitId = "{B2C5F44D-A7AD-E711-A82A-000D3A33A9A3}" #QA9
#$destinationBusinessUnitId = "{C7733681-E3B3-E711-A952-000D3A37027A}" #projectengage

function buildConnectionString([string]$url, [string]$userName, [string]$password) {
    return "AuthType=Office365;Url=$($url);Username=$($username);Password=$($password);RequireNewInstance=true";
}

function buildOAuthConnectionString([string]$url, [string]$clientId, [string]$redirectUri, [string]$tokenCacheStorePath, [string]$userName) {
    return "AuthType=OAuth;Url=$($url);ClientId=$($clientId);RedirectUri=$($redirectUri);TokenCacheStorePath=$($tokenCacheStorePath);RequireNewInstance=true;LoginPrompt=Auto;Username=$($userName);";
}

Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
$ErrorActionPreference = "Stop"

try
{
    Import-Module $PSScriptRoot\Microsoft.Xrm.Data.PowerShell\Microsoft.Xrm.Data.PowerShell.psd1
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls12

    $EMsettings = [Microsoft.Xrm.Sdk.ExecuteMultipleSettings]::new()
    $EMsettings.ContinueOnError = $true
    $EMsettings.ReturnResponses = $true

    $exportConnectionString = "";
    if ($exportOAuth -eq $false) {
        $exportConnectionString = buildConnectionString $exportUrl $exportUserName $exportPassword
    }
    else {
        $exportConnectionString = buildOAuthConnectionString $exportUrl $exportClientId $exportRedirectUri $exportTokenCacheStorePath $exportUserName
    }

    $sourceConn = Get-CrmConnection -ConnectionString $exportConnectionString
    Write-Output ("Source: " + $sourceConn.ConnectedOrgFriendlyName)

    $importConnectionString = "";
    if ($importOAuth -eq $false) {
        $importConnectionString = buildConnectionString $importUrl $importUserName $importPassword
    }
    else {
        $importConnectionString = buildOAuthConnectionString $importUrl $importClientId $importRedirectUri $importTokenCacheStorePath $importUserName
    }

    $destinationConn = Get-CrmConnection -ConnectionString $importConnectionString
    Write-Output ("Destination: " + $destinationConn.ConnectedOrgFriendlyName)

    [Microsoft.Xrm.Sdk.OptionSetValue] $typeValue = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)

    #*********************************************   Teams
    Write-Output "Process Team model and values: " $processTeams
    if ($processTeams -eq $true)
    {
        try {
            $teamFetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='team'>
                    <attribute name='description' />
                    <attribute name='emailaddress' />
                    <attribute name='name' />
                    <attribute name='teamid' />
                    <attribute name='teamtype' />
                    <attribute name='traversedpath' />
                    <filter type='and'>
                    <condition attribute='name' operator='in'>
                        <value>Student Portal Cases</value>
                    </condition>
                    </filter>
                </entity>
                </fetch>"
    
            $teams = Get-CrmRecordsByFetch -conn $sourceConn -Fetch $teamFetch

            Write-Output "Teams to process: " $teams.CrmRecords.Count
            $upsertRequests = [Microsoft.Xrm.Sdk.OrganizationRequestCollection]::new()

            foreach ($rec in $teams.CrmRecords)
            {
                $target = [Microsoft.Xrm.Sdk.Entity]::new($rec.logicalname)
                $target.Id = $rec.teamid
                Write-Output $rec.name
                $target.Attributes.Add("name", $rec.name)
                $target.Attributes.Add("description", $rec.description)
                $target.Attributes.Add("emailaddress", $rec.emailaddress)
                $target.Attributes.Add("teamtype", $rec.teamtype)
                $target.Attributes.Add("traversedpath", $rec.traversedpath)
                $target.Attributes.Add("businessunitid", ([Microsoft.Xrm.Sdk.EntityReference]::new("businessunit", [Guid]::new($destinationBusinessUnitId))))


                $upsertReq = [Microsoft.Xrm.Sdk.Messages.UpsertRequest]::new()
                $upsertReq.Target = $target
                $upsertRequests.Add($upsertReq)

                $upsertResponse = $destinationConn.Execute($upsertReq)
            }

            Write-Output 'Associating Teams with Roles (Errors may indicate the role is already assigned)'
            $teamRoleFetch = "<fetch count='5000' aggregate='false' distinct='false' mapping='logical'>
                <entity name='teamroles'>
                <attribute name='teamid' />
                <attribute name='roleid' />
                <link-entity name='team' to='teamid' from='teamid' link-type='inner'>
                    <filter>
                    <condition attribute='name' operator='in'>
                        <value>Student Portal Cases</value>
                    </condition>
                    </filter>
                </link-entity>
                </entity>
            </fetch>"

            $teamRoles = Get-CrmRecordsByFetch -conn $sourceConn -Fetch $teamRoleFetch
            foreach ($rec in $teamRoles.CrmRecords)
            {
                try 
                {
                    $associateReq = [Microsoft.Xrm.Sdk.Messages.AssociateRequest]::new()
                    $associateReq.Target = ([Microsoft.Xrm.Sdk.EntityReference]::new("team", $rec.teamid))
                    $associateReq.Relationship = "teamroles_association"
                    $associateReq.RelatedEntities = [Microsoft.Xrm.Sdk.EntityReferenceCollection]::new()
                    $associateReq.RelatedEntities.Add(([Microsoft.Xrm.Sdk.EntityReference]::new("role", $rec.roleid)));
                    $response = $destinationConn.Execute($associateReq);
                }
                catch 
                {
                    Write-Output -Message ("Error associating a Team and Security Role: " + $_.Exception.Message + " " + $_.InvocationInfo.ScriptLineNumber + ":" + $_.InvocationInfo.OffsetInLine);
                }
            }
        }
        catch
        {
            Write-Output "Error updating Teams:"
            Write-Output $_.Exception.Message;
            Write-Output $CRMConn.LastCrmExcept
			if ($CRMConn.LastCrmException)
            {
                throw $CRMConn.LastCrmException
            }
            else {
                throw $_.Exception
            }
        }
    }

    #*********************************************   Queues
    Write-Output "Process Queues: " $processQueues
    if ($processQueues -eq $true)
    {
        try {
            $queueFetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='queue'>
                    <attribute name='name' />
                    <attribute name='emailaddress' />
                    <attribute name='queueviewtype' />
                    <attribute name='queueid' />
                    <attribute name='emailusername' />
                    <attribute name='emailrouteraccessapproval' />
                    <attribute name='outgoingemaildeliverymethod' />
                    <attribute name='defaultmailbox' />
                    <attribute name='incomingemaildeliverymethod' />
                    <attribute name='description' />
                    <attribute name='ignoreunsolicitedemail' />
                    <attribute name='incomingemailfilteringmethod' />
                    <attribute name='allowemailcredentials' />
                    <attribute name='ownerid' />
                    <filter type='and'>
                    <condition attribute='statecode' operator='eq' value='0' />
                    <condition attribute='name' operator='in'>
                        <value>Student Portal Cases</value>
                    </condition>
                    </filter>
                </entity>
                </fetch>"
    
            $queues = Get-CrmRecordsByFetch -conn $sourceConn -Fetch $queueFetch

            Write-Output "Queues to process: " $queues.CrmRecords.Count
            $upsertRequests = [Microsoft.Xrm.Sdk.OrganizationRequestCollection]::new()

            foreach ($rec in $queues.CrmRecords)
            {
                $target = [Microsoft.Xrm.Sdk.Entity]::new($rec.logicalname)
                $target.Id = $rec.queueid
                Write-Output $rec.name
                $target.Attributes.Add("name", $rec.name)
                $target.Attributes.Add("emailaddress", $rec.emailaddress)
                
                #***   IF property value fails, check for nulls   ***
                $typeValue = New-Object Microsoft.Xrm.Sdk.OptionSetValue($rec.queueviewtype_Property.Value.Value)
                $target.Attributes.Add("queueviewtype", $typeValue)
                $target.Attributes.Add("emailusername", $rec.emailusername)
                $typeValue = New-Object Microsoft.Xrm.Sdk.OptionSetValue($rec.emailrouteraccessapproval_Property.Value.Value)
                $target.Attributes.Add("emailrouteraccessapproval", $typeValue)
                $typeValue = New-Object Microsoft.Xrm.Sdk.OptionSetValue($rec.outgoingemaildeliverymethod_Property.Value.Value)
                $target.Attributes.Add("outgoingemaildeliverymethod", $typeValue)
                $target.Attributes.Add("defaultmailbox", $rec.defaultmailbox)
                $typeValue = New-Object Microsoft.Xrm.Sdk.OptionSetValue($rec.incomingemaildeliverymethod.Value.Value)
                $target.Attributes.Add("incomingemaildeliverymethod", $typeValue)
                $target.Attributes.Add("description", $rec.description)
                $target.Attributes.Add("ignoreunsolicitedemail", $rec.ignoreunsolicitedemail_Property.Value)
                $typeValue = New-Object Microsoft.Xrm.Sdk.OptionSetValue($rec.incomingemailfilteringmethod_Property.Value.Value)
                $target.Attributes.Add("incomingemailfilteringmethod", $typeValue)
                $target.Attributes.Add("allowemailcredentials", $rec.allowemailcredentials_Property.Value)

                if ($rec.ownerid -and $rec.ownerid_property.Value.LogicalName -eq "team")
                {
                    $target.Attributes.Add("ownerid", $rec.ownerid_property.Value);
                }

                $upsertReq = [Microsoft.Xrm.Sdk.Messages.UpsertRequest]::new()
                $upsertReq.Target = $target
                $upsertRequests.Add($upsertReq)

                $upsertResponse = $destinationConn.Execute($upsertReq)
            }
        }
        catch
        {
            Write-Output "Error updating queues:"
            Write-Output $CRMConn.LastCrmException
            throw $CRMConn.LastCrmException
        }
    }


    #*********************************************   Duplicate Detection Rules
    Write-Output "Process Duplicate Detection Rules: " $processDupeDetect
    if ($processDupeDetect -eq $true)
    {
        try {
            $dupeRuleFetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                <entity name='duplicaterule'>
                    <attribute name='duplicateruleid' />
                    <attribute name='statuscode' />
                    <attribute name='name' />
                    <filter type='or'>
                    <condition attribute='name' operator='eq' value='Accounts with the same Account Name' />                
                    <condition attribute='name' operator='eq' value='Accounts with the same e-mail address' />
                    <condition attribute='name' operator='eq' value='Accounts with the same phone number' />
                    <condition attribute='name' operator='eq' value='Accounts with the same website' />
                    <condition attribute='name' operator='eq' value='Contacts with the same business phone number' />
                    <condition attribute='name' operator='eq' value='Contacts with the same e-mail address' />
                    <condition attribute='name' operator='eq' value='Contacts with the same first name and last name' />
                    <condition attribute='name' operator='eq' value='Department with same name' />
                    <condition attribute='name' operator='eq' value='Leads with the same e-mail address' />
                    <condition attribute='name' operator='eq' value='Office Hours with same User Location and Start Date' />
                    <condition attribute='name' operator='eq' value='Social profiles with same full name and social channel' />
                    <condition attribute='name' operator='eq' value='Staff Survey Template with the same name' />
					<condition attribute='name' operator='eq' value='Success Plan Template with the same name' />
                    <condition attribute='name' operator='eq' value='Success Plan Todo Template with the same name' />
                    </filter>
                </entity>
                </fetch>"

            $destinationRules = Get-CrmRecordsByFetch -conn $destinationConn -Fetch $dupeRuleFetch
            Write-Output ("destination Rules found: " + $destinationRules.CrmRecords.Count)

            foreach($Rule in $destinationRules.CrmRecords)
            {
                if($Rule.statuscode_Property.Value -eq 1)
                {
                    Write-Output ("Duplicate Detection Rule '" + $Rule.name + "' (" + $Rule.duplicateruleid + ") is still in the process of publishing, skipping...")
                }
                else
                {
                    if($Rule.statuscode_Property.Value -eq 2)
                    {
                        Write-Output ("Duplicate Detection Rule '" + $Rule.name + "' (" + $Rule.duplicateruleid + ") is already enabled in the system.")
                    }
                    else
                    {
                        Write-Output ("Publishing rule " + $Rule.name + " in destination")
                        $DuplicateRuleRequest = New-Object -TypeName Microsoft.Crm.Sdk.Messages.PublishDuplicateRuleRequest
                        $DuplicateRuleRequest.DuplicateRuleId = $Rule.duplicateruleid
                        $result = $destinationConn.Execute($DuplicateRuleRequest)
                    }
                }
            }
        }
        catch
        {
            Write-Output "Error processing duplicate detection rules:"
            Write-Output $CRMConn.LastCrmException
            throw $CRMConn.LastCrmException
        }
    }

	#*********************************************   publishing Campus Nexus Brand Publish 
	Write-Output "Process of Publish Theme  for themeid is : " $processDefaultThemeId
	if ($processDefaultThemeId -ne $null)
	{
		try {
            $publishTheme=$false
			# fetch the themes from the Source
			$themeFetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
			    <entity name='theme'>
			        <attribute name='themeid' />
			        <attribute name='isdefaulttheme' />
					<attribute name='name' />
					<attribute name='logoid' />
					<attribute name='logotooltip' />
					<attribute name='navbarbackgroundcolor' />
					<attribute name='navbarshelfcolor' />
					<attribute name='maincolor' />
					<attribute name='accentcolor' />
					<attribute name='globallinkcolor' />
					<attribute name='selectedlinkeffect' />
					<attribute name='hoverlinkeffect' />
					<attribute name='processcontrolcolor' />
					<attribute name='defaultentitycolor' />
					<attribute name='defaultcustomentitycolor' />
			        <attribute name='controlshade' />
					<attribute name='controlborder' />
					<attribute name='pageheaderbackgroundcolor' />
					<attribute name='panelheaderbackgroundcolor' />
					<attribute name='headercolor' />
			        <filter type='or'>
						<condition attribute='themeid' operator='eq' value='"+$processDefaultThemeId+"' />                 
			        </filter>
			    </entity>
			    </fetch>"
			
			$themes = Get-CrmRecordsByFetch -conn $destinationConn -Fetch $themeFetch
			Write-Output ("Theme id : "+$processDefaultThemeId+" found (at Destination Orgination) : " + $themes.CrmRecords.Count)
			
			foreach($theme in $themes.CrmRecords)
			{
			    if($theme.isdefaulttheme -eq "Yes")
			    {
			        Write-Output ("Theme id :"+$processDefaultThemeId+" Published as Default theme already. value of theme.isdefaulttheme is " + $theme.isdefaulttheme)
					Break
			    }
                $publishTheme = $true;
			}
			
			#theme not exists in the destination location. add the theme and publish
			if($themes.CrmRecords.Count -eq 0)
			{
				Write-Output ("Theme id "+$processDefaultThemeId+" not found at desitination org , writing theme from the source.")
							
				$sourceThemes = Get-CrmRecordsByFetch -conn $sourceConn -Fetch $themeFetch
				Write-Output ("Campus branding theme found (at Source Orgination) : " + $themes.CrmRecords.Count)
				foreach ($rec in $sourceThemes.CrmRecords)
				{
					$target = [Microsoft.Xrm.Sdk.Entity]::new("theme")
					Write-Output ("Adding the theme "+$rec.name)
                    $target.Attributes.Add("themeid", $processDefaultThemeId)
					$target.Attributes.Add("name", $rec.name)
					$target.Attributes.Add("logotooltip", $rec.logotooltip)
					$target.Attributes.Add("logoid", ([Microsoft.Xrm.Sdk.EntityReference]::new("webresource", $rec.logoid_Property.value.Id)))
					$target.Attributes.Add("navbarbackgroundcolor", $rec.navbarbackgroundcolor)
					$target.Attributes.Add("navbarshelfcolor", $rec.navbarshelfcolor)
					$target.Attributes.Add("maincolor", $rec.maincolor)
					$target.Attributes.Add("accentcolor", $rec.accentcolor)
					$target.Attributes.Add("globallinkcolor", $rec.globallinkcolor)
					$target.Attributes.Add("selectedlinkeffect", $rec.selectedlinkeffect)
					$target.Attributes.Add("hoverlinkeffect", $rec.hoverlinkeffect)
					$target.Attributes.Add("processcontrolcolor", $rec.processcontrolcolor)
					$target.Attributes.Add("defaultentitycolor", $rec.defaultentitycolor)
					$target.Attributes.Add("defaultcustomentitycolor", $rec.defaultcustomentitycolor)
					$target.Attributes.Add("controlshade", $rec.controlshade)
					$target.Attributes.Add("controlborder", $rec.controlborder)
					$target.Attributes.Add("pageheaderbackgroundcolor", $rec.pageheaderbackgroundcolor)
					$target.Attributes.Add("panelheaderbackgroundcolor", $rec.panelheaderbackgroundcolor)
					$target.Attributes.Add("headercolor", $rec.headercolor)
			                    
			        $upsertReq = [Microsoft.Xrm.Sdk.Messages.UpsertRequest]::new()
			        $upsertReq.Target = $target
			                    
			        $upsertResponse = $destinationConn.Execute($upsertReq)
			        $publishTheme = $true
			        Write-Output ("Completed Adding the theme "+$rec.name + " is Successfull.")
				}
			}
			# publishing the theme as default.
			if ($processDefaultThemeId -ne $null -and $publishTheme -eq $true){
				Write-Output ("Publishing theme id "+$processDefaultThemeId+" as Default Theme in destination")
			    $PublishThemeRequest = New-Object -TypeName Microsoft.Crm.Sdk.Messages.PublishThemeRequest
			    $PublishThemeRequest.Target = ([Microsoft.Xrm.Sdk.EntityReference]::new("theme", $processDefaultThemeId))
			    $result = $destinationConn.Execute($PublishThemeRequest)
				Write-Output ("Published theme id "+$processDefaultThemeId+" as Default Theme Successfull.")
			}
		}
		catch
		{
			Write-Output "Error processing publishing a theme "+$processDefaultThemeId+" as default."
			Write-Output $CRMConn.LastCrmException
			throw $CRMConn.LastCrmException
		}
    }
	
}
Catch
{
    $errorMessage = $_.Exception.Message
    Write-Host "$(Get-Date) - Default Data script error: $errorMessage" -ForegroundColor Red
    
    if ($_.Exception.InnerException) { 
        Write-Host ("$(Get-Date) - Inner Exception: {0}" -f $_.Exception.InnerException.Message) -ForegroundColor Red
    }

    if ($_.Exception.Source) { 
        Write-Host ("$(Get-Date) - Source: {0}" -f $_.Exception.Source) -ForegroundColor Red
    }

    if ($_.Exception.StackTrace) { 
        Write-Host ("$(Get-Date) - StackTrace: {0}" -f $_.Exception.StackTrace) -ForegroundColor Red
    }

    Break
}
Finally
{
    $ErrorActionPreference = $old_ErrorActionPreference
}

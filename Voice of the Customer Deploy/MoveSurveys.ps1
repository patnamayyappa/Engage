Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls12

function Get-TimeStamp {    
    return "[{0:MM/dd/yy} {0:HH:mm:ss}]" -f (Get-Date)    
}

function Log($Message) {
    #Currently logs to console ONLY (no pipe), can expand upon
    Write-Host "$(Get-TimeStamp) $Message";
}

function ConnectToCRM($ConnectionString) {
    Try {
        Log "Connecting to CRM Organization"
        $CrmConnection = Get-CrmConnection -ConnectionString $ConnectionString

        if ($CrmConnection -and $CrmConnection.GetMyCrmUserId()) {
            Log ("Connected to CRM Organization '" + $CrmConnection.ConnectedOrgUniqueName + "'.")
        }
        else {
            Write-Error -Message ("Could not connect to CRM Organization: " + $CrmConnection.LastCrmException)
            throw
        }
    }
    Catch {
        Write-Error -Message ("Error in the CRM Connection task: " + $_.Exception.Message);
        throw
    }

    $CrmConnection
}

function GetMetadata($CrmConnection, $Entities) {
    $Null = @(Try {
            Log "Retrieving entity metadata"
            $allMetadata = Get-CrmEntityAllMetadata -conn $CrmConnection -OnlyPublished $false -EntityFilters All

            $metadata = New-Object System.Collections.ArrayList

            Foreach ($meta in $allMetadata) {
                if ($Entities -match $meta.LogicalName) {
                    $null = $metadata.Add($meta);
                }
            }
        }
        Catch {
            Write-Error -Message ("Error in the retrieve metadata task: " + $_.Exception.Message);
            throw
        })

    $metadata
}

function ProcessEntityRecords($EntityName, $Records, $SourceMetadata, $TargetMetadata) {
    $Null = @(
        $sourceEntityMetadata = $SourceMetadata | Where-Object {$_.LogicalName -eq $EntityName} | Select-Object -First 1

        if (!$sourceEntityMetadata) {
            Log "No metadata found for $EntityName in source. Nothing to process"
            return;
        }

        $targetEntityMetadata = $TargetMetadata | Where-Object {$_.LogicalName -eq $EntityName} | Select-Object -First 1

        if (!$targetEntityMetadata) {
            Log "No metadata found for $EntityName in target. Nothing to process"
            return;
        }

        $sourceInvalidAttributes = $sourceEntityMetadata.Attributes | Where-Object {!$_.IsValidForCreate -and !$_.IsValidForUpdate} | Select-Object -Property LogicalName -ExpandProperty LogicalName

        $targetInvalidAttributes = $targetEntityMetadata.Attributes | Where-Object {!$_.IsValidForCreate -and !$_.IsValidForUpdate} | Select-Object -Property LogicalName -ExpandProperty LogicalName

        $invalidAttributes = $sourceInvalidAttributes + $targetInvalidAttributes

        $allInvalidAttributes = New-Object System.Collections.ArrayList
        $allInvalidAttributes.Add("ownerid")
        $allInvalidAttributes.Add("ownerid_Property")

        Foreach ($attr in $invalidAttributes) {
            $allInvalidAttributes += ($attr + "_Property")
        } 

        $allInvalidAttributes += $invalidAttributes
        $Records = $Records | Select-Object * -ExcludeProperty $allInvalidAttributes)
    return $Records
}

function GetEntityRecords($CrmConnection, $Entities, $SourceMetadata, $TargetMetadata) {
    $Null = @(Try {
            Log "Retrieving entity records"

            $records = New-Object 'System.Collections.Generic.List[System.Management.Automation.PSObject]'
            $allRecords = New-Object System.Collections.ArrayList

            Foreach ($entity in $Entities) {
                if ($entity -eq "annotation") {
                    $fetchxml = "
                        <fetch>
                          <entity name='annotation'>
                            <all-attributes />
                            <order attribute='modifiedon' />
                            <link-entity name='msdyn_survey' from='msdyn_surveyid' to='objectid'>
                              <filter >
                                <condition attribute='createdon' operator='not-null' />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>"

                    $records = Get-CrmRecordsByFetch -conn $CrmConnection -Fetch $fetchxml -AllRows
                } 
                else {
                    $records = Get-CrmRecords -conn $CrmConnection -EntityLogicalName $entity -Fields * -AllRows 
                }

                if ($records -and $records.CrmRecords) {
                    $processedRecords = ProcessEntityRecords $entity $records.Get_Item("CrmRecords") $SourceMetadata $TargetMetadata
                    if (!$processedRecords) {
                        Log "There were no $entity records to process"
                    }
                    elseif ($processedRecords -isnot [array]) {
                        $allRecords.Add($processedRecords);
                    }
                    else {
                        $allRecords.AddRange($processedRecords);
                    } 
                }
            }
        }
        Catch {
            Write-Error -Message ("Error in the retrieve records task: " + $_.Exception.Message);
            throw
        })

    return $allRecords
}

function GetNNRecords($CrmConnection, $EntityRecords, $SourceMetadata, $TargetMetadata) {
    $Null = @(Try {
            Log "Retrieving N:N relationship records"

            $nnRelationships = New-Object System.Collections.ArrayList
            $nnRecords = New-Object System.Collections.ArrayList
            
            $sourceRelationships = $SourceMetadata | Select-Object -Property ManyToManyRelationships -ExpandProperty ManyToManyRelationships

            $targetRelationships = $TargetMetadata | Select-Object -Property ManyToManyRelationships -ExpandProperty ManyToManyRelationships

            Foreach ($relationship in $sourceRelationships) {
                if ($targetRelationships.IntersectEntityName -contains $relationship.IntersectEntityName -and $nnRelationships.IntersectEntityName -notcontains $relationship.IntersectEntityName -and ($SourceMetadata.LogicalName -contains $relationship.Entity2LogicalName -or $SourceMetadata.LogicalName -contains $relationship.Entity1LogicalName)) {
                    $nnRelationships.Add($relationship);
                }
            }

            Foreach ($relationship in $nnRelationships) {
                $ids = $EntityRecords | Where-Object {$_.LogicalName -eq $relationship.Entity1LogicalName } | Foreach {$r = $_; $_} | Select-Object -ExpandProperty EntityReference | Select-Object -Property Id -ExpandProperty Id
                if (!$ids) {
                    continue;
                }

                $filter = ""
                Foreach ($id in $ids) {
                    $filter += "<value>$id</value>"
                }

                $IntersectEntityName = $relationship.IntersectEntityName
                $Entity1IntersectAttribute = $relationship.Entity1IntersectAttribute
                $fetchxml = @"
                <fetch>
                  <entity name="$IntersectEntityName">
                  <all-attributes />
                    <filter type="and">
                      <condition attribute="$Entity1IntersectAttribute" operator="in">
                        $filter
                      </condition>
                    </filter>
                </entity>
            </fetch>
"@

                $records = Get-CrmRecordsByFetch -conn $CrmConnection -Fetch $fetchxml -AllRows
                if ($records -and $records.ContainsKey("CrmRecords")) { 
                    $nnRecords.AddRange($records.Get_Item("CrmRecords"));
                }
            }
        }
        Catch {
            Write-Error -Message ("Error in the retrieve N:N records task: " + $_.Exception.Message);
            throw
        })

    return $nnRecords
}

function UpsertRecords($CrmConnection, $UpsertRecords) {
    try {
        Log "There are $($UpsertRecords.Count) records to upsert"
        $runningCount = 0;

        Foreach ($upsertRecord in  $UpsertRecords) {
            $runningCount++;
            Log "Upsert $runningCount / $($UpsertRecords.Count)"

            try {
                Set-CrmRecord -CrmRecord $upsertRecord -conn $CrmConnection -Upsert
            } catch {
                Log("Failed Upserting " + $upsertRecord.EntityReference.LogicalName + " " + $upsertRecord.EntityReference.Id)
                Log($_.Exception.Message)
            }
        }
    }
    Catch {
        Write-Error -Message ("Error in the upsert task: " + $_.Exception.Message);
        throw
    }
}

function UpdateLookups($CrmConnection, $UpdateRecords) {
    try {
        Log "There are $($UpdateRecords.Count) records to update lookups for"
        $runningCount = 0;

        Foreach ($updateLookup in  $UpdateRecords) {
            $lookups = @{}
            $runningCount++;
            Log "Update Lookup $runningCount / $($UpdateRecords.Count)"

            if ($updateLookup.Attributes.Count -eq 0) {
                # if entity has no lookups
                continue;
            }
            Foreach ($attr in $updateLookup.Attributes) {
                $lookups.Add($attr.Key, $attr.Value)
            }
            
            try {
                Set-CrmRecord -EntityLogicalName $updateLookup.LogicalName -Id $updateLookup.Id -Fields $lookups -conn $CrmConnection -Upsert
            } Catch {
                Log ("Failed to update lookups " +  $updateLookup.LogicalName + " " + $updateLookup.Id)
                Log ($_.Exception.Message);
            }
        }
    }
    Catch {
        Write-Error -Message ("Error in the update lookup task: " + $_.Exception.Message);
        throw
    }
}

function UpsertAnnotations($CrmConnection, $AnnotationRecords) {
    try {
        Log "There are $($AnnotationRecords.Count) annotation records to upsert"
        $runningCount = 0;

        Foreach ($annotationRecord in $AnnotationRecords) {
            $runningCount++;
            Log "Upsert Annotation $runningCount / $($UpsertRecords.Count)"

            $attr = @{}
            Foreach ($key in $annotationRecord.original.Keys) {
                if ($annotationRecord.$key -and $key -like "*_Property") {
                    $attr.Add($annotationRecord.$key.Key, $annotationRecord.$key.Value)
                }
            }

            try {
                Set-CrmRecord -EntityLogicalName $annotationRecord.EntityReference.LogicalName -Id $annotationRecord.EntityReference.Id -Fields $attr -conn $CrmConnection -Upsert
            } Catch {
                Log ("Failed to upsert annotation " + $annotationRecord.EntityReference.Id)
                Log ($_.Exception.Message);
            }
        }
    }
    Catch {
        Write-Error -Message ("Error in the upsert annotation task: " + $_.Exception.Message);
        throw        
    }
}

function AssociateRecords($CrmConnection, $NNRecords, $Metadata) {
    try {
        Log "There are $($NNRecords.Count) records to associate"
        $runningCount = 0;

        $targetRelationships = $Metadata | Select-Object -Property ManyToManyRelationships -ExpandProperty ManyToManyRelationships
            
        Foreach ($nnrecord in $NNRecords) {
            $runningCount++;
            Log "Associate $runningCount / $($NNRecords.Count)"

            $relationship = $targetRelationships | Where-Object {$_.IntersectEntityName -eq $nnrecord.logicalname} | Select-Object -First 1

            if ($relationship.Entity1LogicalName -eq "contact" -or $relationship.Entity2LogicalName -eq "contact")
            {
                continue;
            }

            Log ("Creating relationship " + $nnrecord.logicalname + " between " + $relationship.Entity1LogicalName + "(" + $nnrecord.($relationship.Entity1IntersectAttribute) + ") and " + $relationship.Entity2LogicalName + "(" + $nnrecord.($relationship.Entity2IntersectAttribute) + ")")
            if (!$relationship) {
                Log ("No relationship with name " + $nnrecord.logicalname + " was found in the target org.")
                continue
            }

            try {
                Add-CrmRecordAssociation -conn $CrmConnection -EntityLogicalName1 $relationship.Entity1LogicalName -Id1 $nnrecord.($relationship.Entity1IntersectAttribute) -EntityLogicalName2 $relationship.Entity2LogicalName -Id2 $nnrecord.($relationship.Entity2IntersectAttribute) -RelationshipName $relationship.IntersectEntityName
            } 
            Catch {
                if ($_.Exception.Message -eq "Cannot insert duplicate key.") {
                    # Association already existS
                    continue
                } 
                elseif ($_.Exception.Message -eq ("Entity Relationship " + $nnrecord.logicalname + " was not found in the metadata")) {
                    Log ("WARNING: Relationship not found in metadata.")
                    continue  
                }
                else {
                    throw $_.Exception
                }
            }
        }
    }
    Catch {
        Write-Error -Message ("Error in the associate records task: " + $_.Exception.Message);
        throw
    }
}

function ImportRecords($CrmConnection, $Records, $NNRecords, $Metadata) {
    $Null = @(Try {
            Log "Importing entity records"

            $nextCycleRecords = New-Object System.Collections.ArrayList
            $processedRecords = New-Object System.Collections.ArrayList

            $nonAnnotationRecords = $Records | Where-Object {$_.EntityReference -and $_.EntityReference.LogicalName -ne "annotation" -and $_.EntityReference.LogicalName -ne "adx_portalcomment"}
            
            Log("Removing Lookups")
            Foreach ($record in $nonAnnotationRecords) {
                $lookups = $record.original.Keys | Where-Object {$record.$_ -and $record.$_.Value -and $record.$_.Value.GetType().Name -eq "EntityReference"}
                    
                $newRecord = New-Object Microsoft.Xrm.Sdk.Entity
                $newRecord.Id = $record.EntityReference.Id
                $newRecord.LogicalName = $record.EntityReference.LogicalName

                $inNextCycle = $nextCycleRecords | Where-Object {$_.Id -eq $record.EntityReference.Id} | Select-Object -First 1
                if ($inNextCycle) {
                    $processedRecords.Add($record)
                    break
                }
                        
                Foreach ($lookup in $lookups) {
                    $lookupRecordExists = $Records | Where-Object {$_.EntityReference -and $_.EntityReference.Id -eq $record.$lookup.Value.Id} | Select-Object -First 1
                        
                    if (!$lookupRecordExists) {
                        break
                    }                        

                    $attr = $record.$lookup
                    
                    $newRecord[$attr.Key] = $attr.Value
                    $record = $record | Select-Object * -ExcludeProperty ($lookup, $attr.Key)
                }

                $nextCycleRecords.Add($newRecord)
                $processedRecords.Add($record)
            }

            Log "Upserting entity records"
            UpsertRecords $CrmConnection $processedRecords

            Log "Updating entity records for lookups"
            UpdateLookups $CrmConnection $nextCycleRecords            

            Log "Upserting annotation records"
            $annotationRecords = $Records | Where-Object {$_.EntityReference -and $_.EntityReference.LogicalName -eq "annotation"}
            UpsertAnnotations $CrmConnection $annotationRecords

            Log "Associating N:N records"
            AssociateRecords $CrmConnection $NNRecords $Metadata
        }
        Catch {
            Write-Error -Message ("Error in the import records task: " + $_.Exception.Message);
            throw
        })

    Log "Finished import records task"
}

try {   
    try {
        # Clear out PS cache
        Remove-Variable * -ErrorAction SilentlyContinue; 
        Remove-Module *; 
        $error.Clear(); 
        Clear-Host
    } Catch { }
    
    $startTime = Get-Date

    Log "Start of Voice of the Customer Mover Tasks"
    Import-Module "$PSScriptRoot\Microsoft.Xrm.Data.PowerShell" -Force

    Log "Extract information from config.json file"
    $jsonPath = "$PSScriptRoot\config.json"
    $json = Get-Content -Raw -Path $jsonPath | ConvertFrom-Json
    $connections = @{}
    $connectionStrings = $json | select -expand connectionStrings
    $connectionStrings | % { $connections.Add($_.name, @{ 'connectionString' = $_.connectionString; } ) }
    $exportConnection = $json | select -expand exportConnection
    $importConnection = $json | select -expand importConnection
    $entities = $json | select -expand entities

    Log "Getting source connection and metadata"
    $SourceCrmConnection = ConnectToCRM $connections[$exportConnection].connectionString
    $SourceMetadata = GetMetadata $SourceCrmConnection $entities

    Log "Getting target connection and metadata"
    $TargetCrmConnection = ConnectToCRM $connections[$importConnection].connectionString
    $TargetMetadata = GetMetadata $TargetCrmConnection $entities

    Log "Exporting information from source"
    $EntityRecords = GetEntityRecords $SourceCrmConnection $entities $SourceMetadata $TargetMetadata
    $NNRecords = GetNNRecords $SourceCrmConnection $EntityRecords $SourceMetadata $TargetMetadata

    Log "Importing information into destination"
    ImportRecords $TargetCrmConnection $EntityRecords $NNRecords $TargetMetadata

    Log "End of Survey Mover Tasks"
    $endTime = Get-Date
    $elapsedTime = $endTime - $startTime
    Log "Start Time: $startTime"
    Log "End Time: $endTime"
    Log "Elapsed Time: $($elapsedTime.ToString("hh\:mm\:ss"))"
}
Catch {
    Write-Error -Message ("Error in Survey Mover script: " + $_.Exception.Message)
}
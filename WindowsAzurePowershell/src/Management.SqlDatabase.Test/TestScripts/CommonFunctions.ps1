# ----------------------------------------------------------------------------------
#
# Copyright 2011 Microsoft Corporation
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
# ----------------------------------------------------------------------------------

# Loads Microsoft.WindowsAzure.Management module
# Selects a subscription id to be used by the test

function Init-TestEnvironment
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [ValidateNotNullOrEmpty()]
        [String]
        $subscriptionID,
        [Parameter(Mandatory=$true, Position=1)]
        [ValidateNotNullOrEmpty()]
        $certThumbPrint
    )
    
    $moduleLoaded = Get-Module -Name "Microsoft.WindowsAzure.Management.SqlDatabase"
    if(!$moduleLoaded)
    {
        Import-Module .\Microsoft.WindowsAzure.Management.dll
        Import-Module .\Microsoft.WindowsAzure.Management.SqlDatabase.dll
    }

    $myCert = Get-Item cert:\\CurrentUser\My\$certThumbPrint
    $subName = "MySub" + $subscriptionID
    Set-AzureSubscription -SubscriptionName $subName -SubscriptionId $subscriptionID -Certificate $myCert
    Select-AzureSubscription -SubscriptionName $subName
}

function Assert 
{
    #.Example
    # set-content C:\test2\Documents\test2 "hi"
    # C:\PS>assert { get-item C:\test2\Documents\test2 } "File wasn't created by Set-Content!"
    #
    [CmdletBinding()]
    param( 
       [Parameter(Position=0,ParameterSetName="Script",Mandatory=$true)]
       [ScriptBlock]$condition
    ,
       [Parameter(Position=0,ParameterSetName="Bool",Mandatory=$true)]
       [bool]$success
    ,
       [Parameter(Position=1,Mandatory=$true)]
       [string]$message
    )

    $message = "ASSERT FAILED: $message"
  
    if($PSCmdlet.ParameterSetName -eq "Script") 
    {
        try 
        {
            $ErrorActionPreference = "STOP"
            $success = &$condition
        } 
        catch 
        {
            $success = $false
            $message = "$message`nEXCEPTION THROWN: $($_.Exception.GetType().FullName)"         
        }
    }
    if(!$success) 
    {
        throw $message
    }
}

function Validate-FirewallRule
{
    [CmdletBinding()]
    Param
    (
        [Parameter(Mandatory=$true, Position=0)]
        [Microsoft.WindowsAzure.Management.SqlDatabase.Model.SqlDatabaseServerFirewallRuleContext]
        $rule, 
        [Parameter(Mandatory=$true, Position=1)]
        [ValidateNotNullOrEmpty()]
        [String]
        $expectedServerName,
        [Parameter(Mandatory=$true, Position=2)]
        [ValidateNotNullOrEmpty()]
        [String]
        $expectedName,
        [Parameter(Mandatory=$true, Position=3)]
        [ValidateNotNullOrEmpty()]
        [String]
        $expectedStartIP,
        [Parameter(Mandatory=$true, Position=4)]
        [ValidateNotNullOrEmpty()]
        [String]
        $expectedEndIP
    )

    Assert {$rule} "Firewall rule is null"
    Assert {$rule.ServerName -eq $expectedServerName} "ruleName didn't match. Actual:[$rule.ServerName] expected:[$expectedServerName]"
    Assert {$rule.RuleName -eq $expectedName} "ruleName didn't match. Actual:[$rule.RuleName] expected:[$expectedName]"
    Assert {$rule.StartIpAddress -eq $expectedStartIP} "StartIP address didn't match. Actual:[$rule.StartIpAddress] expected:[$expectedStartIP]"
    Assert {$rule.EndIpAddress -eq $expectedEndIP} "EndIP address didn't match. Actual:[$rule.EndIpAddress] expected:[$expectedEndIP]"
}


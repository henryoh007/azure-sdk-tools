﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Management.Authorization;
using Microsoft.Azure.Management.Authorization.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Commands.Utilities.Common.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.Commands.Resources.Models.Authorization
{
    public class PoliciesClient
    {
        private const string ResourceManagerAppId = "797f4846-ba00-4fd7-ba43-dac1f8f63013";

        public IAuthorizationManagementClient PolicyClient { get; set; }

        public GraphClient GraphClient { get; set; }

        /// <summary>
        /// Creates PoliciesClient using WindowsAzureSubscription instance.
        /// </summary>
        /// <param name="subscription">The WindowsAzureSubscription instance</param>
        public PoliciesClient(WindowsAzureSubscription subscription)
        {
            string tenantId = "1eeeb395-21c8-4ae0-b145-2abd2dfe501d";
            AccessTokenCredential creds = subscription.CreateTokenCredentials();
            GraphClient = new GraphClient(subscription, tenantId, creds);
            PolicyClient = subscription.CreateClientFromResourceManagerEndpoint<AuthorizationManagementClient>();
        }

        public PSRoleDefinition GetRoleDefinition(string roleId)
        {
            return PolicyClient.RoleDefinitions.Get(roleId).RoleDefinition.ToPSRoleDefinition();
        }

        /// <summary>
        /// Filters the existing role Definitions.
        /// </summary>
        /// <param name="roleDefinitionName">The role name</param>
        /// <returns>The matched role Definitions</returns>
        public List<PSRoleDefinition> FilterRoleDefinitions(string roleDefinitionName)
        {
            List<PSRoleDefinition> result = new List<PSRoleDefinition>();

            if (string.IsNullOrEmpty(roleDefinitionName))
            {
                result.AddRange(PolicyClient.RoleDefinitions.List().RoleDefinitions.Select(r => r.ToPSRoleDefinition()));
            }
            else
            {
                var test = PolicyClient.RoleDefinitions.Get(roleDefinitionName).RoleDefinition.ToPSRoleDefinition();
                result.Add(PolicyClient.RoleDefinitions.Get(roleDefinitionName).RoleDefinition.ToPSRoleDefinition());
            }

            return result;
        }

        /// <summary>
        /// Creates new role assignment.
        /// </summary>
        /// <param name="parameters">The create parameters</param>
        /// <returns>The created role assignment object</returns>
        public PSRoleAssignment CreateRoleAssignment(FilterRoleAssignmentsOptions parameters)
        {
            string principalId = GraphClient.GetPrincipalId(parameters.PrincipalName).Id;
            string roleAssignmentId = Guid.NewGuid().ToString();
            string roleDefinitionId = FilterRoleDefinitions(parameters.RoleDefinition).First().Id;

            RoleAssignmentCreateParameters createParameters = new RoleAssignmentCreateParameters
            {
                PrincipalId = principalId,
                RoleDefinitionId = roleDefinitionId
            };

            PolicyClient.RoleAssignments.Create(parameters.Scope, roleAssignmentId, createParameters);
            return PolicyClient.RoleAssignments.Get(parameters.Scope, roleAssignmentId).RoleAssignment.ToPSRoleAssignment(this, GraphClient);
        }

        /// <summary>
        /// Filters role assignments based on the passed options.
        /// </summary>
        /// <param name="options">The filtering options</param>
        /// <returns>The filtered role assignments</returns>
        public List<PSRoleAssignment> FilterRoleAssigbments(FilterRoleAssignmentsOptions options)
        {
            List<PSRoleAssignment> result = new List<PSRoleAssignment>();

            ListAssignmentsFilterParameters listAboveParams = new ListAssignmentsFilterParameters
            {
                PrincipalId = options.PrincipalName
            };

            result.AddRange(PolicyClient.RoleAssignments.List(listAboveParams)
                .RoleAssignments
                .Select(r => r.ToPSRoleAssignment(this, GraphClient)));

            return result;
        }

        /// <summary>
        /// Deletes a role assignments based on the used options.
        /// </summary>
        /// <param name="options">The role assignment filtering options</param>
        /// <returns>The deleted role assignments</returns>
        public PSRoleAssignment RemoveRoleAssignment(FilterRoleAssignmentsOptions options)
        {
            PSRoleAssignment roleAssignment = FilterRoleAssigbments(options).FirstOrDefault();
            PolicyClient.RoleAssignments.Delete(options.Scope, roleAssignment.Id);

            return roleAssignment;
        }
    }
}

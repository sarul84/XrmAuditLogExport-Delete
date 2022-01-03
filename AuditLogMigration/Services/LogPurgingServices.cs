using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using AuditLogMigration.DataModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace AuditLogMigration
{
    public class LogPurgingServices
    {
        /// <summary>
        /// Defines the logger..
        /// </summary>
        private readonly NLog.Logger _logger;

        public IOrganizationService Service { get; set; }
        
        public LogPurgingServices()
        {
            this._logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public void Process(XrmAudit auditPrimary)
        {
            try
            {
                var auditCollection = Service.RetrieveMultiple(new FetchExpression(FrameFetch(auditPrimary.EntityId.ToString())));

                if (!auditCollection.Entities.Any())
                {
                    _logger.Error("No audit logs found for : {entity} id {id}", auditPrimary.EntityName, auditPrimary.EntityId);
                    auditPrimary.IsDeleted = true;
                    return;
                }

                var auditCount = auditCollection.Entities.Count;

                if (auditCount != auditPrimary.AuditCount && auditPrimary.AuditCount != -1)
                {
                    _logger.Info("Audit logs mismatch for : {entity} id {id}", auditPrimary.EntityName, auditPrimary.EntityId);
                    return;
                }

                DeleteRecordChangeHistoryRequest auditLogsDeleteRequest = new DeleteRecordChangeHistoryRequest
                {
                    Target = new EntityReference(auditPrimary.EntityName, auditPrimary.EntityId)
                };

                OrganizationResponse response = Service.Execute(auditLogsDeleteRequest);
                auditPrimary.IsDeleted = response.Results.ContainsKey("DeletedEntriesCount") && (Convert.ToInt32(response.Results["DeletedEntriesCount"]) > 0);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while deleting audit records from Dynamics for {entity} id {id}", auditPrimary.EntityName, auditPrimary.EntityId);
            }
            
        }
        public string FrameFetch(string objectId)
        {
            string fetchXml = string.Empty;
            fetchXml += "<fetch>";
            fetchXml += "<entity name='audit' >";
            fetchXml += "<attribute name='auditid'/>";
            fetchXml += "<filter>";
            fetchXml += "<condition attribute='objectid' operator='eq' value='" + objectId + "' />";
            fetchXml += "</filter>";
            fetchXml += "</entity>";
            fetchXml += "</fetch>";
            return fetchXml;
        }

        internal IEnumerable<XrmAudit> GetEntityRecords(DateTime fromDate, DateTime toDate, string entityName, string schemaName)
        {
            int fetchCount = Convert.ToInt32(ConfigurationManager.AppSettings.Get("MaxRecordCount"));
            int pageNumber = 1;
            string pagingCookie = null;

            List<XrmAudit> result = new List<XrmAudit>();

            string fetchXml = string.Empty;
            fetchXml += "<fetch>";
            fetchXml += "<entity name='" + schemaName + "'>";
            fetchXml += "<attribute name='" + schemaName + "id'/>";
            fetchXml += "<filter>";
            fetchXml += "<condition attribute='createdon' operator='ge' value='" + fromDate + "' />";
            fetchXml += "<condition attribute='createdon' operator='le' value='" + toDate + "' />";
            fetchXml += "</filter>";
            fetchXml += "</entity>";
            fetchXml += "</fetch>";

            while (true)
            {
                string xml = XrmUtilities.CreateXml(fetchXml, pagingCookie, pageNumber, fetchCount);

                EntityCollection returnCollection = Service.RetrieveMultiple(new FetchExpression(xml));

                foreach(var entity in returnCollection.Entities)
                {
                    result.Add(new XrmAudit { EntityId = entity.Id, EntityName = entityName, AuditCount = -1 });
                }

                if (returnCollection.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = returnCollection.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }
}

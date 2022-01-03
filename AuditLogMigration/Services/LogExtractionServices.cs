using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using AuditLogMigration.Services.Interfaces;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AuditLogMigration.Services
{
    public class LogExtractionServices
    {
        /// <summary>
        /// Defines the logger..
        /// </summary>
        private readonly NLog.Logger _logger;

        public IOrganizationService Service { get; set; }

        public IDataAccess dataAccess { get; set; }

        private DataTable auditAttributeChanges;

        private DataTable auditPrimaryTable;

        private DataSet auditLog;

        public LogExtractionServices()
        {
            this._logger = NLog.LogManager.GetCurrentClassLogger();
            this.DataTableInitialization();
        }

        private void DataTableInitialization()
        {
            auditLog = new DataSet("AuditLogs");

            auditPrimaryTable = new DataTable(ConfigurationManager.AppSettings.Get("PrimaryTable"));
            auditPrimaryTable.Columns.Add("auditid", typeof(Guid));
            auditPrimaryTable.Columns.Add("useridname", typeof(string));
            auditPrimaryTable.Columns.Add("attributemask", typeof(string));
            auditPrimaryTable.Columns.Add("operation", typeof(int));
            auditPrimaryTable.Columns.Add("objecttypecodename", typeof(string));
            auditPrimaryTable.Columns.Add("objectid", typeof(Guid));
            auditPrimaryTable.Columns.Add("operationname", typeof(string));
            auditPrimaryTable.Columns.Add("transactionid", typeof(Guid));
            auditPrimaryTable.Columns.Add("regardingobjectidname", typeof(string));
            auditPrimaryTable.Columns.Add("useradditionalinfo", typeof(string));
            auditPrimaryTable.Columns.Add("createdon", typeof(string));
            auditPrimaryTable.Columns.Add("userid", typeof(Guid));
            auditPrimaryTable.Columns.Add("callinguseridname", typeof(string));
            auditPrimaryTable.Columns.Add("regardingobjectid", typeof(Guid));
            auditPrimaryTable.Columns.Add("objecttypecode", typeof(string));
            auditPrimaryTable.Columns.Add("action", typeof(int));
            auditPrimaryTable.Columns.Add("actionname", typeof(string));
            auditPrimaryTable.Columns.Add("callinguserid", typeof(Guid));
            auditPrimaryTable.Columns.Add("objectidname", typeof(string));

            auditLog.Tables.Add(auditPrimaryTable);

            auditAttributeChanges = new DataTable(ConfigurationManager.AppSettings.Get("AttributeTable"));
            auditAttributeChanges.Columns.Add("auditid", typeof(Guid));
            auditAttributeChanges.Columns.Add("fieldname", typeof(string));
            auditAttributeChanges.Columns.Add("oldvalue", typeof(string));
            auditAttributeChanges.Columns.Add("oldvalue_label", typeof(string));
            auditAttributeChanges.Columns.Add("oldvalue_type", typeof(string));
            auditAttributeChanges.Columns.Add("newvalue", typeof(string));
            auditAttributeChanges.Columns.Add("newvalue_label", typeof(string));
            auditAttributeChanges.Columns.Add("newvalue_type", typeof(string));

            auditLog.Tables.Add(auditAttributeChanges);
        }

        public void ClearAuditTables()
        {
            foreach(DataTable auditTable in auditLog.Tables)
            {
                auditTable.Clear();
            }
        }

        public void Process(DateTime fromDate, DateTime endDate, int entityCode, string entityCodeName)
        {
            int fetchCount = Convert.ToInt32(ConfigurationManager.AppSettings.Get("MaxRecordCount"));
            int pageNumber = 1;
            string pagingCookie = null;

            string fetchXml = FrameFetch(entityCode, fromDate, endDate);

            while (true)
            {
                string xml = XrmUtilities.CreateXml(fetchXml, pagingCookie, pageNumber, fetchCount);

                var auditPrimary = Service.RetrieveMultiple(new FetchExpression(xml));

                foreach (var audit in auditPrimary.Entities)
                {
                    var primaryRow = auditPrimaryTable.NewRow();

                    primaryRow["action"] = this.GetIfExists<OptionSetValue>(audit, "action")?.Value;
                    primaryRow["actionname"] = audit.FormattedValues["action"];
                    primaryRow["attributemask"] = this.GetIfExists<string>(audit, "attributemask");
                    primaryRow["auditid"] = audit.Id;
                    primaryRow["callinguseridname"] = this.GetIfExists<EntityReference>(audit, "callinguserid")?.Name;
                    primaryRow["callinguserid"] = this.GetIfExists(audit, "callinguserid", new EntityReference()).Id;
                    primaryRow["createdon"] = new DateTimeOffset(audit.GetAttributeValue<DateTime>("createdon"));
                    primaryRow["objectid"] = this.GetIfExists<EntityReference>(audit, "objectid")?.Id ?? Guid.Empty;
                    primaryRow["objectidname"] = this.GetIfExists<EntityReference>(audit, "objectid")?.Name;
                    primaryRow["objecttypecode"] = this.GetIfExists<string>(audit, "objecttypecode");
                    primaryRow["objecttypecodename"] = entityCodeName;
                    primaryRow["operation"] = this.GetIfExists<OptionSetValue>(audit, "operation")?.Value;
                    primaryRow["operationname"] = audit.FormattedValues["operation"];
                    primaryRow["regardingobjectid"] = this.GetIfExists<Guid>(audit, "regardingobjectid");
                    primaryRow["regardingobjectidname"] = this.GetIfExists<EntityReference>(audit, "regardingobjectid")?.Name;
                    primaryRow["transactionid"] = this.GetIfExists<Guid>(audit, "transactionid");
                    primaryRow["useradditionalinfo"] = this.GetIfExists<string>(audit, "useradditionalinfo");
                    primaryRow["userid"] = this.GetIfExists<EntityReference>(audit, "userid")?.Id ?? Guid.Empty;
                    primaryRow["useridname"] = this.GetIfExists<EntityReference>(audit, "userid")?.Name;
                    auditPrimaryTable.Rows.Add(primaryRow);

                    RetrieveAuditAttributeChanges(audit.Id.ToString());
                }

                if (auditPrimary.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = auditPrimary.PagingCookie;
                }
                else
                {
                    break;
                }
            }
            
        }

        private void RetrieveAuditAttributeChanges(string auditId)
        {
            var auditDetailsRequest = new RetrieveAuditDetailsRequest
            {
                AuditId = new Guid(auditId)
            };

            var auditDetailsResponse = Service.Execute(auditDetailsRequest) as RetrieveAuditDetailsResponse;

            GetAttributeValues(auditId, auditDetailsResponse?.AuditDetail);
        }

        private void GetAttributeValues(string auditId, AuditDetail auditDetail)
        {
            if(auditDetail == null)
            {
                _logger.Error("No audit detail available for the said audit id");
                return;
            }

            var detailType = auditDetail.GetType();
            if (detailType == typeof(AttributeAuditDetail))
            {
                var attributeDetail = (AttributeAuditDetail)auditDetail;
                string oldValue = "(no value)", newValue = "(no value)";

                foreach (KeyValuePair<string, object> attribute in attributeDetail.NewValue.Attributes)
                {
                    var newDetailRow = auditAttributeChanges.NewRow();
                    newDetailRow["auditid"] = auditId;
                    newDetailRow["fieldname"] = attribute.Key;

                    if (attributeDetail.OldValue.Contains(attribute.Key))
                    {
                        oldValue = GetTypedValueAsString(false, newDetailRow, attributeDetail, attribute.Key);
                        newDetailRow["oldvalue"] = oldValue;
                    }

                    newValue = GetTypedValueAsString(true, newDetailRow, attributeDetail, attribute.Key);
                    newDetailRow["newvalue"] = newValue;
                    auditAttributeChanges.Rows.Add(newDetailRow);

                    Console.WriteLine($"Attribute: {attribute.Key}, old value: {oldValue}, new value: {newValue}");
                }

                foreach (KeyValuePair<string, object> attribute in attributeDetail.OldValue.Attributes)
                {
                    var newDetailRow = auditAttributeChanges.NewRow();
                    newDetailRow["auditid"] = auditId;
                    newDetailRow["fieldname"] = attribute.Key;

                    if (!attributeDetail.NewValue.Contains(attribute.Key))
                    {
                        newValue = "(no value)";

                        oldValue = GetTypedValueAsString(false, newDetailRow, attributeDetail, attribute.Key);
                        newDetailRow["oldvalue"] = oldValue;
                        auditAttributeChanges.Rows.Add(newDetailRow);

                        Console.WriteLine($"Attribute: {attribute.Key}, old value: {oldValue}, new value: {newValue}");
                    }
                }
            }
        }

        private T GetIfExists<T>(Entity entity, string attributeName, T defaultValue = default(T))
        {
            if(!entity.Attributes.ContainsKey(attributeName))
            {
                return defaultValue;
            }

            return entity.GetAttributeValue<T>(attributeName);
        }

        /// <summary>
        /// Returns a string value for the type
        /// </summary>
        /// <param name="typedValue"></param>
        /// <returns></returns>
        private string GetTypedValueAsString(bool isNewValue, DataRow detailRow, AttributeAuditDetail attributeAuditDetail, string attributeKey)
        {
            object typedValue = isNewValue ? attributeAuditDetail.NewValue[attributeKey] 
                                        : attributeAuditDetail.OldValue[attributeKey];

            string value = string.Empty;

            switch (typedValue)
            {
                case OptionSetValue o:
                    value = o.Value.ToString();
                    detailRow[isNewValue ? "newvalue_label" : "oldvalue_label"] 
                        = isNewValue ?  attributeAuditDetail.NewValue.FormattedValues[attributeKey]
                            : attributeAuditDetail.OldValue.FormattedValues[attributeKey];                    
                    break;
                case EntityReference e:
                    detailRow[isNewValue ? "newvalue_type" : "oldvalue_type"] = e.LogicalName;
                    detailRow[isNewValue ? "newvalue_label" : "oldvalue_label"] = e.Name;
                    value = $"{e.Id}";                    
                    break;
                default:
                    value = typedValue.ToString();
                    break;
            }

            return value;

        }

        private string FrameFetch(int objectTyoeCode, DateTime from, DateTime to)
        {
            string fetchXml = string.Empty;
            fetchXml += "<fetch>";
            fetchXml += "<entity name='audit'>";
            fetchXml += "<all-attributes/>";
            fetchXml += "<filter>";
            fetchXml += "<condition attribute='objecttypecode' operator='eq' value='" + objectTyoeCode + "' />";
            fetchXml += "<condition attribute='createdon' operator='ge' value='" + from + "' />";
            fetchXml += "<condition attribute='createdon' operator='lt' value='" + to + "' />";
            fetchXml += "</filter>";
            fetchXml += "</entity>";
            fetchXml += "</fetch>";
            return fetchXml;
        }

        public void Save()
        {
            bool writeToExcel = Convert.ToBoolean(ConfigurationManager.AppSettings.Get("WriteToExcel"));
            if (writeToExcel)
            {
                string filePath = string.Empty;
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        filePath = fbd.SelectedPath;
                    }
}

                auditLog.ExportToExcel(filePath + $"\\AuditLog-{ DateTime.Now.ToFileTime()}.xlsx");
                MessageBox.Show("File Saved!!!");
            }
            else
            {
                int totalCount = 0;
                foreach (DataTable auditTable in auditLog.Tables)
                {
                    var result = dataAccess.Upsert(auditTable);
                    totalCount += result.InsertedCount + result.UpdatedCount;
                }
            }
        }
    }
}

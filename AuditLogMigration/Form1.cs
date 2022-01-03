using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.Data;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using AuditLogMigration.Services;
using AuditLogMigration.Services.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;

namespace AuditLogMigration
{
    public partial class Form1 : Form
    {
        IOrganizationService service;
        IDataAccess dataAccess;
        LogPurgingServices purgingService;
        LogExtractionServices logExtractionService;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public Form1()
        {
            InitializeComponent();

            Initializer();
        }

        private void Initializer()
        {
            ////Initialize all business objects. There is better way of doing this with the help of DI.
            purgingService = new LogPurgingServices();
            logExtractionService = new LogExtractionServices();
            dataAccess = new DataAccess();

            logExtractionService.dataAccess = this.dataAccess;

            ////Set organization dropdown
            cbxorganization.Items.Clear();
            Dictionary<string, string> organizationValues = new Dictionary<string, string>();

            var organizations = ConfigurationManager.AppSettings.Get("OrganizationName")?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Split(new[] { ':' }));

            foreach (var item in organizations)
            {
                organizationValues.Add(item[0], item[1]);
            }

            cbxorganization.DataSource = new BindingSource(organizationValues, null);
            cbxorganization.DisplayMember = "Key";
            cbxorganization.ValueMember = "Value";

            ////Set audit action type dropdown
            var auditActions = new Dictionary<string, string>
            {
                { "Export", "Export" },
                { "DeleteAll", "Purge All Audit Record (Entity)" },
                { "ById", "Purge By Entity Record ID" },
                { "ByDate", "Purge By Date" }
            };

            cbxAuditActionType.DataSource = new BindingSource(auditActions, null);
            cbxAuditActionType.DisplayMember = "Value";
            cbxAuditActionType.ValueMember = "Key";

            EnableDateFields();

            ////Set default date values
            dtpFromDate.Value = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Local);
            dtpToDate.Value = DateTime.Now.Date;

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUserName.Text;
            string password = txtPassword.Text;
            string organizationName = cbxorganization.SelectedValue.ToString();

            //Validation
            errorProvider1.ClearAllErrors();

            if (string.IsNullOrEmpty(username))
            {
               errorProvider1.SetErrorWithCount(txtUserName, "Please enter email");
            }

            if (string.IsNullOrEmpty(password))
            {
               errorProvider1.SetErrorWithCount(txtPassword, "Check your password");
            }

            if (errorProvider1.HasErrors())
            {
               return;
            }


            Authenticator app = new Authenticator(
                    username.Trim(),
                    password.Trim(),
                    organizationName);

            ////Best practicse is to run long running process in separate thread.
            Thread authThread = new Thread(() =>
            {
                service = app.Run();
                purgingService.Service = service;
                logExtractionService.Service = service;
            });

            authThread.Start();
            authThread.Join();

            if (service != null)
            {
                Logger.Info("Successfully Authenticated");
                MessageBox.Show("Authenticated", "Login");
            }
        }

        //Audit Purging button event
        private void button1_Click(object sender, EventArgs e)
        {
            var extractedAuditRecords = dataAccess.Read();
            foreach (var auditedRecord in extractedAuditRecords)
            {
                purgingService.Process(auditedRecord);
            }

            dataAccess.Update(extractedAuditRecords.ToDataTable());
        }

        private void btnEntityLoad_Click(object sender, EventArgs e)
        {
            DataTable auditedEntities = new DataTable("Entities");
            auditedEntities.Columns.Add("SelectedEntity", typeof(bool));
            auditedEntities.Columns.Add("TableName", typeof(string));
            auditedEntities.Columns.Add("SchemaName", typeof(string));
            auditedEntities.Columns.Add("ObjectTypeCode", typeof(int));

            var EntityFilter = new MetadataFilterExpression(LogicalOperator.And);
            EntityFilter.Conditions.Add(new MetadataConditionExpression("IsAuditEnabled", MetadataConditionOperator.Equals, true));

            var entityQueryExpression = new EntityQueryExpression()
            {
                Criteria = EntityFilter
            };

            var retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest()
            {
                Query = entityQueryExpression,
                ClientVersionStamp = null,
                DeletedMetadataFilters = DeletedMetadataFilters.Default
            };

            var response = service.Execute(retrieveMetadataChangesRequest) as RetrieveMetadataChangesResponse;

            foreach (var entity in response.EntityMetadata)
            {
                var newEntity = auditedEntities.NewRow();
                newEntity["SelectedEntity"] = false;
                newEntity["TableName"] = entity.DisplayName.LocalizedLabels.FirstOrDefault().Label;
                newEntity["SchemaName"] = entity.LogicalName;
                newEntity["ObjectTypeCode"] = entity.ObjectTypeCode;

                auditedEntities.Rows.Add(newEntity);
            }

            dgEntityList.AutoGenerateColumns = false;            
            dgEntityList.DataSource = auditedEntities;

            dgEntityList.Columns[0].DataPropertyName = auditedEntities.Columns[0].ColumnName;
            dgEntityList.Columns[1].DataPropertyName = auditedEntities.Columns[1].ColumnName;
            dgEntityList.Columns[2].DataPropertyName = auditedEntities.Columns[2].ColumnName;
            dgEntityList.Columns[3].DataPropertyName = auditedEntities.Columns[3].ColumnName;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            (dgEntityList.DataSource as DataTable).DefaultView.RowFilter = "TableName like '%" + txtSearch.Text.Trim() + "%'"; // and TableName >='" + txtSearch.Text + "'";
            dgEntityList.Refresh();
        }

        //Audit Export button event
        private void button3_Click(object sender, EventArgs e)
        {
            string auditAction = cbxAuditActionType.SelectedValue.ToString();

            var fromDate = dtpFromDate.Value;
            var toDate = dtpToDate.Value.AddDays(1);
            var selectedEntities = dgEntityList.SelectedRows;
            
            if (selectedEntities.Count == 0)
            {
                MessageBox.Show("Please select atleast one entity to continue audit extraction", "Audit Utility");
                return;
            }

            if (auditAction == "Export")
            {
                logExtractionService.ClearAuditTables();
                foreach (DataGridViewRow entity in selectedEntities)
                {
                    logExtractionService.Process(
                        fromDate,
                        toDate,
                        Convert.ToInt32(entity.Cells["TypeCode"].Value),
                        entity.Cells["TableName"].Value.ToString());
                }

                logExtractionService.Save();
            }
            else
            {
                if(auditAction == "DeleteAll" || auditAction == "ByDate")
                {
                    foreach (DataGridViewRow entity in selectedEntities)
                    {
                        var toBePurged = purgingService.GetEntityRecords(
                            fromDate,
                            toDate,
                            entity.Cells["TableName"].Value.ToString(),
                            entity.Cells["SchemaName"].Value.ToString()
                            );

                        foreach (var purge in toBePurged)
                        {
                            purgingService.Process(purge);
                        }
                    }
                }
                else if(auditAction == "ById")
                {
                    var extractedAuditRecords = dataAccess.Read();
                    foreach (var auditedRecord in extractedAuditRecords)
                    {
                        purgingService.Process(auditedRecord);
                    }

                    dataAccess.Update(extractedAuditRecords.ToDataTable());
                }                
            }
        }

        private void cbxAuditActionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDateFields();
        }

        private void EnableDateFields()
        {
            dtpFromDate.Enabled = false;
            dtpToDate.Enabled = false;

            string auditAction = cbxAuditActionType.SelectedValue.ToString();

            if (auditAction == "ByDate" || auditAction == "Export")
            {
                dtpFromDate.Enabled = true;
                dtpToDate.Enabled = true;
            }
        }
    }
}

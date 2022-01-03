using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using AuditLogMigration.DataModel;

namespace AuditLogMigration
{
    public static class ErrorProviderExtensions
    {
        private static int count;

        public static void SetErrorWithCount(this ErrorProvider ep, Control c, string message)
        {
            if (message == "")
            {
                if (ep.GetError(c) != "")
                    count--;
            }
            else
                count++;

            ep.SetError(c, message);
        }

        public static bool HasErrors(this ErrorProvider ep)
        {
            return count != 0;
        }

        public static int GetErrorCount(this ErrorProvider ep)
        {
            return count;
        }

        public static void ClearAllErrors(this ErrorProvider ep)
        {
            count = 0;
            ep.Clear();
        }
    }

    public static class DataTableExtensions
    {
        public static bool Any(this DataSet source)
        {
            if (source != null && source.Tables.Count > 0) return true;

            return false;
        }
    }

    public static class XrmAuditExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<XrmAudit> audits)
        {
            DataTable auditPrimary = new DataTable(ConfigurationManager.AppSettings.Get("PrimaryTable"));
            auditPrimary.Columns.Add(new DataColumn("objectid", typeof(Guid)));
            auditPrimary.Columns.Add(new DataColumn("objecttypecode", typeof(string)));
            auditPrimary.Columns.Add(new DataColumn("new_isdeleted", typeof(bool)));

            foreach (var audit in audits)
            {
                var newRow = auditPrimary.NewRow();
                newRow["objectid"] = audit.EntityId;
                newRow["objecttypecode"] = audit.EntityName;
                newRow["new_isdeleted"] = audit.IsDeleted;

                auditPrimary.Rows.Add(newRow);
            }

            return auditPrimary;
        }
    }
}

using System;

namespace AuditLogMigration.DataModel
{
    public sealed class XrmAudit
    {
        public Guid EntityId { get; set; }
        public string EntityName { get; set; }
        public int AuditCount { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

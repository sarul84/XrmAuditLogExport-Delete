// -------------------------------------------------------------------------------
// <copyright file="IDataAccess.cs" company="Prakrishta Technologies">
// Copyright © 2022 All Right Reserved
// </copyright>
// <Author>Arul Sengottaiyan</Author>
// <date>01/03/2022</date>
// <summary>Interface that defines the database operation methods</summary>
// --------------------------------------------------------------------------------

namespace AuditLogMigration.Services.Interfaces
{
    using AuditLogMigration.DataModel;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    /// <summary>
    /// Defines the <see cref="IDataAccess" /> contract.
    /// </summary>
    public interface IDataAccess
    {
        #region |Methods|

        /// <summary>
        /// The method that closes active sql connection.
        /// </summary>
        /// <param name="connection">The connection<see cref="SqlConnection"/>.</param>
        void CloseConnection(SqlConnection connection);

        /// <summary>
        /// The method that does the data retrieval from the database based on the condition.
        /// </summary>        
        /// <returns>The <see cref="IEnumerable{XrmAudit}"/>.</returns>
        IEnumerable<XrmAudit> Read();

        /// <summary>
        /// The mthod that does bulk update to the database.
        /// </summary>
        /// <param name="xrmAuditLogs">The audit logs<see cref="DataTable"/>.</param>
        void Update(DataTable xrmAuditLogs);

        MergeResult Upsert(DataTable xrmAuditLogs);

        #endregion
    }
}

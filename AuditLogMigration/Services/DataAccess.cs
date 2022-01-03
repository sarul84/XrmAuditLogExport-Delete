// -------------------------------------------------------------------------------
// <copyright file="DataAccess.cs" company="Prakrishta Technologies">
// Copyright © 2022 All Right Reserved
// </copyright>
// <Author>Arul Sengottaiyan</Author>
// <date>01/03/2022</date>
// <summary>The Data Access class</summary>
// --------------------------------------------------------------------------------

namespace AuditLogMigration.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
using System.Web.Configuration;
    using AuditLogMigration.DataModel;
    using AuditLogMigration.Services.Interfaces;

    /// <summary>
    /// Defines the <see cref="DataAccess" /> class.
    /// </summary>
    public sealed class DataAccess : IDataAccess
    {
        #region |Private Fields|

        /// <summary>
        /// Defines the logger..
        /// </summary>
        private readonly NLog.Logger _logger;

        private readonly List<int> transientErrorNumbers = new List<int>
                                                            { -2, 64, 4060, 40197, 40501, 40613, 49918, 49919, 49920, 11001 };

        private readonly IEnumerable<QueryItem> queryItems;
        #endregion

        #region |Constructors|

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccess"/> class.
        /// </summary>
        public DataAccess()
        {
            this._logger = NLog.LogManager.GetCurrentClassLogger();

            queryItems = JsonFileReader.Read<List<QueryItem>>($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Queries.json");
        }

        #endregion

        #region |Public Properties|

        /// <summary>
        /// Gets or sets the Connection.
        /// </summary>
        public SqlConnection Connection { get; set; }

        #endregion

        #region |Methods|

        /// <inheritdoc />
        public void CloseConnection(SqlConnection connection)
        {
            if (connection?.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <inheritdoc />
        public IEnumerable<XrmAudit> Read()
        {
            var attachments = Enumerable.Empty<XrmAudit>();

            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 10;

            for (int tries = 1;  tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        this._logger.Info("Transient error encountered. Will begin attempt number {0} of {1} max...", tries, totalNumberOfTimesToTry);
                        Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }

                    attachments = this.GetAuditRecords();
                    break;
                }
                catch (SqlException sqlExc)
                {
                    if (this.transientErrorNumbers.Contains(sqlExc.Number) == true)
                    {
                        this._logger.Error("{0}: transient occurred.", sqlExc.Number);
                        continue;
                    }
                    else
                    {
                        this._logger.Error("There is a SQL error while executing read query, error {message}", sqlExc);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    this._logger.Error("There is an error while executing read query, error {message}", ex.Message);
                    break;
                }
            }              

            return attachments;
        }

        /// <inheritdoc />
        public void Update(DataTable xrmAuditLogs)
        {
            try
            {
                int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CommandTimeOut"));

                using (SqlConnection connection = this.GetOpenConnection())
                {
                    using (SqlCommand command = new SqlCommand(queryItems.FirstOrDefault(x => x.Key == "createtemptable").SqlStatement, connection))
                    {
                        command.ExecuteNonQuery();

                        using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connection))
                        {
                            bulkcopy.BulkCopyTimeout = (timeOut * 2);
                            bulkcopy.DestinationTableName = "#TmpTable";
                            bulkcopy.WriteToServer(xrmAuditLogs);
                            bulkcopy.Close();
                        }

                        command.CommandTimeout = timeOut;
                        command.CommandText = queryItems.FirstOrDefault(x => x.Key == "updateprimary").SqlStatement;
                        var affected = command.ExecuteNonQuery();
                        this._logger.Info("Updated records: {count}", affected);
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.Error("There is an error while executing update query, error {message}", ex.Message);
            }
        }

        public MergeResult Upsert(DataTable xrmAuditLogs)
        {
            var mergeResult = new MergeResult();

            if(xrmAuditLogs.Rows.Count == 0)
            {
                return mergeResult;
            }

            try
            {
                int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CommandTimeOut"));

                var distinctAuditLogs = xrmAuditLogs.DefaultView.ToTable(true, "auditid");

                var columnNames = new HashSet<string>(xrmAuditLogs.Columns.Cast<DataColumn>().Select(c => c.ColumnName));

                var distinctAuditIds = distinctAuditLogs.AsEnumerable()
                                          .Select(s => s.Field<Guid>("auditid"))
                                          .Distinct()
                                          .ToList();

                string idsToBeQueried = string.Empty;
                if (distinctAuditIds.Any())
                {
                    idsToBeQueried = $"'{string.Join("','", distinctAuditIds)}'";
                }

                if (!string.IsNullOrEmpty(idsToBeQueried))
                {
                    using (SqlConnection connection = this.GetOpenConnection())
                    {
                        DataTable dataFromStore = new DataTable();
                        using (SqlCommand command = new SqlCommand($"SELECT {string.Join(",", columnNames)} FROM {xrmAuditLogs.TableName} where auditid in ({idsToBeQueried})", connection))
                        {
                            command.CommandTimeout = (timeOut * 2);
                            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                adapter.Fill(dataFromStore);
                            }
                        }

                        EnumerableRowCollection<DataRow> recordsExistInBoth = null;
                        EnumerableRowCollection<DataRow> recordsNotInStore = null;

                        string joinCondition = string.Empty;

                        if (xrmAuditLogs.TableName == ConfigurationManager.AppSettings.Get("PrimaryTable"))
                        {
                            if (dataFromStore.Rows.Count > 0)
                            {
                                //Records to be updated
                                recordsExistInBoth = xrmAuditLogs.AsEnumerable().Where(
                                    x => !dataFromStore.AsEnumerable().Any(x2 => x["auditid"] == x2["auditid"]));

                                //Records to be inserted
                                recordsNotInStore = xrmAuditLogs.AsEnumerable().Where(
                                    x => dataFromStore.AsEnumerable().Any(x2 => x["auditid"] == x2["auditid"]));
                            }
                            else
                            {
                                recordsNotInStore = xrmAuditLogs.AsEnumerable().Where(x => x["auditid"] != null);
                            }

                            joinCondition = "P.auditid =  T.auditid";
                        }
                        else
                        {
                            if (dataFromStore.Rows.Count > 0)
                            {
                                //Records to be updated
                                recordsExistInBoth = xrmAuditLogs.AsEnumerable().Where(
                                    x => !dataFromStore.AsEnumerable().Any(
                                            x2 => x["auditid"] == x2["auditid"]
                                            && x["fieldname"] == x2["fieldname"]
                                            && string.Compare(x["oldvalue"].ToString(), x2["oldvalue"].ToString(), true) == 0
                                            && string.Compare(x["newvalue"].ToString(), x2["newvalue"].ToString(), true) == 0
                                            && string.Compare(x["oldvalue_label"].ToString(), x2["oldvalue_label"].ToString(), true) == 0
                                            && string.Compare(x["newvalue_label"].ToString(), x2["newvalue_label"].ToString(), true) == 0
                                            && string.Compare(x["oldvalue_type"].ToString(), x2["oldvalue_type"].ToString(), true) == 0
                                            && string.Compare(x["newvalue_type"].ToString(), x2["newvalue_type"].ToString(), true) == 0));

                                //Records to be inserted
                                recordsNotInStore = xrmAuditLogs.AsEnumerable().Where(
                                    x => dataFromStore.AsEnumerable().Any(
                                            x2 => x["auditid"] == x2["auditid"]
                                            && x["fieldname"] == x2["fieldname"]
                                            && string.Compare(x["oldvalue"].ToString(), x2["oldvalue"].ToString(), true) == 0
                                            && string.Compare(x["newvalue"].ToString(), x2["newvalue"].ToString(), true) == 0
                                            && string.Compare(x["oldvalue_label"].ToString(), x2["oldvalue_label"].ToString(), true) == 0
                                            && string.Compare(x["newvalue_label"].ToString(), x2["newvalue_label"].ToString(), true) == 0
                                            && string.Compare(x["oldvalue_type"].ToString(), x2["oldvalue_type"].ToString(), true) == 0
                                            && string.Compare(x["newvalue_type"].ToString(), x2["newvalue_type"].ToString(), true) == 0));
                            }
                            else
                            {
                                recordsNotInStore = xrmAuditLogs.AsEnumerable().Where(x => x["auditid"] != null);
                            }

                            joinCondition = string.Join(" and ", columnNames.Select(name => $"CONVERT(NVARCHAR(MAX), COALESCE(P.[{name}], '0')) = CONVERT(NVARCHAR(MAX), COALESCE(T.[{name}], '0'))"));
                        }

                        //Bulk insert
                        if (recordsNotInStore?.Any() == true)
                        {
                            using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connection))
                            {
                                var filesInserted = 0L;

                                bulkcopy.BulkCopyTimeout = (timeOut * 2);
                                bulkcopy.DestinationTableName = xrmAuditLogs.TableName;
                                bulkcopy.SqlRowsCopied += (s, e) => filesInserted = e.RowsCopied;
                                
                                foreach (var columnName in columnNames)
                                {
                                    bulkcopy.ColumnMappings.Add(columnName, columnName);
                                }

                                bulkcopy.WriteToServer(recordsNotInStore.ToArray());
                                bulkcopy.Close();

                                mergeResult.InsertedCount += Convert.ToInt32(filesInserted);
                            }
                        }

                        //Bulk update
                        if (recordsExistInBoth?.Any() == true)
                        {
                            using (SqlCommand command = new SqlCommand($"SELECT TOP (0) {string.Join(",", columnNames)} INTO #{xrmAuditLogs.TableName} FROM {xrmAuditLogs.TableName}", connection))
                            {
                                command.ExecuteNonQuery();

                                using (var bulkCopy = new SqlBulkCopy(connection))
                                {
                                    bulkCopy.BulkCopyTimeout = (timeOut * 2);
                                    bulkCopy.DestinationTableName = $"#{xrmAuditLogs.TableName}";

                                    foreach (var columnName in columnNames)
                                    {
                                        bulkCopy.ColumnMappings.Add(columnName, columnName);
                                    }

                                    bulkCopy.WriteToServer(xrmAuditLogs);
                                    bulkCopy.Close();
                                }

                                string mergeQuery = queryItems.FirstOrDefault(x => x.Key == "updateaudittables").SqlStatement;

                                mergeQuery = string.Format(mergeQuery,
                                    string.Join(", ", columnNames.Select(name => $"P.[{name}] = T.[{name}]"))
                                    , xrmAuditLogs.TableName
                                    , $"#{xrmAuditLogs.TableName}"
                                    , joinCondition
                                    , $"#{xrmAuditLogs.TableName}");

                                command.CommandTimeout = (timeOut * 2);
                                command.CommandText = mergeQuery;
                                int updatedRecords = command.ExecuteNonQuery();

                                mergeResult.UpdatedCount += updatedRecords;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.Error("There is an error while performing upsert operation, error: {message}", ex.Message);
            }
            
            return mergeResult;
        }

        /// <summary>
        /// Get the attachments record for the given date
        /// </summary>
        /// <param name="input">The process input</param>
        /// <returns>Total number of attachments</returns>
        private IEnumerable<XrmAudit> GetAuditRecords()
        {
            var auditRecords = new Collection<XrmAudit>();

            using (SqlConnection connection = this.GetOpenConnection())
            {
                int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings.Get("CommandTimeOut"));

                string query = queryItems.FirstOrDefault(x => x.Key == "getauditrecords").SqlStatement;

                SqlCommand command = new SqlCommand(query, connection);
                command.CommandTimeout = timeOut;
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    var audirRecord = new XrmAudit
                    {
                        AuditCount = Convert.ToInt32(dataReader["AuditCount"]),
                        EntityId = Guid.Parse(Convert.ToString(dataReader["EntityId"])),
                        EntityName = Convert.ToString(dataReader["EntityName"])                        
                    };

                    auditRecords.Add(audirRecord);
                }

                this._logger.Info("Total number of records: {count}", auditRecords?.Count());
            }

            return auditRecords;
        }

        /// <summary>
        /// The GetOpenConnection.
        /// </summary>
        /// <returns>The <see cref="SqlConnection"/>.</returns>
        private SqlConnection GetOpenConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Staging"]?.ConnectionString;
            Connection = new SqlConnection(connectionString);

            if (Connection.State != System.Data.ConnectionState.Open)
            {
                Connection.Open();
            }

            return Connection;
        }

        #endregion
    }
}

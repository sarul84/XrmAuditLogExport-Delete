using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuditLogMigration.DataModel
{
	public class MergeResult
	{
		public int UpdatedCount { get; set; }
		public int InsertedCount { get; set; }
	}
}

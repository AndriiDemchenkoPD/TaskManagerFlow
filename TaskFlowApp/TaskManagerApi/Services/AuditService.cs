using Microsoft.Data.SqlClient;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services
{
    public class AuditService
    {
        private readonly IConfiguration _config;

        public AuditService(IConfiguration config)
        {
            _config = config;
        }

        private string Conn => _config.GetConnectionString("BTConnection")!;

        public void LogTaskChange(int taskId, string operationType, int userId, string userSign, Dictionary<string, (string? oldValue, string? newValue)> changes)
        {
            using var conn = new SqlConnection(Conn);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // Insert audit header
                string hdrQuery = @"
                    INSERT INTO AuditHdr (vTableName, vSchemaName, nRecordPK, vOperationType, 
                                          nRecordedBy, dRecordedOnUTC, nRecordedAtTimeZone, 
                                          vRecordedAtOffSet, vRecordedSign, cReplicaFlag)
                    OUTPUT INSERTED.nAuditHdrNo
                    VALUES ('BT_Tasks', 'dbo', @taskId, @opType, @userId, GETUTCDATE(), 2, '+05:30', @userSign, 'N')";

                var hdrCmd = new SqlCommand(hdrQuery, conn, transaction);
                hdrCmd.Parameters.AddWithValue("@taskId", taskId.ToString());
                hdrCmd.Parameters.AddWithValue("@opType", operationType);
                hdrCmd.Parameters.AddWithValue("@userId", userId);
                hdrCmd.Parameters.AddWithValue("@userSign", userSign);

                long auditHdrNo = (long)(decimal)hdrCmd.ExecuteScalar();

                // Insert audit details for each changed field
                foreach (var change in changes)
                {
                    string dtlQuery = @"
                        INSERT INTO AuditDtl (nAuditHdrNo, vFieldName, vOldValue, vNewValue, vAuditRemark)
                        VALUES (@hdrNo, @field, @oldVal, @newVal, @remark)";

                    var dtlCmd = new SqlCommand(dtlQuery, conn, transaction);
                    dtlCmd.Parameters.AddWithValue("@hdrNo", auditHdrNo);
                    dtlCmd.Parameters.AddWithValue("@field", change.Key);
                    dtlCmd.Parameters.AddWithValue("@oldVal", (object?)change.Value.oldValue ?? DBNull.Value);
                    dtlCmd.Parameters.AddWithValue("@newVal", (object?)change.Value.newValue ?? DBNull.Value);
                    dtlCmd.Parameters.AddWithValue("@remark", $"{change.Key} changed");

                    dtlCmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<TaskAuditHistory> GetTaskAuditHistory(int taskId)
        {
            var history = new List<TaskAuditHistory>();

            using var conn = new SqlConnection(Conn);
            string query = @"
                SELECT 
                    h.nAuditHdrNo,
                    h.vOperationType,
                    h.dRecordedOnUTC,
                    h.vRecordedSign,
                    d.vFieldName,
                    d.vOldValue,
                    d.vNewValue,
                    d.vAuditRemark
                FROM AuditHdr h
                LEFT JOIN AuditDtl d ON h.nAuditHdrNo = d.nAuditHdrNo
                WHERE h.vTableName = 'BT_Tasks' AND h.nRecordPK = @taskId
                ORDER BY h.dRecordedOnUTC DESC";

            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@taskId", taskId.ToString());

            conn.Open();
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                history.Add(new TaskAuditHistory
                {
                    AuditId = reader.GetInt64(0),
                    OperationType = reader.GetString(1),
                    ChangedOn = reader.GetDateTime(2),
                    ChangedBy = reader.GetString(3),
                    FieldName = reader.IsDBNull(4) ? null : reader.GetString(4),
                    OldValue = reader.IsDBNull(5) ? null : reader.GetString(5),
                    NewValue = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Remark = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }

            return history;
        }
    }

    public class TaskAuditHistory
    {
        public long AuditId { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public DateTime ChangedOn { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public string? FieldName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Remark { get; set; }
    }
}

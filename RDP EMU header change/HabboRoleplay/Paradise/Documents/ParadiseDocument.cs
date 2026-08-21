using System;

namespace Plus.HabboRoleplay.Paradise.Documents
{
    public sealed class ParadiseDocument
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public int TypeId { get; private set; }
        public string TypeCode { get; private set; }
        public string TypeName { get; private set; }
        public string Category { get; private set; }
        public string Number { get; private set; }
        public string Status { get; private set; }
        public DateTime IssuedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public string Metadata { get; private set; }

        public bool IsValid
        {
            get
            {
                if (!String.Equals(Status, "VALID", StringComparison.OrdinalIgnoreCase)) return false;
                return !ExpiresAt.HasValue || ExpiresAt.Value > DateTime.Now;
            }
        }

        public ParadiseDocument(int id, int userId, int typeId, string typeCode, string typeName,
            string category, string number, string status, DateTime issuedAt, DateTime? expiresAt, string metadata)
        {
            Id = id;
            UserId = userId;
            TypeId = typeId;
            TypeCode = typeCode ?? String.Empty;
            TypeName = typeName ?? String.Empty;
            Category = category ?? String.Empty;
            Number = number ?? String.Empty;
            Status = status ?? "VALID";
            IssuedAt = issuedAt;
            ExpiresAt = expiresAt;
            Metadata = metadata ?? String.Empty;
        }
    }
}

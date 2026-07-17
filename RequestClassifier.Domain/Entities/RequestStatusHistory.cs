using RequestClassifier.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RequestClassifier.Domain.Entities
{
    public class RequestStatusHistory
    {
        public int Id { get; set; }

        public int ServiceRequestId { get; set; }

        public ServiceRequest ServiceRequest { get; set; } = null!;

        public RequestStatus? OldStatus { get; set; }

        public RequestStatus NewStatus { get; set; }

        public string? Description { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    }
}

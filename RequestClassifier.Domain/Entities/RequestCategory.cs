using System;
using System.Collections.Generic;
using System.Text;

namespace RequestClassifier.Domain.Entities
{
    public class RequestCategory : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;


        public int DepartmentId { get; set; }               // Foreign key to the Department entity
        public Department Department { get; set; } = null!; // Navigation property to the Department entity


        public ICollection<ServiceRequest> PredictedRequests { get; set; } = new List<ServiceRequest>(); // Navigation property to the ServiceRequest entity, 1xN relationship for predicted requests

        public ICollection<ServiceRequest> AssignedRequests { get; set; } = new List<ServiceRequest>(); // Navigation property to the ServiceRequest entity, 1xN relationship for assigned requests
    }
}

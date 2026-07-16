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


        public Department Department { get; set; } = null!; // Navigation property to the Department entity
        public int DepartmentId { get; set; }               // Foreign key to the Department entity
    }
}

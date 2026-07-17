using System;
using System.Collections.Generic;
using System.Text;

namespace RequestClassifier.Domain.Entities
{
    public class Department : BaseEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property to the RequestCategory entity, 1xN relationship
        public ICollection<RequestCategory> RequestCategories { get; set; } = new List<RequestCategory>();

        // Navigation property to the ApplicationUser entity, 1xN relationship
        public ICollection<ApplicationUser> Employees { get; set; } = new List<ApplicationUser>();
    }

}


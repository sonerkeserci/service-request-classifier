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
    }

}
}

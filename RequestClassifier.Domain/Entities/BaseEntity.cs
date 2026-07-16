using System;
using System.Collections.Generic;
using System.Text;

namespace RequestClassifier.Domain.Entities
{
    internal class BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace RequestClassifier.Domain.Enums
{
    public enum RequestStatus
    {
        Received = 1,
        Classified = 2,
        Assigned = 3,
        InProgress = 4,
        Resolved = 5,
        Closed = 6,
        Rejected = 7
    }
}

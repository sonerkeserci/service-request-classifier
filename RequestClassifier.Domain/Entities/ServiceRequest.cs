using System;
using System.Collections.Generic;
using System.Text;
using RequestClassifier.Domain.Enums;

namespace RequestClassifier.Domain.Entities
{
    public class ServiceRequest : BaseEntity
    {
        public int Id { get; set; }

        public string RequestNumber { get; set; } = string.Empty;   // Unique tracking number for user requests

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public RequestStatus Status { get; set; } = RequestStatus.Received;



        public float? PredictionScore { get; set; }     // Confidence score of the predicted category


        public int? PredictedCategoryId { get; set; }
        public RequestCategory? PredictedCategory { get; set; } // Category predicted by the system's machine learning model

        public int? AssignedCategoryId { get; set; }
        public RequestCategory? AssignedCategory { get; set; }  // Category assigned by the system or personnel after reviewing the prediction


        public bool IsAutoAssigned { get; set; }
        public ICollection<RequestStatusHistory> StatusHistories { get; set; } = new List<RequestStatusHistory>(); // History of status changes for the request



        public string RequesterFirstName { get; set; } = string.Empty;

        public string RequesterLastName { get; set; } = string.Empty;

        public string RequesterEmail { get; set; } = string.Empty;

        public string? RequesterPhoneNumber { get; set; }
    }
}

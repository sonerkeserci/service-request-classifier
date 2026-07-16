using System;
using System.Collections.Generic;
using System.Text;

namespace RequestClassifier.Domain.Entities
{
    public class ServiceRequest : BaseEntity
    {
        public int Id { get; set; }

        public string RequestNumber { get; set; } = string.Empty;   // Unique tracking number for user requests

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;


        public RequestCategory? PredictedCategory { get; set; } // Category predicted by the system's machine learning model
        public int? PredictedCategoryId { get; set; }



        public RequestCategory? AssignedCategory { get; set; }  // Category assigned by the system or personnel after reviewing the prediction
        public int? AssignedCategoryId { get; set; }

        public float? PredictionScore { get; set; }     // Confidence score of the predicted category

        public bool IsAutoAssigned { get; set; }


    }
}

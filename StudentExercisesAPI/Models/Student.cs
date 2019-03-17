using System;
using System.Collections.Generic;

namespace StudentExercisesAPI.Models { 

    public class Student {

        public Student (int id, string firstName, string lastName, string slackHandle, int cohortId) {
           
           Id = id;
           FirstName = firstName;
           LastName = lastName;
           SlackHandle = slackHandle;
           CohortId = cohortId;
           AssignedExercises = new List<Exercise>();
        }

        public Student(string firstName, string lastName, string slackHandle, int cohortId) {
            
            FirstName = firstName;
            LastName = lastName;
            SlackHandle = slackHandle;
            CohortId = cohortId;
            AssignedExercises = new List<Exercise>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SlackHandle { get; set; }
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; }
        public List<Exercise> AssignedExercises { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExercisesAPI.Models { 

    public class Student {

        public Student() {

            FirstName = null;
            LastName = null;
            SlackHandle = null;
            CohortId = 0;
        }

        public Student (int id, string firstName, string lastName, string slackHandle, int cohortId) {
           
           Id = id;
           FirstName = firstName;
           LastName = lastName;
           SlackHandle = slackHandle;
           CohortId = cohortId;
           Cohort = new Cohort();
           AssignedExercises = new List<Exercise>();
        }

        public Student(string firstName, string lastName, string slackHandle, int cohortId) {
            
            FirstName = firstName;
            LastName = lastName;
            SlackHandle = slackHandle;
            CohortId = cohortId;
            Cohort = new Cohort();
            AssignedExercises = new List<Exercise>();
        }

        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        [StringLength(25, MinimumLength = 2)]
        public string LastName { get; set; }

        public string FullName {
            get {
                return $"{FirstName} {LastName}";
            }
        }

        [Required]
        [StringLength(12, MinimumLength = 3)]
        public string SlackHandle { get; set; }

        [Required]
        public int CohortId { get; set; }

        public Cohort Cohort { get; set; }

        public List<Exercise> AssignedExercises { get; set; }

    }
}

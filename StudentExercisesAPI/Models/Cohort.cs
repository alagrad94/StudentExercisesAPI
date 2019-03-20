using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExercisesAPI.Models { 

    public class Cohort {

        public Cohort (int id, string cohortName) {

            Id = id;
            CohortName = cohortName;
            StudentList = new List<Student>();
            InstructorList = new List<Instructor>();
        }

        public Cohort(string cohortName) {

            CohortName = cohortName;
            StudentList = new List<Student>();
            InstructorList = new List<Instructor>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 5)]
        public string CohortName { get; set; }

        public List<Student> StudentList { get; set; }
        public List<Instructor> InstructorList { get; set; }
    }
}

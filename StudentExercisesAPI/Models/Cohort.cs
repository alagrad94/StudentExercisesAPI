using System;
using System.Collections.Generic;

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
        public string CohortName { get; set; }
        public List<Student> StudentList { get; set; }
        public List<Instructor> InstructorList { get; set; }
    }
}

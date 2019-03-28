namespace StudentExercisesAPI.Models {

    public class Assignment {

        public Assignment() {

            ExerciseId = 0;
            StudentId = 0;
        }

        public Assignment(int id, int exerciseId, int studentId) {

            Id = id;
            ExerciseId = exerciseId;
            StudentId = studentId;
        }

        public int Id { get; set; }
        public int ExerciseId { get; set;  }
        public int StudentId { get; set; }
    }
}

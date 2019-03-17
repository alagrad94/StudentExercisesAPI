using System;
using System.Collections.Generic;

namespace StudentExercisesAPI.Models { 

  public class Exercise {

    public Exercise (int id, string exerciseName, string exerciseLanguage) {

        Id = id;
        ExerciseName = exerciseName;
        ExerciseLanguage = exerciseLanguage;
    }

    public Exercise(string exerciseName, string exerciseLanguage) {
        
        ExerciseName = exerciseName;
        ExerciseLanguage = exerciseLanguage;
    }

    public int Id { get; set; }
    public string ExerciseName { get; set; }
    public string ExerciseLanguage { get; set; }

  }
}

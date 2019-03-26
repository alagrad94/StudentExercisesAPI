using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StudentExercisesAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System;

namespace StudentExercisesAPI.Controllers {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    [Route("api/exercises")]
    [ApiController]

    public class ExercisesController: ControllerBase {

        private readonly IConfiguration _config;

        public ExercisesController(IConfiguration config) {

            _config = config;
        }

        public SqlConnection Connection {

            get {

                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }

        // GET api/exercises
        [HttpGet]
        public async Task<IActionResult> Get(string name= "", string language= "", string include = "") {

            string searchName = (name == "") ? "%" : name;
            string searchLang = (language == "") ? "%" : language;

            if (include != "students") {

                using (SqlConnection conn = Connection)
                {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {

                        cmd.CommandText = $@"SELECT id, ExerciseName, ExerciseLanguage 
                                               FROM Exercise 
                                              WHERE (ExerciseLanguage LIKE '{searchLang}' AND ExerciseName LIKE '{searchName}')";

                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Exercise> exercises = new List<Exercise>();

                        while (reader.Read())
                        {

                            Exercise exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("id")),
                                reader.GetString(reader.GetOrdinal("ExerciseName")),
                                reader.GetString(reader.GetOrdinal("ExerciseLanguage")));

                            exercises.Add(exercise);
                        }

                        reader.Close();

                        return Ok(exercises);
                    }
                }
            } else {

                List<Exercise> exercises = new List<Exercise>();

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT id, ExerciseName, ExerciseLanguage 
                                               FROM Exercise 
                                              WHERE (ExerciseLanguage LIKE '{searchLang}' AND ExerciseName LIKE '{searchName}')";

                        SqlDataReader reader = cmd.ExecuteReader();


                        while (reader.Read()) {

                            Exercise exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("id")),
                                reader.GetString(reader.GetOrdinal("ExerciseName")),
                                reader.GetString(reader.GetOrdinal("ExerciseLanguage")));

                            exercises.Add(exercise);
                        }

                        reader.Close();
                    }
                }
                using (SqlConnection conn2 = Connection) {

                    conn2.Open();

                    using (SqlCommand cmd = conn2.CreateCommand()) {

                        foreach (Exercise exercise in exercises) {

                            cmd.CommandText = $@"SELECT s.id, s.FirstName, s.LastName, s.SlackHandle, s.CohortId
                                                   FROM AssignedExercise ae 
                                                   JOIN Student s ON ae.StudentId = s.id
                                                  WHERE ae.ExerciseId = {exercise.Id}";

                            SqlDataReader reader = cmd.ExecuteReader();

                            while (reader.Read()) {

                                Student student = new Student(reader.GetInt32(reader.GetOrdinal("id")),
                                 reader.GetString(reader.GetOrdinal("FirstName")),
                                 reader.GetString(reader.GetOrdinal("LastName")),
                                 reader.GetString(reader.GetOrdinal("SlackHandle")),
                                 reader.GetInt32(reader.GetOrdinal("CohortId")));

                                exercise.ExerciseStudents.Add(student);
                            }

                            reader.Close();
                        }

                        return Ok(exercises);
                    }
                }
            }
        }

        // GET api/exercises/5
        [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute] int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT id, ExerciseName, ExerciseLanguage 
                                           FROM Exercise 
                                          WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = null;

                    while (reader.Read()) {

                        exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetString(reader.GetOrdinal("ExerciseName")),
                            reader.GetString(reader.GetOrdinal("ExerciseLanguage")));
                    }

                    reader.Close();

                    return Ok(exercise);
                }
            }
        }


        // POST api/exercises
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercise exercise) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"INSERT INTO Exercise (ExerciseName, ExerciseLanguage)
                                         OUTPUT INSERTED.Id
                                         VALUES (@exerciseName, @exerciseLanguage)
                                         SELECT MAX(Id) 
                                           FROM Exercise";

                    cmd.Parameters.Add(new SqlParameter("@exerciseName", exercise.ExerciseName));
                    cmd.Parameters.Add(new SqlParameter("@exerciseLanguage", exercise.ExerciseLanguage));

                    int newId = (int)cmd.ExecuteScalar();
                    exercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
                }
            }
        }

        // PUT api/exercises/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Exercise exercise) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"UPDATE Exercise
                                                SET ExerciseName = @exerciseName, ExerciseLanguage = @exerciseLanguage
                                              WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@exerciseName", exercise.ExerciseName));
                        cmd.Parameters.Add(new SqlParameter("@exerciseLanguage", exercise.ExerciseLanguage));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0) {

                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }

                        throw new Exception("No rows affected");
                    }
                }
            }

            catch (Exception) {

                if (!ExerciseExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        // DELETE api/exercises/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"DELETE FROM Exercise 
                                                   WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0) {

                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }

                        throw new Exception("No rows affected");
                    }
                }
            }

            catch (Exception) {

                if (!ExerciseExists(id)) {

                    return NotFound();

                }

                throw;
            }
        }

        private bool ExerciseExists(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT Id, ExerciseName, ExerciseLanguage 
                                           FROM Exercise 
                                          WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

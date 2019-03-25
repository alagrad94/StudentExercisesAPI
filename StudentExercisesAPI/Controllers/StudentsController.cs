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
    [Route("api/students")]
    [ApiController]

    public class StudentsController : ControllerBase {

        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config) {

            _config = config;
        }

        public SqlConnection Connection {

            get {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }

        // GET api/students
        [HttpGet]
        public async Task<IActionResult> Get(string firstName = "", string lastName = "", string slackHandle = "", string include = "") {

            string searchFN = (firstName == "") ? "%" : firstName;
            string searchLN = (lastName == "") ? "%" : lastName;
            string searchSH = (slackHandle == "") ? "%" : slackHandle;

            if (include != "exercise") {

                List<Student> students = new List<Student>();

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"SELECT id, FirstName, LastName, SlackHandle, CohortId FROM Student
                                              WHERE (FirstName LIKE '{searchFN}' AND LastName LIKE '{searchLN}' AND SlackHandle LIKE '{searchSH}')";

                        SqlDataReader reader = cmd.ExecuteReader();


                        while (reader.Read()) {

                            Student student = new Student(reader.GetInt32(reader.GetOrdinal("id")),
                                reader.GetString(reader.GetOrdinal("FirstName")),
                                reader.GetString(reader.GetOrdinal("LastName")),
                                reader.GetString(reader.GetOrdinal("SlackHandle")),
                                reader.GetInt32(reader.GetOrdinal("CohortId")));

                            students.Add(student);
                        }

                        reader.Close();

                    }
                }

                using (SqlConnection conn2 = Connection) {

                    conn2.Open();
                    using (SqlCommand cmd = conn2.CreateCommand()) {

                        foreach (Student student in students) {

                            cmd.CommandText = $@"SELECT id, CohortName FROM Cohort
                                          WHERE id = {student.CohortId}";

                            SqlDataReader reader = cmd.ExecuteReader();

                            while (reader.Read()) {

                                student.Cohort = new Cohort(reader.GetInt32(reader.GetOrdinal("id")),
                                reader.GetString(reader.GetOrdinal("CohortName")));
                            }

                            reader.Close();
                        }

                        return Ok(students);
                    }
                }

            } else {
           
                List<Student> students = new List<Student>();

                using (SqlConnection conn = Connection) {

                    conn.Open();

                    using (SqlCommand cmd = conn.CreateCommand()) {

                        // String interpolation lets us inject the id passed into this method.
                        cmd.CommandText = $@"SELECT id, FirstName, LastName, SlackHandle, CohortId FROM Student
                                              WHERE (FirstName LIKE '{searchFN}' AND LastName LIKE '{searchLN}' AND SlackHandle LIKE '{searchSH}')";
                        SqlDataReader reader = cmd.ExecuteReader();


                        while (reader.Read()) {

                            Student student = new Student(reader.GetInt32(reader.GetOrdinal("id")),
                                 reader.GetString(reader.GetOrdinal("FirstName")),
                                 reader.GetString(reader.GetOrdinal("LastName")),
                                 reader.GetString(reader.GetOrdinal("SlackHandle")),
                                 reader.GetInt32(reader.GetOrdinal("CohortId")));

                            students.Add(student);
                        }

                        reader.Close();
                    }
                }

                using (SqlConnection conn2 = Connection) {

                    conn2.Open();

                    using (SqlCommand cmd = conn2.CreateCommand()) {

                        foreach (Student student in students) {

                            cmd.CommandText = $@"SELECT e.id, e.ExerciseName, e.ExerciseLanguage
                                       FROM AssignedExercise ae JOIN Exercise e ON ae.ExerciseId = e.id
                                       WHERE ae.StudentId = {student.Id}";

                            SqlDataReader reader = cmd.ExecuteReader();

                            while (reader.Read()) {

                                Exercise exercise = new Exercise(reader.GetInt32(reader.GetOrdinal("id")),
                                    reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    reader.GetString(reader.GetOrdinal("ExerciseLanguage")));

                                student.AssignedExercises.Add(exercise);
                            }

                            reader.Close();
                        }

                    }
                }

                using (SqlConnection conn3 = Connection) {

                    conn3.Open();
                    using (SqlCommand cmd = conn3.CreateCommand()) {

                        foreach (Student student in students) {

                            cmd.CommandText = $@"SELECT id, CohortName FROM Cohort
                                          WHERE id = {student.CohortId}";

                            SqlDataReader reader = cmd.ExecuteReader();

                            while (reader.Read()) {

                                student.Cohort = new Cohort(reader.GetInt32(reader.GetOrdinal("id")),
                                reader.GetString(reader.GetOrdinal("CohortName")));
                            }

                            reader.Close();
                        }

                        return Ok(students);
                    }
                }
            }
        }

        // GET api/students/5
        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute] int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = "SELECT id, FirstName, LastName, SlackHandle, CohortId FROM Student WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    while (reader.Read()) {

                        student = new Student(reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetString(reader.GetOrdinal("FirstName")),
                            reader.GetString(reader.GetOrdinal("LastName")),
                            reader.GetString(reader.GetOrdinal("SlackHandle")),
                            reader.GetInt32(reader.GetOrdinal("CohortId")));
                    }

                    reader.Close();

                    return Ok(student);
                }
            }
        }


        // POST api/students
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student student) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId) 
                                         OUTPUT INSERTED.Id
                                         VALUES (@firstName, @lastName, @slackHandle, @cohortId);
                                         SELECT MAX(Id) FROM Student";

                    cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    student.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, student);
                }
            }
        }

        // PUT api/students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Student student) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"UPDATE Student
                                                SET FirstName = @firstName, LastName = @lastName, SlackHandle = @slackHandle, CohortId = @cohortId
                                              WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));
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

                if (!StudentExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        // DELETE api/students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"DELETE FROM Student WHERE Id = @id";
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

                if (!StudentExists(id)) {

                    return NotFound();

                }

                throw;
            }
        }

        private bool StudentExists(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT Id, FirstName, LastName, SlackHandle, CohortId FROM Student WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
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

    [Route("api/instructors")]
    [ApiController]
    public class InstructorsController: ControllerBase {

        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config) {

            _config = config;
        }

        public SqlConnection Connection {

            get {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }

        // GET api/instructors
        [HttpGet]
        public async Task<IActionResult> Get(string q, string firstName = "", string lastName = "", string slackHandle = "") {

            string searchFN = (firstName == "") ? "%" : firstName;
            string searchLN = (lastName == "") ? "%" : lastName;
            string searchSH = (slackHandle == "") ? "%" : slackHandle;

            List<Instructor> instructors = new List<Instructor>();

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT i.id AS instId, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, 
                                                c.CohortName, c.id AS chrtId
                                           FROM Instructor i 
                                           JOIN Cohort c ON i.CohortId = c.id
                                          WHERE i.FirstName LIKE '{searchFN}' AND i.LastName LIKE '{searchLN}' 
                                                AND i.SlackHandle LIKE '{searchSH}'";

                    if (!string.IsNullOrWhiteSpace(q)) {
                        cmd.CommandText += @" AND (i.FirstName LIKE @q OR i.LastName LIKE @q OR i.SlackHandle LIKE @q)";
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read()) {

                        Instructor instructor = new Instructor(reader.GetInt32(reader.GetOrdinal("instId")),
                            reader.GetString(reader.GetOrdinal("FirstName")),
                            reader.GetString(reader.GetOrdinal("LastName")),
                            reader.GetString(reader.GetOrdinal("SlackHandle")),
                            reader.GetInt32(reader.GetOrdinal("CohortId"))) {
                            Cohort = new Cohort(
                                reader.GetInt32(reader.GetOrdinal("chrtId")),
                                reader.GetString(reader.GetOrdinal("CohortName")))};

                        instructors.Add(instructor);
                    }

                    reader.Close();
                    return Ok(instructors);
                }
            }
        }

        // GET api/instructors/5
        [HttpGet("{id}", Name = "GetInstructor")]
        public async Task<IActionResult> Get([FromRoute] int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT i.id AS instId, i.FirstName, i.LastName, i.SlackHandle, i.CohortId, 
                                                c.CohortName, c.id AS chrtId
                                           FROM Instructor i 
                                           JOIN Cohort c ON i.CohortId = c.id
                                           WHERE i.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    while (reader.Read()) {

                        instructor = new Instructor(reader.GetInt32(reader.GetOrdinal("instId")),
                           reader.GetString(reader.GetOrdinal("FirstName")),
                           reader.GetString(reader.GetOrdinal("LastName")),
                           reader.GetString(reader.GetOrdinal("SlackHandle")),
                           reader.GetInt32(reader.GetOrdinal("CohortId"))) {
                           Cohort = new Cohort(
                               reader.GetInt32(reader.GetOrdinal("chrtId")),
                               reader.GetString(reader.GetOrdinal("CohortName")))};
                    }

                    reader.Close();

                    return Ok(instructor);
                }
            }
        }


        // POST api/instructors
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Instructor instructor) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"INSERT INTO Instructor (FirstName, LastName, SlackHandle, CohortId) 
                                         OUTPUT INSERTED.Id
                                         VALUES (@firstName, @lastName, @slackHandle, @cohortId);
                                         SELECT MAX(Id) 
                                           FROM Instructor";

                    cmd.Parameters.Add(new SqlParameter("@firstName", instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackHandle", instructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", instructor.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    instructor.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, instructor);
                }
            }
        }

        // PUT api/instructors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Instructor instructor) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"UPDATE Instructor
                                                SET FirstName = @firstName, LastName = @lastName, 
                                                    SlackHandle = @slackHandle, CohortId = @cohortId
                                              WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@firstName", instructor.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", instructor.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", instructor.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", instructor.CohortId));
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

                if (!InstructorExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        // DELETE api/instructors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"DELETE FROM Instructor 
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

                if (!InstructorExists(id)) {

                    return NotFound();

                }

                throw;
            }
        }

        private bool InstructorExists(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT Id, FirstName, LastName, SlackHandle, CohortId 
                                           FROM Instructor 
                                          WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

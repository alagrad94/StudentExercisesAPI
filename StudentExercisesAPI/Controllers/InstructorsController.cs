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
        public async Task<IActionResult> Get(string firstName = "", string lastName = "", string slackHandle = "") {

            string searchFN = (firstName == "") ? "%" : firstName;
            string searchLN = (lastName == "") ? "%" : lastName;
            string searchSH = (slackHandle == "") ? "%" : slackHandle;

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT id, FirstName, LastName, SlackHandle, CohortId FROM Instructor
                                          WHERE (FirstName LIKE '{searchFN}' AND LastName LIKE '{searchLN}' AND SlackHandle LIKE '{searchSH}')";

                    //WHERE(FirstName LIKE '{searchFN}' AND LastName LIKE '{searchLN}' AND SlackHandle LIKE '{searchSH}')

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();

                    while (reader.Read()) {

                        Instructor instructor = new Instructor(reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetString(reader.GetOrdinal("FirstName")),
                            reader.GetString(reader.GetOrdinal("LastName")),
                            reader.GetString(reader.GetOrdinal("SlackHandle")),
                            reader.GetInt32(reader.GetOrdinal("CohortId")));

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

                    cmd.CommandText = "SELECT id, FirstName, LastName, SlackHandle, CohortId FROM Instructor WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;

                    while (reader.Read()) {

                        instructor = new Instructor(reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetString(reader.GetOrdinal("FirstName")),
                            reader.GetString(reader.GetOrdinal("LastName")),
                            reader.GetString(reader.GetOrdinal("SlackHandle")),
                            reader.GetInt32(reader.GetOrdinal("CohortId")));
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
                                         SELECT MAX(Id) FROM Instructor";

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
                                                SET FirstName = @firstName, LastName = @lastName, SlackHandle = @slackHandle, CohortId = @cohortId
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

                        cmd.CommandText = $@"DELETE FROM Instructor WHERE Id = @id";
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

                    cmd.CommandText = $@"SELECT Id, FirstName, LastName, SlackHandle, CohortId FROM Instructor WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

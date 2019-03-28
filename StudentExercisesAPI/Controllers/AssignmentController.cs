using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercisesAPI.Models;

namespace StudentExercisesAPI.Controllers {

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

    [Route("api/assignments")]
    [ApiController]

    public class AssignmentController : ControllerBase {

        private readonly IConfiguration _config;

        public AssignmentController(IConfiguration config) {

            _config = config;
        }

        public SqlConnection Connection {

            get {

                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }

        // GET api/assignments/5
        [HttpGet("{id}", Name = "GetAssignement")]
        public async Task<IActionResult> Get([FromRoute] int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT id, ExerciseId, StudentId
                                           FROM AssignedExercise
                                          WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Assignment assignment = null;

                    while (reader.Read()) {

                        assignment = new Assignment(
                            reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                            reader.GetInt32(reader.GetOrdinal("StudentId")));
                    }

                    reader.Close();
                    return Ok(assignment);
                }
            }
        }

        // POST api/assignments
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Assignment assignment) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"INSERT INTO AssignedExercise (ExerciseId, StudentId)
                                              OUTPUT INSERTED.Id
                                              VALUES (@exerciseId, @studentId)
                                              SELECT MAX(Id)
                                              FROM AssignedExercise";

                    cmd.Parameters.Add(new SqlParameter("@exerciseId", assignment.ExerciseId));
                    cmd.Parameters.Add(new SqlParameter("@studentId", assignment.StudentId));

                    int newId = (int)cmd.ExecuteScalar();
                    assignment.Id = newId;
                    return CreatedAtRoute("GetAssignement", new { id = newId }, assignment);
                }
            }
        }

        // DELETE api/assignments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"DELETE FROM AssignedExercise 
                                              WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0) {

                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }

                        throw new Exception("No rows affected");
                    }
                }
            } catch (Exception) {

                if (!AssignmentExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        private bool AssignmentExists(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT ExerciseId, StudentId
                                           FROM AssignedExercise 
                                          WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
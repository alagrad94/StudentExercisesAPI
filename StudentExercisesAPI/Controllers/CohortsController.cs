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
    [Route("api/cohorts")]
    [ApiController]

    public class CohortsController: ControllerBase {

        private readonly IConfiguration _config;

        public CohortsController (IConfiguration config) {

            _config = config;
        }

        public SqlConnection Connection {

            get {

                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }

        // GET api/cohorts
        [HttpGet]

        public async Task<IActionResult> Get(string name = "") {

            string searchName =  (name == "") ? "%" : name;

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $"SELECT id, CohortName FROM Cohort WHERE CohortName LIKE '{searchName}'";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read()) {

                        Cohort cohort = new Cohort(reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetString(reader.GetOrdinal("CohortName")));

                        cohorts.Add(cohort);
                    }

                    reader.Close();

                    return Ok(cohorts);
                }
            }
        }

        // GET api/cohorts/5
        [HttpGet("{id}", Name = "GetCohort")]
        public async Task<IActionResult> Get([FromRoute] int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = "SELECT id, CohortName FROM Cohort WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    while (reader.Read()) {

                        cohort = new Cohort(reader.GetInt32(reader.GetOrdinal("id")),
                            reader.GetString(reader.GetOrdinal("CohortName")));
                    }

                    reader.Close();

                    return Ok(cohort);
                }
            }
        }


        // POST api/cohorts
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cohort cohort) {

            using (SqlConnection conn = Connection) {
               
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"INSERT INTO Cohort (CohortName)
                                         OUTPUT INSERTED.Id
                                         VALUES (@cohortName) 
                                         SELECT MAX(Id) FROM Cohort";
                    cmd.Parameters.Add(new SqlParameter("@cohortName", cohort.CohortName));

                    int newId = (int) cmd.ExecuteScalar();
                    cohort.Id = newId;

                    return CreatedAtRoute("GetCohort", new { id = newId }, cohort);
                }
            }
        }

        // PUT api/cohorts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Cohort cohort) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = @"UPDATE Coffee
                                               SET CohortName = @cohortName,
                                             WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@cohortName", cohort.CohortName));
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

                if (!CohortExists(id)) {

                    return NotFound();
                }

                throw;
            }
        }

        // DELETE api/cohorts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {

            try {

                using (SqlConnection conn = Connection) {

                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand()) {

                        cmd.CommandText = $@"DELETE FROM Cohort WHERE Id = @id";
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

                if (!CohortExists(id)) {

                    return NotFound();

                }

                throw;
            }
        }

        private bool CohortExists(int id) {

            using (SqlConnection conn = Connection) {

                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand()) {

                    cmd.CommandText = $@"SELECT Id, CohortName FROM Cohort WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}

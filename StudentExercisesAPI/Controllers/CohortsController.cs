using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StudentExercisesAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Dapper;
using System;

namespace StudentExercisesAPI.Controllers
{

    [Route("api/cohorts")]
    [ApiController]

    public class CohortsController: ControllerBase {

        private readonly IConfiguration _config;

        public CohortsController (IConfiguration config) {

            _config = config;
        }

        public IDbConnection Connection {

            get {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }

        // GET api/cohorts
        [HttpGet]
        public async Task<IActionResult> Get() {

            using (IDbConnection conn = Connection) {

                string sql = "SELECT * FROM Cohort";

                IEnumerable<Cohort> cohorts = await conn.QueryAsync<Cohort>(sql);
                return Ok(cohorts);
            }
        }

        // GET api/cohorts/5
        [HttpGet("{id}", Name = "GetCohort")]
        public async Task<IActionResult> Get([FromRoute] int id) {

            using (IDbConnection conn = Connection) {

                string sql = $"SELECT * FROM Cohort WHERE Id = {id}";

                var singleCohort = (await conn.QueryAsync<Cohort>(sql)).Single();
                return Ok(singleCohort);
            }
        }


        // POST api/cohorts
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cohort cohort) {

            string sql = $@"INSERT INTO Cohort (CohortName)
                                 VALUES ('{cohort.CohortName}');
                                 SELECT MAX(Id) FROM Cohort";

            using (IDbConnection conn = Connection) {

                var newId = (await conn.QueryAsync<int>(sql)).Single();
                cohort.Id = newId;
                return CreatedAtRoute("GetCohort", new { id = newId }, cohort);
            }

        }

        // PUT api/cohorts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Cohort cohort) {
            string sql = $@"UPDATE Exercise
                               SET CohortName ='{cohort.CohortName}'
                             WHERE Id = {id}";

            try {

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync(sql);
                    if (rowsAffected > 0) {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
            catch (Exception) {

                if (!CohortExists(id)) {

                    return NotFound();

                } else {

                    throw;
                }
            }
        }

        // DELETE api/cohorts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {

            string sql = $@"DELETE FROM Cohort WHERE Id = {id}";

            using (IDbConnection conn = Connection) {

                int rowsAffected = await conn.ExecuteAsync(sql);

                if (rowsAffected > 0) {
                    return new StatusCodeResult(StatusCodes.Status204NoContent);
                }

                throw new Exception("No rows affected");
            }

        }

        private bool CohortExists(int id) {

            string sql = $"SELECT Id, CohortName FROM Cohort WHERE Id = {id}";
            using (IDbConnection conn = Connection) {

                return conn.Query<Cohort>(sql).Count() > 0;
            }
        }
    }
}

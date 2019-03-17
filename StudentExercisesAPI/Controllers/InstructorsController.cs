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

    [Route("api/instructors")]
    [ApiController]

    public class InstructorsController: ControllerBase {

        private readonly IConfiguration _config;

        public InstructorsController(IConfiguration config) {

            _config = config;
        }

        public IDbConnection Connection {

            get {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }

        // GET api/instructors
        [HttpGet]
        public async Task<IActionResult> Get() {

            using (IDbConnection conn = Connection) {

                string sql = "SELECT * FROM Instructor";

                IEnumerable<Instructor> instructors = await conn.QueryAsync<Instructor>(sql);
                return Ok(instructors);
            }
        }

        // GET api/instructors/5
        [HttpGet("{id}", Name = "GetInstructor")]
        public async Task<IActionResult> Get([FromRoute] int id) {

            using (IDbConnection conn = Connection) {

                string sql = $"SELECT * FROM Instructor WHERE Id = {id}";

                var singleInstructor = (await conn.QueryAsync<Instructor>(sql)).Single();
                return Ok(singleInstructor);
            }
        }


        // POST api/instructors
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Instructor instructor) {

            string sql = $@"INSERT INTO Instructor (FirstName, LastName, SlackHandle, CohortId)
                                 VALUES ('{instructor.FirstName}', '{instructor.LastName}', 
                                         '{instructor.SlackHandle}', '{instructor.CohortId}');
                                 SELECT MAX(Id) FROM Instructor";

            using (IDbConnection conn = Connection) {

                var newId = (await conn.QueryAsync<int>(sql)).Single();
                instructor.Id = newId;
                return CreatedAtRoute("GetInstructor", new { id = newId }, instructor);
            }

        }

        // PUT api/instructors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Instructor instructor) {
            string sql = $@"UPDATE Instructor
                               SET FirstName = '{instructor.FirstName}', LastName = '{instructor.LastName}',
                                   SlackHandle = '{instructor.SlackHandle}', CohortId = '{instructor.CohortId}'
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

                if (!InstructorExists(id)) {

                    return NotFound();

                } else {

                    throw;
                }
            }
        }

        // DELETE api/instructors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {

            string sql = $@"DELETE FROM Instructor WHERE Id = {id}";

            using (IDbConnection conn = Connection) {

                int rowsAffected = await conn.ExecuteAsync(sql);

                if (rowsAffected > 0) {
                    return new StatusCodeResult(StatusCodes.Status204NoContent);
                }

                throw new Exception("No rows affected");
            }

        }

        private bool InstructorExists(int id) {

            string sql = $"SELECT Id, FirstName, LastName, SlackHandle, CohortId FROM Instructor WHERE Id = {id}";
            using (IDbConnection conn = Connection) {

                return conn.Query<Instructor>(sql).Count() > 0;
            }
        }
    }
}

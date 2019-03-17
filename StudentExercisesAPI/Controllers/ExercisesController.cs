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

    [Route("api/exercises")]
    [ApiController]

    public class ExercisesController: ControllerBase {

        private readonly IConfiguration _config;

        public ExercisesController (IConfiguration config) {

            _config = config;
        }

        public IDbConnection Connection {

            get {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }

        // GET api/exercises
        [HttpGet]
        public async Task<IActionResult> Get() {

            using (IDbConnection conn = Connection) {

                string sql = "SELECT * FROM Exercise";

                IEnumerable<Exercise> exercises = await conn.QueryAsync<Exercise>(sql);
                return Ok(exercises);
            }
        }

        // GET api/exercises/5
        [HttpGet("{id}", Name = "GetExercise")]
        public async Task<IActionResult> Get([FromRoute] int id) {

            using (IDbConnection conn = Connection) {

                string sql = $"SELECT * FROM Exercise WHERE Id = {id}";

                var singleExercise = (await conn.QueryAsync<Exercise>(sql)).Single();
                return Ok(singleExercise);
            }
        }


        // POST api/exercises
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Exercise exercise) {

            string sql = $@"INSERT INTO Exercise (ExerciseName, ExerciseLanguage)
                                 VALUES ('{exercise.ExerciseName}', '{exercise.ExerciseLanguage}');
                                 SELECT MAX(Id) FROM Exercise";

            using (IDbConnection conn = Connection) {

                var newId = (await conn.QueryAsync<int>(sql)).Single();
                exercise.Id = newId;
                return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
            }

        }

        // PUT api/exercises/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Exercise exercise) {
            string sql = $@"UPDATE Exercise
                               SET ExerciseName ='{exercise.ExerciseName}', ExerciseLanguage = '{exercise.ExerciseLanguage}'
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

                if (!ExerciseExists(id)) {

                    return NotFound();

                } else {

                    throw;
                }
            }
        }

        // DELETE api/exercises/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {

            string sql = $@"DELETE FROM Exercise WHERE Id = {id}";

            using (IDbConnection conn = Connection) {

                int rowsAffected = await conn.ExecuteAsync(sql);

                if (rowsAffected > 0) {
                    return new StatusCodeResult(StatusCodes.Status204NoContent);
                }

                throw new Exception("No rows affected");
            }

        }

        private bool ExerciseExists(int id) {

            string sql = $"SELECT Id, ExerciseName, ExerciseLanguage FROM Exercise WHERE Id = {id}";
            using (IDbConnection conn = Connection) {

                return conn.Query<Exercise>(sql).Count() > 0;
            }
        }
    }
}

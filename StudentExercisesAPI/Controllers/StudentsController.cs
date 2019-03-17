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

    [Route("api/students")]
    [ApiController]

    public class StudentsController : ControllerBase
    {

        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {

            _config = config;
        }

        public IDbConnection Connection
        {

            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }

        }

        // GET api/students
        [HttpGet]
        public async Task<IActionResult> Get()
        {

            using (IDbConnection conn = Connection)
            {

                string sql = "SELECT * FROM Student";

                IEnumerable<Student> students = await conn.QueryAsync<Student>(sql);
                return Ok(students);
            }
        }

        // GET api/students/5
        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {

            using (IDbConnection conn = Connection)
            {

                string sql = $"SELECT * FROM Student WHERE Id = {id}";

                var singleStudent = (await conn.QueryAsync<Student>(sql)).Single();
                return Ok(singleStudent);
            }
        }


        // POST api/students
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student student)
        {

            string sql = $@"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId)
                                 VALUES ('{student.FirstName}', '{student.LastName}', 
                                         '{student.SlackHandle}', '{student.CohortId}');
                                 SELECT MAX(Id) FROM Student";

            using (IDbConnection conn = Connection)
            {

                var newId = (await conn.QueryAsync<int>(sql)).Single();
                student.Id = newId;
                return CreatedAtRoute("GetStudent", new { id = newId }, student);
            }

        }

        // PUT api/students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Student student)
        {
            string sql = $@"UPDATE Student
                               SET FirstName = '{student.FirstName}', LastName = '{student.LastName}',
                                   SlackHandle = '{student.SlackHandle}', CohortId = '{student.CohortId}'
                             WHERE Id = {id}";

            try
            {

                using (IDbConnection conn = Connection)
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
            catch (Exception)
            {

                if (!StudentExists(id))
                {

                    return NotFound();

                }
                else
                {

                    throw;
                }
            }
        }

        // DELETE api/students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {

            string sql = $@"DELETE FROM Student WHERE Id = {id}";

            using (IDbConnection conn = Connection)
            {

                int rowsAffected = await conn.ExecuteAsync(sql);

                if (rowsAffected > 0)
                {
                    return new StatusCodeResult(StatusCodes.Status204NoContent);
                }

                throw new Exception("No rows affected");
            }

        }

        private bool StudentExists(int id)
        {

            string sql = $"SELECT Id, FirstName, LastName, SlackHandle, CohortId FROM Student WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {

                return conn.Query<Student>(sql).Count() > 0;
            }
        }
    }
}

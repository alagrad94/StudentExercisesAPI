using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StudentExercisesAPI.Controllers {
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

        // POST api/assignments
        [HttpPost]
        public void Post([FromBody]string value) {
        }



        // DELETE api/assignments/5
        [HttpDelete("{id}")]
        public void Delete(int id) {

        }
    }
}


//public void AddAssignedExercise(Student student, Exercise exercise) {

    //using (SqlConnection conn = Connection) {

    //    conn.Open();

    //    using (SqlCommand cmd = conn.CreateCommand()) {

    //        cmd.CommandText = $@"INSERT INTO AssignedExercise (ExerciseId, StudentId) 
    //                                          VALUES ('{exercise.ExerciseId}', '{student.StudentId}')";
    //        cmd.ExecuteNonQuery();
    //    }
    //}
using Newtonsoft.Json;
using StudentExercisesAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentExercisesAPI.Tests {

    public class StudentTests {

        [Fact]
        public async Task TestCreateStudent() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

                // Construct a new student object to be sent to the API
                Student student = new Student() {

                    FirstName = "Austin",
                    LastName = "Blade", 
                    SlackHandle = "ABlade",
                    CohortId = 3
                };

                // Serialize the C# object into a JSON string
                var studentAsJSON = JsonConvert.SerializeObject(student);

                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/students",
                    new StringContent(studentAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Animal
                var newStudent = JsonConvert.DeserializeObject<Student>(responseBody);


                /* ASSERT */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Austin", newStudent.FirstName);
                Assert.Equal("Blade", newStudent.LastName);
                Assert.Equal("ABlade", newStudent.SlackHandle);
                Assert.Equal(3, newStudent.CohortId);
            }
        }

        [Fact]
        public async Task TestGetStudent() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

              
                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.GetAsync("/api/students");

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Animal
                var studentList = JsonConvert.DeserializeObject<List<Student>>(responseBody);


                /* ASSERT */

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(studentList.Count > 0);
            }
        }

        [Fact]
        public async Task TestUpdateStudent() {

            // New last name to change to and test
            int newCohortId = 4;

            using (var client = new APIClientProvider().Client) {
                /*
                    PUT section
                */
                Student modifiedStudent = new Student {
                    FirstName = "Hunter",
                    LastName = "Metts",
                    SlackHandle = "HunterMetts",
                    CohortId = newCohortId
                };

                var modifiedStudentAsJSON = JsonConvert.SerializeObject(modifiedStudent);

                var response = await client.PutAsync(
                    "/api/students/5",
                    new StringContent(modifiedStudentAsJSON, Encoding.UTF8, "application/json")
                );

                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /*
                    GET section
                    Verify that the PUT operation was successful
                */
                var getStudent = await client.GetAsync("/api/students/5");
                getStudent.EnsureSuccessStatusCode();

                string getStudentBody = await getStudent.Content.ReadAsStringAsync();
                Student newStudent = JsonConvert.DeserializeObject<Student>(getStudentBody);

                Assert.Equal(HttpStatusCode.OK, getStudent.StatusCode);
                Assert.Equal(newCohortId, newStudent.CohortId);
            }

        }

        [Fact]
        public async Task TestDeleteStudent() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */


                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.DeleteAsync("/api/students/7");

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                /* ASSERT */

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                var getStudent = await client.GetAsync("/api/students/7");
                getStudent.EnsureSuccessStatusCode();

                string getStudentBody = await getStudent.Content.ReadAsStringAsync();
                Student newStudent = JsonConvert.DeserializeObject<Student>(getStudentBody);

                Assert.Equal(HttpStatusCode.NoContent, getStudent.StatusCode);
            }

        }
    }
}

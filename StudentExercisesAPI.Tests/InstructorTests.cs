using Newtonsoft.Json;
using StudentExercisesAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentExercisesAPI.Tests {

    public class InstructorTests {

        [Fact]
        public async Task TestCreateInstructor() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

                // Construct a new instructor object to be sent to the API
                Instructor instructor = new Instructor() {

                    FirstName = "Austin",
                    LastName = "Blade", 
                    SlackHandle = "ABlade",
                    CohortId = 3
                };

                // Serialize the C# object into a JSON string
                var instructorAsJSON = JsonConvert.SerializeObject(instructor);

                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/instructors",
                    new StringContent(instructorAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Animal
                var newInstructor = JsonConvert.DeserializeObject<Instructor>(responseBody);


                /* ASSERT */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Austin", newInstructor.FirstName);
                Assert.Equal("Blade", newInstructor.LastName);
                Assert.Equal("ABlade", newInstructor.SlackHandle);
                Assert.Equal(3, newInstructor.CohortId);
            }
        }

        [Fact]
        public async Task TestGetInstructor() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

              
                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.GetAsync("/api/instructors");

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Animal
                var instructorList = JsonConvert.DeserializeObject<List<Instructor>>(responseBody);


                /* ASSERT */

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(instructorList.Count > 0);
            }
        }

        [Fact]
        public async Task TestUpdateInstructor() {

            // New last name to change to and test
            int newCohortId = 4;

            using (var client = new APIClientProvider().Client) {
                /*
                    PUT section
                */
                Instructor modifiedInstructor = new Instructor {
                    FirstName = "Hunter",
                    LastName = "Metts",
                    SlackHandle = "HunterMetts",
                    CohortId = newCohortId
                };

                var modifiedInstructorAsJSON = JsonConvert.SerializeObject(modifiedInstructor);

                var response = await client.PutAsync(
                    "/api/instructors/10",
                    new StringContent(modifiedInstructorAsJSON, Encoding.UTF8, "application/json")
                );

                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /*
                    GET section
                    Verify that the PUT operation was successful
                */
                var getInstructor = await client.GetAsync("/api/instructors/10");
                getInstructor.EnsureSuccessStatusCode();

                string getInstructorBody = await getInstructor.Content.ReadAsStringAsync();
                Instructor newInstructor = JsonConvert.DeserializeObject<Instructor>(getInstructorBody);

                Assert.Equal(HttpStatusCode.OK, getInstructor.StatusCode);
                Assert.Equal(newCohortId, newInstructor.CohortId);
            }

        }

        [Fact]
        public async Task TestDeleteInstructor() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */


                /* ACT */
                // Use the client to send the request and store the response
                var response = await client.DeleteAsync("/api/instructors/10");

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                /* ASSERT */

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                var getInstructor = await client.GetAsync("/api/instructors/10");
                getInstructor.EnsureSuccessStatusCode();

                string getInstructorBody = await getInstructor.Content.ReadAsStringAsync();
                Instructor newInstructor = JsonConvert.DeserializeObject<Instructor>(getInstructorBody);

                Assert.Equal(HttpStatusCode.NoContent, getInstructor.StatusCode);
            }

        }
    }
}

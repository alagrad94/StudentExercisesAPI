using Newtonsoft.Json;
using StudentExercisesAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentExercisesAPI.Tests {

    public class AssignmentTests {

        [Fact]
        public async Task TestCreateAssignment() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

                // Construct a new assignment object to be sent to the API
                Assignment assignment = new Assignment() {

                    ExerciseId = 13,
                    StudentId = 6
                };

                // Serialize the C# object into a JSON string
                var assignementAsJSON = JsonConvert.SerializeObject(assignment);

                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/assignments",
                    new StringContent(assignementAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of AssignedExercise
                var newAssignment = JsonConvert.DeserializeObject<Assignment>(responseBody);

                /* ASSERT */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(13, newAssignment.ExerciseId);
                Assert.Equal(6, newAssignment.StudentId);
            }
        }

        [Fact]
        public async Task TestDeleteExercise() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

               
                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.DeleteAsync($"/api/assignments/1039");

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                /* ASSERT */

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                var getAssignment = await client.GetAsync($"/api/assignments/1039");
                getAssignment.EnsureSuccessStatusCode();

                string getAssignmentBody = await getAssignment.Content.ReadAsStringAsync();
                Assignment newAssignment = JsonConvert.DeserializeObject<Assignment>(getAssignmentBody);

                Assert.Equal(HttpStatusCode.NoContent, getAssignment.StatusCode);
            }

        }
    }
}

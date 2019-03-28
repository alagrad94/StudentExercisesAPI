using Newtonsoft.Json;
using StudentExercisesAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentExercisesAPI.Tests {

    public class ExerciseTests {

        [Fact]
        public async Task TestCreateExercise() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

                // Construct a new exercise object to be sent to the API
                Exercise exercise = new Exercise() {

                    ExerciseName = "Tests",
                    ExerciseLanguage = "CSharp"
                };

                // Serialize the C# object into a JSON string
                var exerciseAsJSON = JsonConvert.SerializeObject(exercise);

                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/exercises",
                    new StringContent(exerciseAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Exercise
                var newExercise = JsonConvert.DeserializeObject<Exercise>(responseBody);


                /* ASSERT */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Tests", newExercise.ExerciseName);
            }
        }

        [Fact]
        public async Task TestGetExercises() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */


                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.GetAsync("/api/exercises");

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Animal
                var exerciseList = JsonConvert.DeserializeObject<List<Exercise>>(responseBody);


                /* ASSERT */

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(exerciseList.Count > 0);
            }
        }

        [Fact]
        public async Task TestUpdateExercise() {

            // New exercise name to change to and test
            string newExerciseName = "More Tests";

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */


                /* PUT section */

                Exercise modifiedExercise = new Exercise {
                    ExerciseName = newExerciseName,
                    ExerciseLanguage = "CSharp"
                };

                var modifiedExerciseAsJSON = JsonConvert.SerializeObject(modifiedExercise);

                var response = await client.PutAsync(
                    $"/api/exercises/1011",
                    new StringContent(modifiedExerciseAsJSON, Encoding.UTF8, "application/json")
                );

                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /* GET section. Verify that the PUT operation was successful */

                var getExercise = await client.GetAsync($"/api/exercises/1011");
                getExercise.EnsureSuccessStatusCode();

                string getExerciseBody = await getExercise.Content.ReadAsStringAsync();
                Exercise newExercise = JsonConvert.DeserializeObject<Exercise>(getExerciseBody);

                Assert.Equal(HttpStatusCode.OK, getExercise.StatusCode);
                Assert.Equal(newExerciseName, newExercise.ExerciseName);
            }

        }

        [Fact]
        public async Task TestDeleteExercise() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

               
                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.DeleteAsync($"/api/exercises/1011");

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                /* ASSERT */

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                var getExercise = await client.GetAsync($"/api/exercises/1011");
                getExercise.EnsureSuccessStatusCode();

                string getExerciseBody = await getExercise.Content.ReadAsStringAsync();
                Exercise newExercise = JsonConvert.DeserializeObject<Exercise>(getExerciseBody);

                Assert.Equal(HttpStatusCode.NoContent, getExercise.StatusCode);
            }

        }
    }
}

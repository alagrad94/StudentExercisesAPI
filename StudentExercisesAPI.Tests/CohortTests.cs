using Newtonsoft.Json;
using StudentExercisesAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StudentExercisesAPI.Tests {

    public class CohortTests {

        [Fact]
        public async Task TestCreateCohort() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

                // Construct a new cohort object to be sent to the API
                Cohort cohort = new Cohort() {

                    CohortName = "Day 40"
                };

                // Serialize the C# object into a JSON string
                var cohortAsJSON = JsonConvert.SerializeObject(cohort);

                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.PostAsync(
                    "/api/cohorts",
                    new StringContent(cohortAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Cohort
                var newCohort = JsonConvert.DeserializeObject<Cohort>(responseBody);


                /* ASSERT */

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Day 40", newCohort.CohortName);
            }
        }

        [Fact]
        public async Task TestGetCohorts() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */


                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.GetAsync("/api/cohorts");

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Animal
                var cohortList = JsonConvert.DeserializeObject<List<Cohort>>(responseBody);


                /* ASSERT */

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(cohortList.Count > 0);
            }
        }

        [Fact]
        public async Task TestUpdateCohort() {

            // New cohort name to change to and test
            string newCohortName = "Day 41";

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

                var getCohortToUpdate = await client.GetAsync("/api/cohorts?name=Day 40");
                getCohortToUpdate.EnsureSuccessStatusCode();

                string getCohortToUpdateBody = await getCohortToUpdate.Content.ReadAsStringAsync();
                var cohortToUpdate = JsonConvert.DeserializeObject <List<Cohort >> (getCohortToUpdateBody);

                int cohortToUpdateId = cohortToUpdate[0].Id;

                /*
                    PUT section
                */
                Cohort modifiedCohort = new Cohort {
                    CohortName = newCohortName
                };

                var modifiedCohortAsJSON = JsonConvert.SerializeObject(modifiedCohort);

                var response = await client.PutAsync(
                    $"/api/cohorts/{cohortToUpdateId}",
                    new StringContent(modifiedCohortAsJSON, Encoding.UTF8, "application/json")
                );

                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);


                /*
                    GET section
                    Verify that the PUT operation was successful
                */
                var getCohort = await client.GetAsync($"/api/cohorts/{cohortToUpdateId}");
                getCohort.EnsureSuccessStatusCode();

                string getCohortBody = await getCohort.Content.ReadAsStringAsync();
                Cohort newCohort = JsonConvert.DeserializeObject<Cohort>(getCohortBody);

                Assert.Equal(HttpStatusCode.OK, getCohort.StatusCode);
                Assert.Equal(newCohortName, newCohort.CohortName);
            }

        }

        [Fact]
        public async Task TestDeleteCohort() {

            using (var client = new APIClientProvider().Client) {
                /* ARRANGE */

                /* ARRANGE */

                var getCohortToUpdate = await client.GetAsync("/api/cohorts?name=Day 41");
                getCohortToUpdate.EnsureSuccessStatusCode();

                string getCohortToUpdateBody = await getCohortToUpdate.Content.ReadAsStringAsync();
                var cohortToUpdate = JsonConvert.DeserializeObject<List<Cohort>>(getCohortToUpdateBody);

                int cohortToUpdateId = cohortToUpdate[0].Id;
                /* ACT */

                // Use the client to send the request and store the response
                var response = await client.DeleteAsync($"/api/cohorts/{cohortToUpdateId}");

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                /* ASSERT */

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                var getCohort = await client.GetAsync($"/api/cohorts/{cohortToUpdateId}");
                getCohort.EnsureSuccessStatusCode();

                string getCohortBody = await getCohort.Content.ReadAsStringAsync();
                Cohort newCohort = JsonConvert.DeserializeObject<Cohort>(getCohortBody);

                Assert.Equal(HttpStatusCode.NoContent, getCohort.StatusCode);
            }

        }
    }
}

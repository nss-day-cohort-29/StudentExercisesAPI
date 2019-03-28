using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercisesAPI.Models;

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CohortsController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public CohortsController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            }
        }


        // GET: api/Cohorts
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Cohorts/5
        [HttpGet("{id}", Name = "GetSingleCohort")]
        public Cohort Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"select c.id, c.[name], 
                                               s.id AS StudentId, s.FirstName AS StudentFirstname, 
                                               s.LastName AS StudentLastName, 
                                               s.SlackHandle AS StudentSlackHandle,
                                               i.id AS InstructorId, i.FirstName AS InstructorFirstName,
                                               i.LastName AS InstructorLastName, 
                                               i.SlackHandle AS InstructorSlackHandle
                                          from cohort c 
                                               left join student s on c.id = s.cohortid
                                               left join Instructor i on c.id = i.CohortId
                                         where c.id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;
                    while (reader.Read())
                    {
                        if (cohort == null)
                        {
                            cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Name = reader.GetString(reader.GetOrdinal("name"))
                            };
                        }

                        if (! reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!cohort.Students.Any(s => s.Id == studentId))
                            {
                                Student student = new Student
                                {
                                    Id = studentId,
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlackHandle")),
                                    CohortId = cohort.Id
                                };
                                cohort.Students.Add(student);
                            }
                        }


                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            int instructorId = reader.GetInt32(reader.GetOrdinal("InstructorId"));
                            if (!cohort.Instructors.Any(i => i.Id == instructorId))
                            {
                                Instructor instructor = new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlackHandle")),
                                    CohortId = cohort.Id
                                };

                                cohort.Instructors.Add(instructor);
                            }
                        }
                    }


                    reader.Close();
                    return cohort;
                }
            }
         }

        // POST: api/Cohorts
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Cohorts/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using TowerDefenseWeb.Models;

namespace TowerDefenseWeb.Controllers
{
    [RoutePrefix("api/Scores")]
    public class ScoresController : ApiController
    {
        TowerDefenseEntities db = new TowerDefenseEntities();

        // GET api/Scores/Top/{number}
        [Route("Top/{number}")]
        [SwaggerOperation("GetTop")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult GetTop(int number)
        {
            List<Score> scores = db.Scores.OrderByDescending(item => item.Value).Take(number).ToList();
            return Ok(scores);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult Post([FromBody]Score body)
        {
            try
            {
                System.Diagnostics.Debug.Print(body.ToString());
                System.Diagnostics.Debug.Print(body.Name);
                System.Diagnostics.Debug.Print(body.Value.ToString());
                System.Diagnostics.Debug.Print(body.Id.ToString());
                System.Diagnostics.Debug.Print(body.Date.ToString());

                if (body == null || body.Name == null || body.Value < 0)
                {
                    throw new DbEntityValidationException();
                }

                Score newScore = new Score
                {
                    Name = body.Name,
                    Value = body.Value,
                    Date = DateTime.Now
                };
                db.Scores.Add(newScore);
                db.SaveChanges();
            } catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.StackTrace);
                return BadRequest();
            }

            return Ok();
        }
    }
}

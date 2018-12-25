using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;
using TowerDefenseWeb.Auth;
using TowerDefenseWeb.Models;

namespace TowerDefenseWeb.Controllers
{
    [RoutePrefix("api/Scores")]
    public class ScoresController : ApiController
    {
        TowerDefenseEntities db = new TowerDefenseEntities();

        // GET api/Scores/Top/{number}
        [HttpGet]
        [Route("Top/{number}")]
        [SwaggerOperation("GetTop")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult GetTop(int number)
        {
            List<Score> scores = db.Scores.OrderByDescending(item => item.Value).ThenBy(item => item.Date).Take(number).ToList();
            return Ok(scores);
        }

        // POST api/Scores
        [HttpPost]
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public IHttpActionResult Post([FromBody]Score body)
        {
            try
            {
                if (body == null || body.Name == null || body.Value < 0)
                {
                    throw new DbEntityValidationException();
                }
                HttpRequestHeaders headers = Request.Headers;
                if (!headers.Contains("AuthToken") || !body.Name.Equals(TokenManager.ValidateToken(headers.GetValues("AuthToken").First()))) body.Name = "Anonymous";
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

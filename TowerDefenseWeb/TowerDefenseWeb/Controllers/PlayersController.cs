using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using TowerDefenseWeb.Auth;
using TowerDefenseWeb.Models;

namespace TowerDefenseWeb.Controllers
{
    [RoutePrefix("api/Players")]
    public class PlayersController : ApiController
    {
        TowerDefenseEntities db = new TowerDefenseEntities();

        // POST: api/Players/Login
        [HttpPost]
        [Route("Login")]
        [SwaggerOperation("Login")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        public IHttpActionResult PostLogin([FromBody]JObject body)
        {
            if (body == null) return BadRequest();
            string name = body["name"]?.ToString();
            string password = body["password"]?.ToString();
            if (name == null || password == null || name == "Anonymous" || name.Length > 10 || name.Length < 1 || password.Length > 128 || password.Length < 6) return Unauthorized();
            string hashedPassword = PasswordManager.HashPassword(password);
            Player player = db.Players.Find(name);
            if (player == null || !player.Password.Equals(hashedPassword)) return Unauthorized(); // Auth failed
            string token = TokenManager.GenerateToken(name);
            player.LastLoginTime = DateTime.Now; // update last login time
            db.SaveChanges();
            return Ok(token);
        }

        // POST: api/Players
        [HttpPost]
        [SwaggerOperation("Login")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public IHttpActionResult PostRegister([FromBody]JObject body)
        {
            if (body == null) return BadRequest();
            string name = body["name"]?.ToString();
            string password = body["password"]?.ToString();
            if (name == null || password == null || name == "Anonymous" || name.Length > 10 || name.Length < 1 || password.Length > 128 || password.Length < 6) return BadRequest();
            Player player = db.Players.Find(name);
            if (player != null) return Conflict(); // player already exists in db
            string hashedPassword = PasswordManager.HashPassword(password);
            Player newPlayer = new Player { Name = name, Password = hashedPassword, LastLoginTime = DateTime.Now };
            string token = TokenManager.GenerateToken(name);
            db.Players.Add(newPlayer);
            db.SaveChanges();
            return Ok(token);
        }

        // PUT: api/Players/{oldName}
        [HttpPut]
        [Route("{oldName}")]
        [SwaggerOperation("Put New Name")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.Unauthorized)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        public IHttpActionResult PutNewName(string oldName, [FromBody]JObject body)
        {
            if (body == null) return BadRequest();
            HttpRequestHeaders headers = Request.Headers;
            if (!headers.Contains("AuthToken")) return Unauthorized(); // no token
            string token = headers.GetValues("AuthToken").First();
            if (oldName == null || oldName == "Anonymous" || !oldName.Equals(TokenManager.ValidateToken(token))) return Unauthorized(); // incorrect token
            string newName = body["name"]?.ToString();
            if (newName == null || newName == "Anonymous" || newName.Length > 10 || newName.Length < 1 || newName.Equals(oldName)) return BadRequest();
            Player newPlayer = db.Players.Find(newName);
            if (newPlayer != null) return Conflict(); // player's name already taken
            Player oldPlayer = db.Players.Find(oldName);
            if (oldPlayer == null) return BadRequest();
            string newToken = TokenManager.GenerateToken(newName);
            db.Players.Add(new Player { Name = newName, Password = oldPlayer.Password, LastLoginTime = oldPlayer.LastLoginTime }); // Add player with the new name
            oldPlayer.Scores.ToList().ForEach(s => s.Name = newName); // Update the Scores table with the new name
            db.Players.Remove(oldPlayer); // Remove old player name
            db.SaveChanges();
            return Ok(newToken);
        }
    }
}

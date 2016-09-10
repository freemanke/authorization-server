using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ResourceServer.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        [HttpGet]
        [Authorize]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        [Authorize]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        [Authorize]
        public void Post([FromBody]string value)
        {
        }

        [HttpPut("{id}")]
        [Authorize]
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpDelete("{id}")]
        [Authorize]
        public void Delete(int id)
        {
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LineFunctionPages.ControllerAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class GPTDataController : ControllerBase
    {
      
        [HttpGet]
        public string Get()
        {


            return "You Get";
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "past text";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public string Post([FromBody] dynamic data)
        {
            return "200";

        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


    }



}

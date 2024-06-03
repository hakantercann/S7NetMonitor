using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using S7Plc.DALs;

namespace S7Plc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupTagsController : ControllerBase
    {
        private readonly DAL_SetupTags dAL_SetupTags = new DAL_SetupTags();
        [HttpGet(Name = "TagAddress")]
        public async Task<ActionResult> Get()
        {
           
            return Ok(dAL_SetupTags.GetAll());

        }
    }
}

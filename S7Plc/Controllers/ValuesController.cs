using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace S7Plc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet(Name = "GetDBAddresses")]
        public async Task<ActionResult> Get()
        {
            string jsonString = @"[
          { 'name':'Tag1', 'TagAddress':'DB1.DW2', 'type':'int', 'skip': 2, 'take':2, 'labelType': 1 },
          { 'name':'Tag2', 'TagAddress':'DB1.DD4', 'type':'float', 'skip': 4, 'take':4, 'labelType': 1 },
          { 'name':'Tag3', 'TagAddress':'DB1.10.STRING10', 'type':'int', 'skip': 10, 'take':12, 'labelType': 1 }
        ]";
            var a = JsonConvert.DeserializeObject<List<Tag>>(jsonString);
            return Ok(a);

        }
    }
    public class Tag
    {
        public int ConfigID { get; set; }
        public string LabelName { get; set; }
        public string TagAddress { get; set; }
        public DataTypeS7 VariableType { get; set; }
        public int SkipByte { get; set; }
        public int TakeByte { get; set; }
        public int LabelType { get; set; }
    }
    public enum DataTypeS7
    {
        Int = 1,
        Real = 2,
        String = 3
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;
using S7.Net.Types;
namespace S7Plc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController1 : ControllerBase
    {
        [HttpGet("{configId}")]
        public async Task<ActionResult> Get(int configId)
        {
            string jsonString = @"[
          {'configID':1, 'name':'Tag1', 'TagAddress':'DB1.DW2', 'type':'Int', 'skip': 0, 'take':2, 'labelType': 1 },
          {'configID':1,  'name':'Tag2', 'TagAddress':'DB1.DD4', 'type':'Real', 'skip': 2, 'take':4, 'labelType': 1 },
          {'configID':1,  'name':'Tag3', 'TagAddress':'DB1.10.STRING10', 'type':'String', 'skip': 10, 'take':12, 'labelType': 1 }
        ]";
            var tags =  JsonConvert.DeserializeObject<List<Tag>>(jsonString);

            var temp = MyBackgroundService.datablockBytes.Where(x => x.ConfigID == configId).FirstOrDefault();
            if (temp != null)
            {
                List<ResultTemplate> results = new List<ResultTemplate>();
                foreach(var item in tags.Where(x => x.ConfigID == configId))
                {
                    var s7Type = Type.GetType("S7.Net.Types." + item.VariableType+ ", S7.Net");
                    if (s7Type != null)
                    {
                        // Get the FromByteArray method info
                        var fromByteArrayMethod = s7Type.GetMethod("FromByteArray", BindingFlags.Static | BindingFlags.Public);
                        if (fromByteArrayMethod != null)
                        {
                            // Take the relevant bytes
                            var relevantBytes = temp.bytes.Skip(item.SkipByte).Take(item.TakeByte).ToArray();

                            // Invoke the method dynamically
                            var result = fromByteArrayMethod.Invoke(null, new object[] { relevantBytes });

                            // Add to the results list
                            results.Add(new ResultTemplate() { Name = item.TagAddress,obj = result });
                        }
                    }
                }
                return Ok(results);
            }
            else
            {
                return NoContent();
            }
           

        }
    }
    public class ResultTemplate
    {
        public string Name { get; set; }
        public Object obj { get; set; }
    }
}

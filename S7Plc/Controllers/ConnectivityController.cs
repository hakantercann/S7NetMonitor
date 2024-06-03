using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using S7.Net;
using System.Linq;
using System.Reflection;

namespace S7Plc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectivityController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] int configId, [FromQuery] string jsonString)
        {
            var tags = JsonConvert.DeserializeObject<List<Tag>>(jsonString);
            if (tags == null)
            {
                return BadRequest();
            }
                var temp = MyBackgroundService.datablockBytes.Where(x => x.ConfigID == configId).FirstOrDefault();
            if (temp != null)
            {
                List<ResultTemplate> results = new List<ResultTemplate>();
                foreach (var item in tags)
                {
                    if (item.VariableType == (DataTypeS7)3)
                    {
                        byte length = S7.Net.Types.Byte.FromByteArray(temp.bytes.Skip(item.SkipByte + 1).Take(1).ToArray());
                        var result = S7.Net.Types.String.FromByteArray(temp.bytes.Skip(item.SkipByte + 2).Take(length).ToArray());
                        results.Add(new ResultTemplate() { Name = item.TagAddress, obj = result });
                        continue;
                    }
                    else
                    {
                        var s7Type = Type.GetType("S7.Net.Types." + item.VariableType + ", S7.Net");
                        if (s7Type != null)
                        {

                            // Get the FromByteArray method info
                            var fromByteArrayMethod = s7Type.GetMethod("FromByteArray", BindingFlags.Static | BindingFlags.Public);
                            if (fromByteArrayMethod != null)
                            {
                                var relevantBytes = temp.bytes.Skip(item.SkipByte).Take(item.TakeByte).ToArray();

                                var result = fromByteArrayMethod.Invoke(null, new object[] { relevantBytes });

                                results.Add(new ResultTemplate() { Name = item.TagAddress, obj = result });
                            }
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
}

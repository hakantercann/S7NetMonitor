using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using S7Plc.DALs;
using S7Plc.Entities;
using System.Data;
using System.Text.RegularExpressions;

namespace S7Plc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupInitController2 : ControllerBase
    {
        private readonly ILogger<SetupInitController2> _logger;
        private readonly DAL_SetupConfigs DAL_SetupConfigs = new DAL_SetupConfigs();
        private readonly DAL_SetupTags DAL_SetupTags = new DAL_SetupTags();
        SqlConnection conn = new SqlConnection("Data Source=.;Initial Catalog=PlcConnArge;User Id=sa;Password=123456;MultipleActiveResultSets=true;TrustServerCertificate=True");


        public SetupInitController2(ILogger<SetupInitController2> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "GetSetupInitController2")]
        public async Task<ActionResult> Post(List<Setup_Tags> entityList)
        {
            Dictionary<int, int> dbByteCounts = new Dictionary<int, int>();
            foreach (var address in entityList.Select(x => x.TagAddress).ToList())
            {
                // Dizi adreslemesi
                var arrayMatch = Regex.Match(address, @"DB(\d+)\.ARRAY(\d+)\[0\.\.(\d+)\] OF (\w+)");
                if (arrayMatch.Success)
                {
                    int dbNumber = int.Parse(arrayMatch.Groups[1].Value);
                    int elementCount = int.Parse(arrayMatch.Groups[3].Value) + 1; // Array length
                    string elementType = arrayMatch.Groups[4].Value;

                    int elementSize = 0;
                    switch (elementType)
                    {
                        case "BOOL":
                            elementSize = 1; // 1 bit, but here counted as 1 byte
                            break;
                        case "BYTE":
                            elementSize = 1; // 1 byte
                            break;
                        case "WORD":
                            elementSize = 2; // 2 bytes
                            break;
                        case "DWORD":
                            elementSize = 4; // 4 bytes
                            break;
                        case "INT":
                            elementSize = 2; // 2 bytes
                            break;
                        case "DINT":
                            elementSize = 4; // 4 bytes
                            break;
                        case "REAL":
                            elementSize = 4; // 4 bytes
                            break;
                            // Add more types if needed
                    }

                    int bytesUsed = elementCount * elementSize;

                    if (!dbByteCounts.ContainsKey(dbNumber))
                    {
                        dbByteCounts[dbNumber] = bytesUsed;
                    }
                    else
                    {
                        dbByteCounts[dbNumber] = Math.Max(dbByteCounts[dbNumber], bytesUsed);
                    }

                    continue;
                }

                // String adreslemesi
                var stringMatch = Regex.Match(address, @"DB(\d+)\.STRING(\d+)\[0\.\.(\d+)\]");
                if (stringMatch.Success)
                {
                    int dbNumber = int.Parse(stringMatch.Groups[1].Value);
                    int stringLength = int.Parse(stringMatch.Groups[3].Value) + 1; // Including terminator

                    // Siemens strings have a fixed overhead of 2 bytes
                    int bytesUsed = 2 + stringLength;

                    if (!dbByteCounts.ContainsKey(dbNumber))
                    {
                        dbByteCounts[dbNumber] = bytesUsed;
                    }
                    else
                    {
                        dbByteCounts[dbNumber] = Math.Max(dbByteCounts[dbNumber], bytesUsed);
                    }

                    continue;
                }

                // Adresi parçalara ayır
                var match = Regex.Match(address, @"DB(\d+)\.DB([XBWLD])(\d+)(?:\.(\d+))?");
                if (match.Success)
                {
                    int dbNumber = int.Parse(match.Groups[1].Value);
                    string type = match.Groups[2].Value;
                    int byteOffset = int.Parse(match.Groups[3].Value);

                    // Byte adresini belirle
                    int bytesUsed = 0;
                    switch (type)
                    {
                        case "X":
                            // Bit adreslemesi
                            int bitOffset = int.Parse(match.Groups[4].Value);
                            bytesUsed = byteOffset + 1;
                            break;
                        case "B":
                            // Byte adreslemesi
                            bytesUsed = byteOffset + 1;
                            break;
                        case "W":
                            // Word adreslemesi (2 byte)
                            bytesUsed = byteOffset + 2;
                            break;
                        case "D":
                            // Double Word adreslemesi (4 byte)
                            bytesUsed = byteOffset + 4;
                            break;
                        case "L":
                            // Long Word adreslemesi (8 byte)
                            bytesUsed = byteOffset + 8;
                            break;
                    }

                    // Sözlükte mevcut değilse ekle, varsa maksimum değeri güncelle
                    if (!dbByteCounts.ContainsKey(dbNumber))
                    {
                        dbByteCounts[dbNumber] = bytesUsed;
                    }
                    else
                    {
                        dbByteCounts[dbNumber] = Math.Max(dbByteCounts[dbNumber], bytesUsed);
                    }
                }
            }
            conn.Open(); SqlTransaction sqlTransaction = conn.BeginTransaction();
            try
            {
                foreach (var item in dbByteCounts)
                {
                    int lastConfigID = 0;
                    using (SqlCommand cmd = new SqlCommand("Insert Into Setup_Configs (db, sizeByte, dataType) " +
                        "VALUES(@p1,@p2,@p3);" +
                        "SELECT SCOPE_IDENTITY()", conn, sqlTransaction))
                    {
                        cmd.Parameters.Add("@p1", SqlDbType.Int).Value =item.Key;
                        cmd.Parameters.Add("@p2", SqlDbType.Int).Value = item.Value;
                        cmd.Parameters.Add("@p3", SqlDbType.Int).Value = 1;

                        lastConfigID = Convert.ToInt32(cmd.ExecuteScalar());

                    }
                    foreach (var subItem in entityList.Where(x=> x.TagAddress.StartsWith("DB" + item.Key)).ToList())
                    {
                        
                        using (SqlCommand cmd = new SqlCommand("Insert Into Setup_Tags (labelName, tagAddress, variableType, skipByte, takeByte, labelType,configID) " +
                              "VALUES (@p1, @p2, @p3, @p4, @p5,@p6,@p7)", conn, sqlTransaction))
                        {
                            cmd.Parameters.Add("@p1", SqlDbType.NVarChar).Value = subItem.LabelName;
                            cmd.Parameters.Add("@p2", SqlDbType.NVarChar).Value = subItem.TagAddress;
                            cmd.Parameters.Add("@p3", SqlDbType.Int).Value = subItem.VariableType;
                            cmd.Parameters.Add("@p4", SqlDbType.Int).Value = subItem.SkipByte;
                            cmd.Parameters.Add("@p5", SqlDbType.Int).Value = subItem.TakeByte;
                            cmd.Parameters.Add("@p6", SqlDbType.Int).Value = subItem.LabelType;
                            cmd.Parameters.Add("@p7", SqlDbType.Int).Value = lastConfigID;

                           cmd.ExecuteNonQuery();

                        }

                    }
                }
                sqlTransaction.Commit();
            }
            catch (Exception ex) {
                try
                    {
                    sqlTransaction.Rollback();

                }
                catch (Exception exRollback)
                {
                    Console.WriteLine("Rollback failed: " + exRollback.Message);
                }
                return StatusCode(500);
            }
            finally
            {
                sqlTransaction.Dispose();
                conn.Close();

            }

            return Ok();
        }
    }
}

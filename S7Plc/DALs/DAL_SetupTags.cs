using Microsoft.Data.SqlClient;
using S7Plc.Entities;
using System.Data;
using System.Diagnostics;

namespace S7Plc.DALs
{
    public class DAL_SetupTags
    {
        SqlConnection conn  =new SqlConnection("Data Source=.;Initial Catalog=PlcConnArge;User Id=sa;Password=123456;MultipleActiveResultSets=true;Trusted_Connection=true;Encrypt=false");
        public int Post(Setup_Tags entity)
        {
            try
            {

                conn.Open();
            }
            catch (Exception ex)
            {
                return 500;
            }
            try
            {
                using (SqlCommand cmd = new SqlCommand("Insert Into Setup_Tags (labelName, tagAddress, variableType, skipByte, takeByte, labelType,configID)" +
                    "VALUES (@p1, @p2, @p3, @p4, @p5,@p6,@p7)", conn))
                {
                    cmd.Parameters.Add("@p1", SqlDbType.NVarChar).Value = entity.LabelName;
                    cmd.Parameters.Add("@p2", SqlDbType.NVarChar).Value = entity.TagAddress;
                    cmd.Parameters.Add("@p3", SqlDbType.Int).Value = entity.VariableType;
                    cmd.Parameters.Add("@p4", SqlDbType.Int).Value = entity.SkipByte;
                    cmd.Parameters.Add("@p5", SqlDbType.Int).Value = entity.TakeByte;
                    cmd.Parameters.Add("@p6", SqlDbType.Int).Value = entity.LabelType;
                    cmd.Parameters.Add("@p7", SqlDbType.Int).Value = entity.ConfigID;
                    cmd.ExecuteNonQuery();
                }


            }
            catch (Exception ex)
            {

                
                return 400;
            }
            finally
            {
                try
                { conn.Close(); }
                catch (Exception ex) { }
            }
            return 200;
        }
        public IEnumerable<Setup_Tags> GetAll()
        {
            try
            {

                conn.Open();
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<Setup_Tags>();
            }
            DataTable dt = new DataTable();
            Setup_Tags Setup_Tags = new Setup_Tags();
            {
                try
                {
                    using (SqlDataAdapter cmd = new SqlDataAdapter("Select * From Setup_Tags", conn))
                    {
                        {

                            cmd.Fill(dt);
                            conn.Close();
                        }
                        return dt.AsEnumerable().Select(x => new Setup_Tags
                        {
                            ID = x.Field<int>("ID"),
                            LabelName = x.Field<string>("labelName"),
                            TagAddress = x.Field<string>("tagaddress"),
                            VariableType = x.Field<int>("variableType"),
                            SkipByte = x.Field<int>("skipByte"),
                            TakeByte = x.Field<int>("takeByte"),
                            LabelType = x.Field<int>("labelType"),
                            ConfigID = x.Field<int>("configID"),
                        });
                    }
    

            }
            catch (Exception ex)
                {

                    return Enumerable.Empty<Setup_Tags>();
                }
                finally
                {
                    try
                    { conn.Close(); }
                    catch (Exception ex) { }
                } }
        }
    }
 

}

using Microsoft.Data.SqlClient;
using S7Plc.Entities;
using System.Data;

namespace S7Plc.DALs
{
    public class DAL_SetupConfigs
    {
        SqlConnection conn = new SqlConnection("Data Source=.;Initial Catalog=PlcConnArge;User Id=sa;Password=123456;MultipleActiveResultSets=true");
        public int Post(Setup_Configs entity)
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
                using (SqlCommand cmd = new SqlCommand("Insert Into Setup_Configs (db, sizeByte, dataType)" +
                    "VALUES (@p1, @p2, @p3)", conn))
                {
                    cmd.Parameters.Add("@p1", SqlDbType.Int).Value = entity.Db;
                    cmd.Parameters.Add("@p2", SqlDbType.Int).Value = entity.SizeByte;
                    cmd.Parameters.Add("@p3", SqlDbType.Int).Value = entity.DataType;
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
    }
}

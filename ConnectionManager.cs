using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace EngemixAnaliseAutomaizador
{
    public class ConnectionManager
    {
        //private readonly string _connectionString = "Data Source=SYNDES;User ID=avl2;password=avldesenv";

        private readonly string _connectionString = "Data Source=AVL2PROD;User ID=avl2;password=avlsynapsis";

        OracleConnection objConn = null;
        OracleTransaction objTransacao = null;

        public string ReadDataString(string queryString)
        {
             string Result = null;
             OracleConnection connection = new OracleConnection(_connectionString);
            connection.Open();

            OracleCommand comando = new OracleCommand(queryString, connection);
            comando.ExecuteNonQuery();

            OracleDataReader dr;
            dr = comando.ExecuteReader();
            dr.Read();

            if (dr.HasRows)
            {
                Result = dr.GetString(0);
            }

            connection.Close();

            return Result;
        }

        public DateTime ReadDataDateTime(string queryString)
        {
            DateTime Result = new DateTime();
             OracleConnection connection = new OracleConnection(_connectionString);
            connection.Open();

            OracleCommand comando = new OracleCommand(queryString, connection);
            comando.ExecuteNonQuery();

            OracleDataReader dr;
            dr = comando.ExecuteReader();
            dr.Read();

            if (dr.HasRows)
            {
                Result = dr.GetDateTime(0);
            }

            connection.Close();

            return Result;
        }
        public ArrayList ReadDataList(string queryString)
        {
            ArrayList lista = new ArrayList();
            using (var conn = new OracleConnection(_connectionString))
            {
                OracleDataAdapter adapter = new OracleDataAdapter();
                adapter.SelectCommand = new OracleCommand(queryString, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                foreach(DataRow row in dt.Rows)
                {
                    lista.Add(row[0].ToString());
                }

            }
            return lista;
        }        
        public ArrayList ReadDataCollum_to_List(string queryString)
        {
            ArrayList lista = new ArrayList();
            using (var conn = new OracleConnection(_connectionString))
            {
                OracleDataAdapter adapter = new OracleDataAdapter();
                adapter.SelectCommand = new OracleCommand(queryString, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                int count = 0;
                foreach(DataColumn col in dt.Columns)
                {                    
                    foreach (DataRow row in dt.Rows)
                    {                        
                        lista.Add(row[count].ToString());                        
                    }
                    count++;
                }

            }
            return lista;
        }
        public DataTable ReadDataTable(string queryString)
        {
            DataTable dt = new DataTable();
            using(var conn = new OracleConnection(_connectionString))
            {
                
                OracleDataAdapter adapter = new OracleDataAdapter();
                adapter.SelectCommand = new OracleCommand(queryString, conn);
                adapter.Fill(dt);
                
            }
            return dt;
        }

        public int ReadDataInt(string queryString)
        {
            int result = 0;
            OracleConnection conexao = new OracleConnection(_connectionString);
            conexao.Open();

            OracleCommand comandos = new OracleCommand(queryString, conexao);
            comandos.ExecuteNonQuery();

            OracleDataReader dr;
            dr = comandos.ExecuteReader();
            dr.Read();

            if (dr.HasRows)
            {
                result = dr.GetInt32(0);
            }
            conexao.Close();

            return result;
        }

    }
}

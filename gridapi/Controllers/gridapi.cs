using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

namespace gridapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class gridapi : ControllerBase
    {
        private IConfiguration _configruration;
        public gridapi(IConfiguration configruration)
        {
            _configruration = configruration;
        }

        [HttpGet]
        [Route("GetGridList")]
        public JsonResult GetGrid()
        {
            string query = "select * from dbo.GridList";
            DataTable table = new DataTable();
            string sqlDatasource = _configruration.GetConnectionString("gridApiDBCon");
            SqlDataReader myReader;
            using(SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();
                using (SqlCommand sqlCommand = new SqlCommand(query, myCon)) {
                    myReader = sqlCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);
        }

        [HttpPost]
        [Route("AddGrid")]
        public JsonResult AddGrid([FromBody] List<gridList> grids)
        {
            foreach (var gridRequest in grids)
            {
                string gridName = gridRequest.gridName;
                List<grid[]> gridList = gridRequest.grid;

                string query = "INSERT INTO dbo.GridList (gridName, grid) VALUES (@gridName, @gridList)";
                string sqlDatasource = _configruration.GetConnectionString("gridApiDBCon");

                using (SqlConnection myCon = new SqlConnection(sqlDatasource))
                {
                    myCon.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(query, myCon))
                    {
                        sqlCommand.Parameters.AddWithValue("@gridName", gridName);
                        sqlCommand.Parameters.AddWithValue("@gridList", JsonConvert.SerializeObject(gridList)); // Serialize the grid data
                        sqlCommand.ExecuteNonQuery();
                    }

                    myCon.Close();
                }
            }

            return new JsonResult(new { message = "Grids added successfully" });
        }

        [HttpPut]
        [Route("UpdateGrid/{id}")]
        public JsonResult UpdateGrid(int id, [FromBody] gridList GridRequest)
        {
            string query = "UPDATE dbo.GridList SET gridName = @gridName, grid = @gridList WHERE id = @gridId";
            string sqlDatasource = _configruration.GetConnectionString("gridApiDBCon");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                using (SqlCommand sqlCommand = new SqlCommand(query, myCon))
                {
                    sqlCommand.Parameters.AddWithValue("@gridId", id);
                    sqlCommand.Parameters.AddWithValue("@gridName", GridRequest.gridName);
                    sqlCommand.Parameters.AddWithValue("@gridList", JsonConvert.SerializeObject(GridRequest.grid));
                    sqlCommand.ExecuteNonQuery();
                }

                myCon.Close();
            }

            return new JsonResult(new { message = "Grid updated successfully" });
        }


        [HttpDelete]
        [Route("DeleteGrid/{id}")]
        public JsonResult DeleteGrid(string id)
        {
            string query = "DELETE FROM dbo.GridList WHERE id = @gridId";
            DataTable table = new DataTable();
            string sqlDatasource = _configruration.GetConnectionString("gridApiDBCon");

            using (SqlConnection myCon = new SqlConnection(sqlDatasource))
            {
                myCon.Open();

                using (SqlCommand sqlCommand = new SqlCommand(query, myCon))
                {
                    sqlCommand.Parameters.AddWithValue("@gridId", id);
                    sqlCommand.ExecuteNonQuery();
                }

                myCon.Close();
            }

            return new JsonResult(new { message = "Grid deleted successfully" });
        }
    }
}

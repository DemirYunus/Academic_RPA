using Microsoft.Data.SqlClient;
using RPA.Context;
using RPA.Entities;
using System.Data;

namespace RPA.DAL
{
	public class GetData
	{
		public GetData() { }

		public DataTable GetProcessData()
		{
			SqlConnection con = new SqlConnection(@"Data Source=94.73.146.3;Initial Catalog=u0987408_Acdmy;Persist Security Info=True;User ID=u0987408_user23E;Password=7VPK-0M-0_zw2g=d;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True");
			con.Open();
			var table = new DataTable();
			using (var da = new SqlDataAdapter("SELECT * FROM Process", con))
			{
				da.Fill(table);
			}

			return table;	
		}

		public DataTable GetProcessInstanceData()
		{
			SqlConnection con = new SqlConnection(@"Data Source=94.73.146.3;Initial Catalog=u0987408_Acdmy;Persist Security Info=True;User ID=u0987408_user23E;Password=7VPK-0M-0_zw2g=d;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True");
			con.Open();
			var table = new DataTable();
			using (var da = new SqlDataAdapter("SELECT * FROM Instance", con))
			{
				da.Fill(table);
			}

			return table;
		}
	}
}

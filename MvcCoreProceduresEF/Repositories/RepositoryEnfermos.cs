using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;
using System.Data.Common;

namespace MvcCoreProceduresEF.Repositories
{
    #region STORED PROCEDURES
    /*
     * 
        create procedure SP_ALL_ENFERMOS
        AS
        select * from ENFERMO
        GO

        create procedure SP_FIND_ENFERMO(@inscripcion nvarchar(50))
        AS
	        select * from ENFERMO WHERE INSCRIPCION=@inscripcion
        GO

        create procedure SP_DELETE_ENFERMO(@inscripcion nvarchar(50))
        AS
	        DELETE from ENFERMO WHERE INSCRIPCION=@inscripcion
        GO
     
     */
    #endregion
    public class RepositoryEnfermos
    {
        private EnfermosContext context;
        public RepositoryEnfermos(EnfermosContext context)
        {
            this.context = context;
        }

        public async Task<List<Enfermo>> GetEnfermosAsync()
        {
            //NECESITAMOS UN DBCOMMAND Y VAMOS A UTILIZAR UN USING PARA TODO
            //,EL COMMAND EN SU CREACIÓN NECESITA DE UNA CADENA DE CONEXION
            //EL OBJETO CONNECTION NOS LO OFRECE EF
            using (DbCommand com=
                this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ENFERMOS";
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                //ABRIMOS LA CONEXION A PARTIR DE COMMAND
                await com.Connection.OpenAsync();
                //EJECUTAMOS NUETSRO READER
                DbDataReader reader = await com.ExecuteReaderAsync();
                //DEBEMOS MAPEAR LOS DATOS MANUALMENTE
                List<Enfermo> enfermos = new List<Enfermo>();
                while (await reader.ReadAsync())
                {
                    Enfermo enfermo = new Enfermo
                    {
                        Inscripcion = reader["INSCRIPCION"].ToString(),
                        Apellido = reader["APELLIDO"].ToString(),
                        Direccion = reader["DIRECCION"].ToString(),
                        FechaNacimiento = DateTime.Parse(reader["FECHA_NAC"].ToString()),
                        Genero = reader["S"].ToString(),
                        Nss = reader["NSS"].ToString(),
                    };
                    enfermos.Add(enfermo);
                }
                //CERRAMOS
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return enfermos;

                
            }
        }


    }
}

using Microsoft.Data.SqlClient;
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

    create procedure SP_INSERT_ENFERMO(@apellido nvarchar(50),@direccion nvarchar(50),@fecha_nac datetime,@genero nvarchar(50), @nss nvarchar(50) )
        AS
            declare @inscripcion int
            select @inscripcion= MAX(INSCRIPCION) FROM ENFERMO 
            select @inscripcion = @inscripcion+1;
            print @inscripcion

            insert into ENFERMO values(@inscripcion,@apellido,@direccion,@fecha_nac,@genero,@nss)
            
        GO
     
     */
    #endregion
    public class RepositoryEnfermos
    {
        private HospitalContext context;
        public RepositoryEnfermos(HospitalContext context)
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
        //FIND ENFERMO
        public async Task<Enfermo> FindEnfermo(string inscripcion)
        {
            //PARA LLAMAR UN PROCEDIMIENTO QUE CONTIENE PARAMS LA LLAMADA SE REALIZA MEDIANTE EL PROCEDURE Y
            //CADA PARAMETRO A CONTINUACION EN LA DECLARACION
            //DEL SQL:SP_PROCEDURE @PAM1 @PAM2

            string sql = "SP_FIND_ENFERMO @inscripcion";
            SqlParameter pamIns = new SqlParameter("@inscripcion", inscripcion);
            //SI LOS DATOS QUE DEVUELVE EL PROCEDURE ESTÁN MAPEADOS
            //CON UN MODEL,PODEMOS UTILIZAR EL METODO FromSqlRaw para recuperar directamente el model/s
            //NO PODEMOS CONSULTAR Y EXTRAER A LA VEZ CON LINQ,SE DEBE REALIZAR EN 2 PASOS

            //o se puede añadir await this.context.Enfermos.FromSqlRaw(sql, pamIns).toListAsync() y abajo 
            //consulta.Enumerable().FirstOrDefault()
            var consulta =
                this.context.Enfermos.FromSqlRaw(sql, pamIns);
            //DEBEMOS UTILIZAR ASENUMERABLE PARA EXTRAER LOS DATOS

            Enfermo enfermo = await consulta.ToAsyncEnumerable().FirstOrDefaultAsync() ;
            return enfermo;

        }
        public async Task DeleteEnfermoAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO";//en este caso no haría falta poner la variable ya que usamos DbCommand
            SqlParameter pamIns = new SqlParameter("@inscripcion", inscripcion);
            using (DbCommand com =
               this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(pamIns);
                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();

                com.Parameters.Clear();
            }

        }

        public async Task DeleteEnfermoRawAsync(string inscripcion)
        {
            string sql = "SP_DELETE_ENFERMO @inscripcion";
            SqlParameter pamIns = new SqlParameter("@inscripcion", inscripcion);

            await this.context.Database
                .ExecuteSqlRawAsync(sql, pamIns);

        }

        public async Task InsertEnfermoAsync(string apellido,string direccion,DateTime fechanac,string genero,string nss)
        {
            string sql = "SP_INSERT_ENFERMO";//en este caso no haría falta poner la variable ya que usamos DbCommand
            SqlParameter pamName = new SqlParameter("@apellido", apellido);
            SqlParameter pamDir = new SqlParameter("@direccion", direccion);
            SqlParameter pamFech = new SqlParameter("@fecha_nac", fechanac);
            SqlParameter pamGen = new SqlParameter("@genero", genero);
            SqlParameter pamNss = new SqlParameter("@nss", nss);

            //(Mirar Referencia DeleteRaw)Se podría hacer con sqlraw(no usaríamos using) pero habría que añadir en sql las variables y 
            // await this.context.Database.ExecuteSqlRawAsync(sql, pamName,pamDir,pamFech,pamGen,pamNss);

            using (DbCommand com =
               this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(pamName);
                com.Parameters.Add(pamDir);
                com.Parameters.Add(pamFech);
                com.Parameters.Add(pamGen);
                com.Parameters.Add(pamNss);
                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();

                com.Parameters.Clear();
                                
            }

        }









    }
}

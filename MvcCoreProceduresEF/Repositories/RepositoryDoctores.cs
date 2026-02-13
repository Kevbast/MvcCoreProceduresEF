using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;
using System.Data.Common;

namespace MvcCoreProceduresEF.Repositories
{
    #region STORED PROCEDURES DOCTOR
    /*
     create procedure SP_ALL_ESPECIALIDADES
        AS
         select distinct(ESPECIALIDAD) from DOCTOR
        GO
        --update

        create procedure SP_UPDATE_ENFERMO(@especialidad nvarchar(50),@incremento int)
        AS
            update DOCTOR set SALARIO=SALARIO + @incremento where ESPECIALIDAD=@especialidad;
        GO

        select * from DOCTOR
        --VER DOCTORES DE ESA ESPECIALIDAD
        create procedure SP_DOCTORESESPECIALIDAD(@especialidad nvarchar(50))
        AS
            select * from DOCTOR where ESPECIALIDAD=@especialidad;
        GO
     */
    #endregion
    public class RepositoryDoctores
    {

        SqlConnection cn;
        SqlCommand com;
        //Implementamos el mismo context
        private HospitalContext context;

        public RepositoryDoctores(HospitalContext context)
        {
            this.context = context;

            string connectionString = @"Data Source=LOCALHOST\DEVELOPER;Initial Catalog=HOSPITAL;Persist Security Info=True;User ID=SA;Encrypt=True;Trust Server Certificate=True";
            this.cn = new SqlConnection(connectionString);
            this.com = new SqlCommand();
            this.com.Connection = this.cn;

        }

        public async Task<List<string>> GetEspecialidades()
        {
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_ALL_ESPECIALIDADES";
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                //ABRIMOS LA CONEXION A PARTIR DE COMMAND
                await com.Connection.OpenAsync();
                //EJECUTAMOS NUETSRO READER
                DbDataReader reader = await com.ExecuteReaderAsync();
                //DEBEMOS MAPEAR LOS DATOS MANUALMENTE
                List<string> especialidades = new List<string>();
                while (await reader.ReadAsync())
                {
                    especialidades.Add(reader["ESPECIALIDAD"].ToString());
                }

                //CERRAMOS
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return especialidades;

            }

        }

        //ahora procedemos con los 2 tipos de updates

        public async Task UpdateDoctorAsync(string especialidad,int incremento)
        {
            string sql = "SP_UPDATE_ENFERMO";//en este caso no haría falta poner la variable ya que usamos DbCommand
            SqlParameter pamEsp = new SqlParameter("@especialidad", especialidad);
            SqlParameter pamInc = new SqlParameter("@incremento", incremento);

            //REALMENTE RAW SE HARÍA A PARTIR DE UN MODELO
            //(Mirar Referencia DeleteRaw)Se podría hacer con sqlraw(no usaríamos using) pero habría que añadir en sql las variables y 
            // await this.context.Database.ExecuteSqlRawAsync(sql, pamEsp,pamInc);

            using (DbCommand com =
               this.context.Database.GetDbConnection().CreateCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(pamEsp);
                com.Parameters.Add(pamInc);
                await com.Connection.OpenAsync();
                await com.ExecuteNonQueryAsync();
                await com.Connection.CloseAsync();

                com.Parameters.Clear();

            }

        }

        //POR ULTIMO EL UPDATE
        public async Task UpdateDoctorSinEFAsync
            (string especialidad, int incremento)
        {
            
            //DEBEMOS RECUPERAR LOS DATOS A MODIFICAR/ELIMINAR
            //DESDE CONTEXT
            var consulta = from datos in this.context.Doctores where datos.Especialidad == especialidad
                           select datos;

            foreach (Doctor doc in consulta)
            {
                doc.Salario += incremento;
            }
            
            //NO TENEMOS NINGÚN MÉTODO PARA REALIZAR UPDATE DENTRO DE LAS COLECCIONES
            await this.context.SaveChangesAsync();
        }

        //VEMOS LOS DOCTORES POR SU ESPECIALIDAD

        public async Task<List<Doctor>> GetDoctoresEspecialidad(string especialidad)
        {
            using (DbCommand com =
                this.context.Database.GetDbConnection().CreateCommand())
            {
                string sql = "SP_DOCTORESESPECIALIDAD";
                SqlParameter pamEsp = new SqlParameter("@especialidad", especialidad);

                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = sql;
                com.Parameters.Add(pamEsp);

                //ABRIMOS LA CONEXION A PARTIR DE COMMAND
                await com.Connection.OpenAsync();
                //EJECUTAMOS NUETSRO READER
                DbDataReader reader = await com.ExecuteReaderAsync();
                //DEBEMOS MAPEAR LOS DATOS MANUALMENTE
                List<Doctor> doctores = new List<Doctor>();
                while (await reader.ReadAsync())
                {
                    Doctor doctor = new Doctor
                    {
                        IdHospital = int.Parse(reader["HOSPITAL_COD"].ToString()),
                        IdDoctor = int.Parse(reader["DOCTOR_NO"].ToString()),
                        Apellido = reader["APELLIDO"].ToString(),
                        Especialidad = reader["ESPECIALIDAD"].ToString(),
                        Salario = int.Parse(reader["SALARIO"].ToString()),
                    };
                    doctores.Add(doctor);
                }
                //CERRAMOS
                await reader.CloseAsync();
                await com.Connection.CloseAsync();
                return doctores;


            }

        }




    }
}

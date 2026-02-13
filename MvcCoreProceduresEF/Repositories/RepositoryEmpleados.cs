using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCoreProceduresEF.Data;
using MvcCoreProceduresEF.Models;
using System.Data;

#region STORED VIEW EMPLEADOS
/*
 CREATE VIEW V_EMPLEADOS_DEPARTAMENTOS
as
--CREAREMOS UN CMAPO PARA LA CONSULTA NUMERICA(lo rodeamos en isnull)
	select isnull(ROW_NUMBER() over (order by EMP.APELLIDO),0) AS ID,
	EMP.APELLIDO,EMP.OFICIO,EMP.SALARIO,DEPT.DNOMBRE as DEPARTAMENTO
	,DEPT.LOC AS LOCALIDAD from EMP inner join DEPT on EMP.DEPT_NO=DEPT.DEPT_NO
go
 */
#endregion

#region STORED PROCEDURE TRABAJADOR
/*
create view V_TRABAJADORES
AS

	select EMP_NO AS IDTRABAJADOR ,APELLIDO,OFICIO,SALARIO FROM EMP 
	union
	select DOCTOR_NO ,APELLIDO,ESPECIALIDAD,SALARIO FROM DOCTOR 
	union
	select EMPLEADO_NO ,APELLIDO,FUNCION,SALARIO FROM PLANTILLA 

GO

create procedure SP_TRABAJADORES_OFICIO
(@oficio nvarchar(50),
@personas int out,
@media int out,
@suma int out)
AS
	SELECT * FROM V_TRABAJADORES WHERE OFICIO=@oficio

	select @personas = count(IDTRABAJADOR),@media= avg(SALARIO),@SUMA = sum(SALARIO) FROM V_TRABAJADORES where OFICIO = @oficio

GO
 */
#endregion

namespace MvcCoreProceduresEF.Repositories
{
    public class RepositoryEmpleados
    {
        private HospitalContext context;

        public RepositoryEmpleados(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<VistaEmpleado>> GetVistaEmpleadosAsync()
        {
            var consulta = from datos in this.context.VistaEmpleados
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task<TrabajadoresModel> GetTrabajadoresAsync()
        {//CON LINQ PRIMERO
            var consulta = from datos in this.context.Trabajadores
                           select datos;
            TrabajadoresModel model = new TrabajadoresModel();
            model.Trabajadores = await consulta.ToListAsync();
            model.Personas = await consulta.CountAsync();
            model.SumaSalarial = await consulta.SumAsync(z=>z.Salario);
            model.MediaSalarial = (int)await consulta.AverageAsync(z => z.Salario);

            return model;

        }

        public async Task<List<string>> GetOficiosAsync()
        {
            var consulta = (from datos in this.context.Trabajadores
                            select datos.Oficio).Distinct();

            return await consulta.ToListAsync();
        }

        //CON ENTITYFRAMEWORK Y PROCEDURE
        public async Task<TrabajadoresModel> GetTrabajadoresOficoAsync(string oficio)
        {
            //YA QUE TENEMOS EL MODEL,VAMOS A LLAMARLO CON EF
            //LA UNICA DIFERENCIA CUAMDO TENE,OS PARAMETROS DE SALIDA
            //ES INDICAR OUT EN LA DECLARACION DE LAS VARIABLES
            string sql = "SP_TRABAJADORES_OFICIO @oficio, @personas out,@media out,@suma out";

            SqlParameter pamOficio = new SqlParameter("@oficio", oficio);
            //LOS PARAMS DE SALIDA TIENEN QUE TENER UN VALOR POR DEFECTO
            SqlParameter pamPersonas = new SqlParameter("@personas", -1);
            pamPersonas.Direction = ParameterDirection.Output;
            SqlParameter pamMedia = new SqlParameter("@media", -1);
            pamMedia.Direction = ParameterDirection.Output;
            SqlParameter pamSuma = new SqlParameter("@suma", 1);
            pamSuma.Direction = ParameterDirection.Output;

            //EJECUTAMOS LA CONSULTA CON EL MODEL FromSqlRaw
            var consulta = this.context.Trabajadores.FromSqlRaw(sql, pamOficio, pamPersonas, pamMedia, pamSuma);

            TrabajadoresModel model = new TrabajadoresModel();
            //PRIMERO QUE RECOJA TODOS LOS EMPLEADOS
            model.Trabajadores = await consulta.ToListAsync();

            model.Personas = int.Parse(pamPersonas.Value.ToString());
            model.MediaSalarial = int.Parse(pamMedia.Value.ToString());
            model.SumaSalarial = int.Parse(pamSuma.Value.ToString());

            return model;


        }







    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MvcCoreProceduresEF.Models
{
    [Table("V_EMPLEADOS_DEPARTAMENTOS")]
    public class VistaEmpleado
    {
        //cuando es un campo calculado(Id) la bbdd hace lo que quiere,en este caso es int.64,se podria 
        //poner en vez de int => int64 o la segunda opción sería modificar el tipado de la vista
        //mediante CAST O CONVERT
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("APELLIDO")]
        public string Apellido { get; set; }
        [Column("OFICIO")]
        public string Oficio { get; set; }
        [Column("SALARIO")]
        public int Salario { get; set; }
        [Column("DEPARTAMENTO")]
        public string Departamento { get; set; }
        [Column("LOCALIDAD")]
        public string Localidad { get; set; }


    }
}

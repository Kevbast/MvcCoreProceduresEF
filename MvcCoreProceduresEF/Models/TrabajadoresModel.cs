namespace MvcCoreProceduresEF.Models
{
    public class TrabajadoresModel
    {//esta es externa,no tiene nada que ver con la bbdd,la usaremos para almacenar 
        public List<Trabajador> Trabajadores { get; set; }
        public int Personas { get; set; }
        public int SumaSalarial { get; set; }
        public int MediaSalarial { get; set; }


        public TrabajadoresModel()
        {
            this.Trabajadores = new List<Trabajador>();
        }

    }
}

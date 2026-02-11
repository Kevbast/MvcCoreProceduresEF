using Microsoft.AspNetCore.Mvc;
using MvcCoreProceduresEF.Models;
using MvcCoreProceduresEF.Repositories;

namespace MvcCoreProceduresEF.Controllers
{
    public class DoctoresController : Controller
    {
        private RepositoryDoctores repo;

        public DoctoresController(RepositoryDoctores repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<string> especialidades = await this.repo.GetEspecialidades();
            ViewData["ESPECIALIDADES"] = especialidades;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(string especialidad, int incremento,string btn)
        {
            List<string> especialidades = await this.repo.GetEspecialidades();
            ViewData["ESPECIALIDADES"] = especialidades;
            if (btn== "procedure")
            {
                await this.repo.UpdateDoctorAsync(especialidad, incremento);

            } else if(btn == "sinprocedure")
            {
                await this.repo.UpdateDoctorSinEFAsync(especialidad, incremento);
            }
            
            List<Doctor> doctores = await this.repo.GetDoctoresEspecialidad(especialidad);
            return View(doctores);
        }


    }
}

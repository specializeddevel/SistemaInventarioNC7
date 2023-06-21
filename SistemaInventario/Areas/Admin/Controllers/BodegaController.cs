using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using SistemaInventario.Data.Repositorio.IRepositorio;
using SistemaInventario.Model;
using SistemaInventario.Utils;

namespace SistemaInventario.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BodegaController : Controller
    {

        private readonly IUnidadTrabajo _unidadTrabajo;

        public BodegaController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {

            Bodega bodega = new Bodega();

            if (id == null)
            {
                //se trata de un registro nuevo
                bodega.Estado = true;
                return View(bodega);
            }
            //se trata de una actualizacion a un registro existente
            bodega = await _unidadTrabajo.Bodega.Obtener(id.GetValueOrDefault()); //GetValueOrDefault() permite la recepcion del id nulo
            if (bodega == null)
            {
                //si la bodega no se encontro se retorna notfound
                return NotFound();
            }
            return View(bodega);
        }

        /******
        * creacion y actulizacion de los datos
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Bodega bodega)
        {
            if (ModelState.IsValid)
            {
                if (bodega.Id == 0)
                {
                    //verificamos si el nombre de Bodega existe
                    var resultadoJson = await ValidarNombre(bodega.Nombre);
                    var jsonResult = (JsonResult)resultadoJson;                    
                    var jsonData = jsonResult.Value;
                    JObject jsonObject = JObject.FromObject(jsonData);
                    bool existe = (bool)jsonObject["data"];
                    if(existe)
                    {
                        TempData[DS.Error] = "Ya existe una bodega con ese nombre!";
                        return View(bodega);
                    } else {
                        await _unidadTrabajo.Bodega.Adicionar(bodega);
                        //muestra el toast con el mensaje enviado a la TempData de la vista parcial _Notificaiones
                        TempData[DS.Exitosa] = "Bodega creada exitosamente!";
                    }                    
                } else
                {
                    var resultadoJson = await ValidarNombre(bodega.Nombre, bodega.Id);
                    var jsonResult = (JsonResult)resultadoJson;
                    var jsonData = jsonResult.Value;
                    JObject jsonObject = JObject.FromObject(jsonData);
                    bool existe = (bool)jsonObject["data"];
                    if (existe)
                    {
                        TempData[DS.Error] = "Ya existe una bodega con ese nombre!";
                        return View(bodega);
                    }
                    else
                    {
                        _unidadTrabajo.Bodega.Actualizar(bodega);
                        TempData[DS.Exitosa] = "Bodega actualizada exitosamente!";
                    }
                }
                await _unidadTrabajo.Guardar();
                //
                return RedirectToAction(nameof(Index));
            }
            TempData[DS.Error] = "Error al grabar la Bodega!";
            return View(bodega);
        }


        #region API //metodos que se llaman desde JS

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Bodega.ObtenerTodos();
            return Json(new { data = todos });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var bodegaDb = await _unidadTrabajo.Bodega.Obtener(id);
            if (bodegaDb == null)
            {
                return Json(new { success = false, message = "Error al eliminar la Bodega"});
            }            
            _unidadTrabajo.Bodega.Eliminar(bodegaDb);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Bodega eliminada exitosamente" });
        }

        //El actionName permite que se pueda llamar con ese nombre este metodo desde JS
        //Este metodo controla que no se tenga una misma bodega con el mismo nombre
        [ActionName("ValidarNombre")]
        public async Task<IActionResult> ValidarNombre(string nombre, int id = 0)
        {
            var valor = false;
            var listaBodegas = await _unidadTrabajo.Bodega.ObtenerTodos();
            if (id == 0)
            {
                if (nombre != null)
                {
                    valor = listaBodegas.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
                }
            }
            else
            {
                if (nombre != null)
                {
                    //Controla que no se trate del objeto que se esta editando
                    valor = listaBodegas.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim() && b.Id != id);
                }
            }
            if (valor)
            {
                return Json(new { data = true });
            }
            return Json(new { data = false });
        }

        #endregion



    }
}

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using SistemaInventario.Data.Repositorio.IRepositorio;
using SistemaInventario.Model;
using SistemaInventario.Utils;

namespace SistemaInventario.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MarcaController : Controller
    {

        private readonly IUnidadTrabajo _unidadTrabajo;

        public MarcaController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {

            Marca marca = new Marca();

            if (id == null)
            {
                //se trata de un registro nuevo
                marca.Estado = true;
                return View(marca);
            }
            //se trata de una actualizacion a un registro existente
            marca = await _unidadTrabajo.Marca.Obtener(id.GetValueOrDefault()); //GetValueOrDefault() permite la recepcion del id nulo
            if (marca == null)
            {
                //si la bodega no se encontro se retorna notfound
                return NotFound();
            }
            return View(marca);
        }

        /******
        * creacion y actulizacion de los datos
        */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Marca marca)
        {
            if (ModelState.IsValid)
            {
                if (marca.Id == 0)
                {
                    //verificamos si el nombre de Bodega existe
                    var resultadoJson = await ValidarNombre(marca.Nombre);
                    var jsonResult = (JsonResult)resultadoJson;                    
                    var jsonData = jsonResult.Value;
                    JObject jsonObject = JObject.FromObject(jsonData);
                    bool existe = (bool)jsonObject["data"];
                    if(existe)
                    {
                        TempData[DS.Error] = "Ya existe una marca con ese nombre!";
                        return View(marca);
                    } else {
                        await _unidadTrabajo.Marca.Adicionar(marca);
                        //muestra el toast con el mensaje enviado a la TempData de la vista parcial _Notificaiones
                        TempData[DS.Exitosa] = "Marca creada exitosamente!";
                    }                    
                } else
                {
                    var resultadoJson = await ValidarNombre(marca.Nombre, marca.Id);
                    var jsonResult = (JsonResult)resultadoJson;
                    var jsonData = jsonResult.Value;
                    JObject jsonObject = JObject.FromObject(jsonData);
                    bool existe = (bool)jsonObject["data"];
                    if (existe)
                    {
                        TempData[DS.Error] = "Ya existe una marca con ese nombre!";
                        return View(marca);
                    }
                    else
                    {
                        _unidadTrabajo.Marca.Actualizar(marca);
                        TempData[DS.Exitosa] = "Marca actualizada exitosamente!";
                    }
                }
                await _unidadTrabajo.Guardar();
                //
                return RedirectToAction(nameof(Index));
            }
            TempData[DS.Error] = "Error al grabar la Bodega!";
            return View(marca);
        }


        #region API //metodos que se llaman desde JS

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.Marca.ObtenerTodos();
            return Json(new { data = todos });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var marcaDB = await _unidadTrabajo.Marca.Obtener(id);
            if (marcaDB == null)
            {
                return Json(new { success = false, message = "Error al eliminar la marca"});
            }            
            _unidadTrabajo.Marca.Eliminar(marcaDB);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Marca eliminada exitosamente" });
        }

        //El actionName permite que se pueda llamar con ese nombre este metodo desde JS
        //Este metodo controla que no se tenga una misma bodega con el mismo nombre
        [ActionName("ValidarNombre")]
        public async Task<IActionResult> ValidarNombre(string nombre, int id = 0)
        {
            var valor = false;
            var listaMarcas = await _unidadTrabajo.Marca.ObtenerTodos();
            if (id == 0)
            {
                if (nombre != null)
                {
                    valor = listaMarcas.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
                }
            }
            else
            {
                if (nombre != null)
                {
                    //Controla que no se trate del objeto que se esta editando
                    valor = listaMarcas.Any(b => b.Nombre.ToLower().Trim() == nombre.ToLower().Trim() && b.Id != id);
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

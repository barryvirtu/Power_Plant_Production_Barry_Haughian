using Microsoft.AspNetCore.Mvc;

namespace Power_Plant_Production.Controllers
{
    [ApiController]
    public class PowerPlant : ControllerBase
    {
     


        [HttpPost("api/productionplan")]
        public IActionResult ProductionPlan(Power_Plant_Production_Classes.PowerPlantAPIRequest request)
        {
            try
            {

                return Ok(Power_Plant_Production_Classes.Load.Calculate(request));
            }
            catch (Exception)
            {

                return StatusCode(500);
      
            }
        }
    }
}
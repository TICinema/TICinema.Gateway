using Microsoft.AspNetCore.Mvc;
using Google.Protobuf.WellKnownTypes; // Для использования Empty
using TICinema.Contracts.Protos.Category;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController(CategoryService.CategoryServiceClient categoryClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await categoryClient.GetAllCategoriesAsync(new Empty());
        
        return Ok(response.Categories);
    }
}
using Microsoft.AspNetCore.Mvc;
using TICinema.Contracts.Protos.Movie;
using TICinema.Gateway.Models.Request;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/movies")]
public class MovieController(MovieService.MovieServiceClient movieClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetMoviesRequest dto)
    {
        // Превращаем DTO фронтенда в gRPC сообщение
        var request = new ListMoviesRequest
        {
            Category = dto.Category ?? "",
            Random = dto.Random,
            Limit = dto.Limit ?? 0
        };

        var response = await movieClient.ListMoviesAsync(request);
        return Ok(response.Movies);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        // Используем oneof из proto: заполняем только поле Slug
        var request = new GetMovieRequest { Slug = slug };

        try 
        {
            var response = await movieClient.GetMovieAsync(request);
            return Ok(response.Movie);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound(new { message = "Фильм не найден" });
        }
    }
}
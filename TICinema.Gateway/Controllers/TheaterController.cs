using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TICinema.Contracts.Protos.Theater;
using TICinema.Gateway.Models.DTOs;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/theaters")]
public class TheaterController(TheaterService.TheaterServiceClient theaterClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var request = new ListTheatersRequest();

        var response = await theaterClient.ListTheatersAsync(request);

        return Ok(response.Theaters);
    }
    
    [HttpPost]
    /*[Authorize(Roles = "Admin")]*/
    public async Task<IActionResult> Create([FromBody] CreateTheaterDto dto)
    {
        var request = new CreateTheaterRequest 
        { 
            Name = dto.Name ?? "", 
            Slug = dto.Slug ?? "", 
            Address = dto.Address ?? "", 
            Location = dto.Location ?? "" 
        };

        var response = await theaterClient.CreateTheaterAsync(request);
        return Ok(response);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TICinema.Contracts.Protos.Hall;
using TICinema.Gateway.Models.DTOs;

namespace TICinema.Gateway.Controllers;

[ApiController]
[Route("api/halls")]
public class HallController(HallService.HallServiceClient hallClient) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var response = await hallClient.GetHallAsync(new GetHallRequest { Id = id });
            return Ok(response.Halls);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound(new { message = ex.Status.Detail });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHallDto dto)
    {
        var request = new CreateHallRequest
        {
            Name = dto.Name,
            TheaterId = dto.TheaterId
        };

        // Маппим ряды из DTO в gRPC repeated поле
        request.Rows.AddRange(dto.Rows.Select(r => new RowLayout
        {
            Row = r.Row,
            Columns = r.Columns,
            Type = r.Type,
            Price = r.Price
        }));

        var response = await hallClient.CreateHallAsync(request);
        return Ok(response.Hall);
    }
}
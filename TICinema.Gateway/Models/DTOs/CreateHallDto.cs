namespace TICinema.Gateway.Models.DTOs;

public class CreateHallDto
{
    public string Name { get; set; } = string.Empty;
    public string TheaterId { get; set; } = string.Empty;
    
    public List<CreateHallRowDto> Rows { get; set; } = new();
}
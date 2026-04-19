namespace TICinema.Gateway.Models.DTOs;

public class CreateHallRowDto
{
    public int Row { get; set; }
    public int Columns { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Price { get; set; }
}
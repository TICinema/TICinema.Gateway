using System.ComponentModel.DataAnnotations;

namespace TICinema.Gateway.Models.Request;

public class GetMoviesRequest
{
    public string? Category { get; set; }

    // Логика Random (по умолчанию false)
    public bool Random { get; set; } = false;

    // Лимит (валидация как в видео: от 1 до 100)
    [Range(1, 100)]
    public int? Limit { get; set; }
}
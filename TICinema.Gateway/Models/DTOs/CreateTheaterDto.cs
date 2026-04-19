using System.ComponentModel.DataAnnotations;

namespace TICinema.Gateway.Models.DTOs;

public class CreateTheaterDto
{
    [Required(ErrorMessage = "Название кинотеатра обязательно")]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Слаг должен состоять только из строчных букв, цифр и дефисов")]
    public string? Slug { get; set; } = string.Empty;

    [Required(ErrorMessage = "Адрес не может быть пустым")]
    public string Address { get; set; } = string.Empty;

    public string? Location { get; set; } = string.Empty;
}
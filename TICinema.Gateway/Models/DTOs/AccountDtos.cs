namespace TICinema.Gateway.Models.DTOs;

public record InitChangeDto(string Value); // Для email или телефона
public record ConfirmChangeDto(string Value, string Code);
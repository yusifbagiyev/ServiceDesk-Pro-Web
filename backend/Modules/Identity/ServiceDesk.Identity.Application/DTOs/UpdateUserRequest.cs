namespace ServiceDesk.Identity.Application.DTOs;

public sealed record UpdateUserRequest(string Email, string FullName, string? PhoneNumber, bool WhatsAppOptIn);

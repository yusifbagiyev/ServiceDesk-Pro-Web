namespace ServiceDesk.Identity.Api.Controllers;

public sealed record UpdateContactRequest(string? PhoneNumber, bool WhatsAppOptIn);

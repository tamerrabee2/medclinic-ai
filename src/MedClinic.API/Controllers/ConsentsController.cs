using System.Security.Claims;
using MedClinic.Application.Interfaces;
using MedClinic.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MedClinic.API.Controllers;
[Authorize][ApiController][Route("api/v1/patients/{patientId:guid}/consents")]
public sealed class ConsentsController:IController { }
public record RecordConsentRequest(ConsentType ConsentType,bool IsGranted,DateTime? ExpiresAt,string? Notes);
public record RevokeConsentRequest(string Reason);
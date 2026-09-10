namespace MedClinic.Domain.Exceptions;

public class QuotaReservationExpiredException : Exception
{
    public Guid ReservationId { get; }
    public DateTime ExpiresAtUtc { get; }

    public QuotaReservationExpiredException(Guid reservationId, DateTime expiresAtUtc)
        : base($"Quota reservation '{reservationId}' expired at {expiresAtUtc:u} and cannot be committed.")
    {
        ReservationId = reservationId;
        ExpiresAtUtc = expiresAtUtc;
    }

    public QuotaReservationExpiredException(Guid reservationId, DateTime expiresAtUtc, string message)
        : base(message)
    {
        ReservationId = reservationId;
        ExpiresAtUtc = expiresAtUtc;
    }
}

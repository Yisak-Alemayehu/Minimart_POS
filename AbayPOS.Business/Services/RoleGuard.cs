using AbayPOS.Domain.Enums;
using AbayPOS.Domain.Models;

namespace AbayPOS.Business.Services;

internal static class RoleGuard
{
    public static void RequireManager(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (session.Role != UserRole.Manager)
            throw new UnauthorizedAccessException("This action is available to managers only.");
    }
}

using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace PseApi.Data
{
    /// <summary>
    /// Helpers for database errors raised by <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>.
    /// </summary>
    public static class DbUpdateExceptionExtensions
    {
        /// <summary>
        /// True when the save failed on a unique index or primary key, e.g. because a concurrent
        /// request inserted the same row first.
        /// </summary>
        public static bool IsUniqueViolation(this DbUpdateException exception) =>
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}

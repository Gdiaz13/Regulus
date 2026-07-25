namespace api.Models;

// Every shape a user takes - the stored row, the response the frontends get -
// answers the admin question the same way, so callers never have to know which
// shape they are holding to find out.
public interface IAdminAware
{
    bool IsAdmin { get; }
}

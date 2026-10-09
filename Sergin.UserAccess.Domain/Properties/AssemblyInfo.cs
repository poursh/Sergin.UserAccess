using System.Runtime.CompilerServices;

// The unit tests build a scope tree in memory, which needs the root the migration otherwise seeds.
[assembly: InternalsVisibleTo("Sergin.UserAccess.UnitTests")]

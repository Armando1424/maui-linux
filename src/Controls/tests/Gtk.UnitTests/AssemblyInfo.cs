using Microsoft.Maui.Hosting;
using Xunit;

// GTK must be driven from a single thread; disable parallelism for the whole
// assembly so the handlers never race.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

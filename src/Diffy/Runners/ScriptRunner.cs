using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

namespace Diffy.Runners
{

    /// Generate deltas by reading a script from a configuration file
    /// listing the changed versions of the project source files.
    public class ScriptRunner : Runner {
        public ScriptRunner (Config config) : base (config) { }


        public override IAsyncEnumerable<Delta> SetupDeltas (BaselineArtifacts baselineArtifacts, CancellationToken ct = default)
        {
            return ScriptedPlanInputs (config, baselineArtifacts, ct);
        }

        private static async IAsyncEnumerable<Delta> ScriptedPlanInputs (Config config, BaselineArtifacts baselineArtifacts, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var scriptPath = config.ScriptPath;
            var parser = new Diffy.Script.Json.Parser(scriptPath);
            IReadOnlyCollection<Plan.Change<string,string>> parsed;
            using (var scriptStream = new FileStream(scriptPath, FileMode.Open)) {
                parsed = await parser.ReadAsync (scriptStream, ct);
            }
            var resolver = baselineArtifacts.docResolver;
            var artifacts = parsed.Select(c => new Delta(Plan.Change.Create(ResolveForScript(resolver, c.Document), c.Update)));
            foreach (var a in artifacts) {
                yield return a;
                if (ct.IsCancellationRequested)
                    break;
            }
        }
        private static DocumentId ResolveForScript (DocResolver resolver, string relativePath) {
            if (resolver.TryResolveDocumentId(relativePath, out var id))
                return id;
            throw new DiffyException($"Could not find {relativePath} in {resolver.Project.Name}", exitStatus: 12);
        }

    }
}

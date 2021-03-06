using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Conan.VisualStudio.Core
{
    public class ConanRunner
    {
        private readonly string _executablePath;

        public ConanRunner(string executablePath) =>
            _executablePath = executablePath;

        private string Escape(string arg) =>
            arg.Contains(" ") ? $"\"{arg}\"" : arg;

        public Task<Process> Install(ConanProject project, ConanConfiguration configuration)
        {
            string ProcessArgument(string name, string value) => $"-s {name}={Escape(value)}";

            var path = project.Path;
            const string generatorName = "visual_studio_multi";
            var settingValues = new[]
            {
                ("arch", configuration.Architecture),
                ("build_type", configuration.BuildType),
                ("compiler.toolset", configuration.CompilerToolset),
                ("compiler.version", configuration.CompilerVersion)
            };
            const string options = "--build missing --update";

            var settings = string.Join(" ", settingValues.Where(pair => pair.Item2 != null).Select(pair =>
            {
                var (key, value) = pair;
                return ProcessArgument(key, value);
            }));
            var arguments = $"install {Escape(path)} " +
                            $"-g {generatorName} " +
                            $"--install-folder {Escape(project.InstallPath)} " +
                            $"{settings} {options}";

            var startInfo = new ProcessStartInfo
            {
                FileName = _executablePath,
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = path,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            return Task.Run(() => Process.Start(startInfo));
        }

        public Task<Process> Inspect(ConanProject project)
        {
            var path = project.Path;
            var arguments = $"inspect {Escape(path)} -a name -j";

            var startInfo = new ProcessStartInfo
            {
                FileName = _executablePath,
                Arguments = arguments,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(path),
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            return Task.Run(() => Process.Start(startInfo));
        }
    }
}

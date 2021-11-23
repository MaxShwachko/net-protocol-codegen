using System.Collections.Generic;
using System.IO;
using Generator.Common;
using NetProtocolCodeGen.Editor.Generator.Method;
using NetProtocolCodeGen.Editor.Generator.Parameters;
using NetProtocolCodeGen.Editor.Generator.Result;
using NetProtocolCodeGen.Editor.Generator.Utils;
using NetProtocolCodeGen.Editor.Scheme;

namespace NetProtocolCodeGen.Editor.Generator
{
    public class ProtocolGenerator
    {
        public List<GeneratedFile> GenerateProtocols(DirectoryInfo generationDirectory, string baseNamespace, List<MethodScheme> methodSchemes, Lang lang)
        {
            var files = new List<GeneratedFile>();

            var agents = GetAgents(methodSchemes);
            var basePartialProtocolGenerator = new BasePartialClassGenerator();
            foreach (var agent in agents)
            {
                var directory = new DirectoryInfo(Path.Combine(generationDirectory.FullName, "Methods", agent.FirstCharToUpper()));
                var file = basePartialProtocolGenerator.Generate(directory, baseNamespace, agent);
                files.Add(file);
            }

            var methodGenerator = new MethodGenerator();
            foreach (var methodScheme in methodSchemes)
            {
                var directory = new DirectoryInfo(Path.Combine(generationDirectory.FullName, "Methods", methodScheme.agent.FirstCharToUpper()));
                var file = methodGenerator.Generate(directory, baseNamespace, methodScheme, lang);
                files.Add(file);
            }

            var dir = new DirectoryInfo(Path.Combine(generationDirectory.FullName, "Methods"));
            var methodResultProcessor =
                new MethodResultProcessorGenerator().Generate(dir, baseNamespace, methodSchemes);
            files.Add(methodResultProcessor);
            
            var methodResultReaderRepository =
                new MethodResultReaderRepositoryGenerator().Generate(dir, baseNamespace, methodSchemes);
            files.Add(methodResultReaderRepository);
            
            var methodParametersProcessor =
                new MethodParametersProcessorGenerator().Generate(dir, baseNamespace, methodSchemes);
            files.Add(methodParametersProcessor);
            
            var methodParametersReaderRepository =
                new MethodParametersReaderRepositoryGenerator().Generate(dir, baseNamespace, methodSchemes);
            files.Add(methodParametersReaderRepository);
            
            return files;
        }

        private List<string> GetAgents(List<MethodScheme> methodSchemes)
        {
            var res = new List<string>();
            foreach (var methodScheme in methodSchemes)
            {
                if (!res.Contains(methodScheme.agent))
                {
                    res.Add(methodScheme.agent);
                }
            }
            return res;
        }
    }
}
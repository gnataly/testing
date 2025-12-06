using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

internal class Program
{
    private static readonly SymbolDisplayFormat MethodDisplayFormat =
        new(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
            delegateStyle: SymbolDisplayDelegateStyle.NameOnly,
            extensionMethodStyle: SymbolDisplayExtensionMethodStyle.Default,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeDefaultValue,
            propertyStyle: SymbolDisplayPropertyStyle.ShowReadWriteDescriptor,
            localOptions: SymbolDisplayLocalOptions.None,
            kindOptions: SymbolDisplayKindOptions.None,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    private static async Task<int> Main(string[] args)
    {
        var parsed = ArgumentParser.Parse(args);
        if (parsed is null)
        {
            ArgumentParser.PrintUsage();
            return 1;
        }

        var config = MetricConfig.Load(parsed.ConfigPath);

        TryRegisterMsBuild();
        using var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (_, e) => Console.WriteLine($"[workspace] {e.Diagnostic}");

        var solution = await workspace.OpenSolutionAsync(parsed.SolutionPath);

        var analyzer = new MetricAnalyzer(config);
        var report = await analyzer.AnalyzeAsync(solution);

        MetricReporter.WriteReport(report, parsed.ReportPath);

        if (report.Violations.Count > 0)
        {
            MetricReporter.PrintViolations(report.Violations, config);
            return 2;
        }

        MetricReporter.PrintSummary(report);
        return 0;
    }

    private static void TryRegisterMsBuild()
    {
        try
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
        }
        catch (InvalidOperationException)
        {
            RegisterFallbackInstance();
        }
    }

    private static void RegisterFallbackInstance()
    {
        var instances = MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(i => i.Version).ToArray();
        if (instances.Length > 0)
        {
            MSBuildLocator.RegisterInstance(instances[0]);
            return;
        }

        var envPath = Environment.GetEnvironmentVariable("MSBUILD_PATH");
        if (!string.IsNullOrWhiteSpace(envPath) && Directory.Exists(envPath))
        {
            MSBuildLocator.RegisterMSBuildPath(envPath);
            return;
        }

        var msbuildDir = FindMsBuildDirectory();
        if (msbuildDir is not null)
        {
            MSBuildLocator.RegisterMSBuildPath(msbuildDir);
            return;
        }

        throw new InvalidOperationException("MSBuild не найден. Установите .NET 8 SDK или Visual Studio Build Tools и задайте переменную DOTNET_ROOT/MSBUILD_PATH при необходимости.");
    }

    private static string? FindMsBuildDirectory()
    {
        var candidateFiles = new List<string>();

        // dotnet SDK roots
        foreach (var root in GetDotnetRoots())
        {
            var sdkRoot = Path.Combine(root, "sdk");
            if (!Directory.Exists(sdkRoot))
            {
                continue;
            }

            candidateFiles.AddRange(
                Directory.GetDirectories(sdkRoot)
                    .Select(dir => Path.Combine(dir, "MSBuild.dll"))
                    .Where(File.Exists));
        }

        // VS/BuildTools installations
        var vsRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio");
        if (Directory.Exists(vsRoot))
        {
            candidateFiles.AddRange(
                Directory.EnumerateFiles(vsRoot, "MSBuild.dll", SearchOption.AllDirectories)
                    .Where(path => path.Contains($"{Path.DirectorySeparatorChar}MSBuild{Path.DirectorySeparatorChar}Current{Path.DirectorySeparatorChar}Bin", StringComparison.OrdinalIgnoreCase)));
        }

        var best = candidateFiles
            .Select(path => new { Path = path, Version = TryParseVersion(Path.GetFileName(Path.GetDirectoryName(path) ?? string.Empty)) })
            .OrderByDescending(x => x.Version)
            .Select(x => x.Path)
            .FirstOrDefault();

        return best is null ? null : Path.GetDirectoryName(best);
    }

    private static IEnumerable<string> GetDotnetRoots()
    {
        var roots = new List<string>();

        var envRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(envRoot))
        {
            roots.Add(envRoot);
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrWhiteSpace(programFiles))
        {
            roots.Add(Path.Combine(programFiles, "dotnet"));
        }

        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrWhiteSpace(programFilesX86))
        {
            roots.Add(Path.Combine(programFilesX86, "dotnet"));
        }

        return roots.Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static Version? TryParseVersion(string? text)
    {
        return Version.TryParse(text, out var version) ? version : null;
    }

    private static class ArgumentParser
    {
        public static AnalyzerArguments? Parse(string[] args)
        {
            var solution = "TheatreCenter.sln";
            var report = "artifacts/metrics.json";
            var config = "codequality.config.json";

            var queue = new Queue<string>(args);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (IsHelp(current))
                {
                    return null;
                }

                if (TryReadOption(current, "--solution", "-s", queue, out var value))
                {
                    solution = value;
                    continue;
                }

                if (TryReadOption(current, "--report", "-r", queue, out value))
                {
                    report = value;
                    continue;
                }

                if (TryReadOption(current, "--config", "-c", queue, out value))
                {
                    config = value;
                    continue;
                }

                return null;
            }

            return new AnalyzerArguments(solution, report, config);
        }

        public static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet run --project tools/CodeQuality/TheatreCenter.CodeQuality.csproj -- [--solution|-s] <path> [--report|-r] <path> [--config|-c] <path>");
        }

        private static bool TryReadOption(string token, string longOption, string shortOption, Queue<string> remaining, out string value)
        {
            value = string.Empty;
            if (token != longOption && token != shortOption)
            {
                return false;
            }

            if (remaining.Count == 0)
            {
                return false;
            }

            value = remaining.Dequeue();
            return true;
        }

        private static bool IsHelp(string token) => token is "--help" or "-h";
    }

    private sealed record AnalyzerArguments(string SolutionPath, string ReportPath, string ConfigPath);

    private sealed class MetricAnalyzer
    {
        private readonly MetricConfig _config;

        public MetricAnalyzer(MetricConfig config)
        {
            _config = config;
        }

        public async Task<MetricReport> AnalyzeAsync(Solution solution)
        {
            var methods = new List<MethodMetrics>();
            var violations = new List<MetricViolation>();

            foreach (var project in solution.Projects.Where(p => p.Language == LanguageNames.CSharp))
            {
                await AnalyzeProjectAsync(project, methods, violations);
            }

            return new MetricReport(methods, violations);
        }

        private async Task AnalyzeProjectAsync(Project project, ICollection<MethodMetrics> methods, ICollection<MetricViolation> violations)
        {
            var compilation = await project.GetCompilationAsync();
            if (compilation is null)
            {
                return;
            }

            foreach (var document in project.Documents.Where(d => d.SupportsSyntaxTree))
            {
                await AnalyzeDocumentAsync(project, compilation, document, methods, violations);
            }
        }

        private async Task AnalyzeDocumentAsync(Project project, Compilation compilation, Document document, ICollection<MethodMetrics> methods, ICollection<MetricViolation> violations)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();
            if (syntaxTree is null)
            {
                return;
            }

            var root = await syntaxTree.GetRootAsync();
            var model = compilation.GetSemanticModel(syntaxTree);

            foreach (var methodNode in MethodLocator.Find(root))
            {
                var symbol = MethodLocator.GetSymbol(model, methodNode);
                var methodName = symbol?.ToDisplayString(MethodDisplayFormat) ?? MethodLocator.GetFallbackName(methodNode);

                var halstead = HalsteadCalculator.Calculate(methodNode);
                var complexity = CyclomaticCalculator.Calculate(methodNode);

                var metric = new MethodMetrics(
                    methodName,
                    document.FilePath ?? project.Name,
                    complexity,
                    halstead);

                methods.Add(metric);
                AppendViolations(metric, halstead, complexity, violations);
            }
        }

        private void AppendViolations(MethodMetrics metric, HalsteadMetrics halstead, int complexity, ICollection<MetricViolation> violations)
        {
            if (complexity > _config.CyclomaticComplexityThreshold)
            {
                violations.Add(new MetricViolation(
                    metric.Method,
                    metric.FilePath,
                    "CyclomaticComplexity",
                    complexity,
                    _config.CyclomaticComplexityThreshold));
            }

            if (_config.HalsteadVolumeThreshold.HasValue && halstead.Volume > _config.HalsteadVolumeThreshold.Value)
            {
                violations.Add(new MetricViolation(
                    metric.Method,
                    metric.FilePath,
                    "HalsteadVolume",
                    halstead.Volume,
                    _config.HalsteadVolumeThreshold.Value));
            }

            if (_config.HalsteadDifficultyThreshold.HasValue && halstead.Difficulty > _config.HalsteadDifficultyThreshold.Value)
            {
                violations.Add(new MetricViolation(
                    metric.Method,
                    metric.FilePath,
                    "HalsteadDifficulty",
                    halstead.Difficulty,
                    _config.HalsteadDifficultyThreshold.Value));
            }
        }
    }

    private static class MetricReporter
    {
        public static void WriteReport(MetricReport report, string reportPath)
        {
            var directory = Path.GetDirectoryName(reportPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(reportPath, JsonSerializer.Serialize(report, jsonOptions));
        }

        public static void PrintViolations(IEnumerable<MetricViolation> violations, MetricConfig config)
        {
            Console.WriteLine("❌ Quality gates failed:");
            foreach (var violation in violations)
            {
                Console.WriteLine($"  {violation.Metric}: {violation.Value:F2} (>{violation.Threshold}) in {violation.Method} [{violation.FilePath}]");
            }

            Console.WriteLine();
            Console.WriteLine($"Cyclomatic complexity threshold: {_format(config.CyclomaticComplexityThreshold)}");
            if (config.HalsteadVolumeThreshold.HasValue)
            {
                Console.WriteLine($"Halstead volume threshold: {_format(config.HalsteadVolumeThreshold.Value)}");
            }

            if (config.HalsteadDifficultyThreshold.HasValue)
            {
                Console.WriteLine($"Halstead difficulty threshold: {_format(config.HalsteadDifficultyThreshold.Value)}");
            }

            static string _format(double value) => value.ToString("0.##");
        }

        public static void PrintSummary(MetricReport report)
        {
            Console.WriteLine("✅ Quality gates passed");
            Console.WriteLine($"Analyzed methods: {report.Methods.Count}");
        }
    }

    private static class MethodLocator
    {
        public static IEnumerable<SyntaxNode> Find(SyntaxNode root)
        {
            foreach (var method in root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
            {
                yield return method;
            }

            foreach (var accessor in root.DescendantNodes().OfType<AccessorDeclarationSyntax>())
            {
                yield return accessor;
            }

            foreach (var localFunction in root.DescendantNodes().OfType<LocalFunctionStatementSyntax>())
            {
                yield return localFunction;
            }
        }

        public static ISymbol? GetSymbol(SemanticModel model, SyntaxNode node) =>
            node switch
            {
                BaseMethodDeclarationSyntax method => model.GetDeclaredSymbol(method),
                AccessorDeclarationSyntax accessor => model.GetDeclaredSymbol(accessor),
                LocalFunctionStatementSyntax localFunction => model.GetDeclaredSymbol(localFunction),
                _ => null
            };

        public static string GetFallbackName(SyntaxNode node) =>
            node switch
            {
                MethodDeclarationSyntax method => method.Identifier.Text,
                ConstructorDeclarationSyntax ctor => ctor.Identifier.Text,
                DestructorDeclarationSyntax dtor => dtor.Identifier.Text,
                OperatorDeclarationSyntax op => op.OperatorToken.Text,
                ConversionOperatorDeclarationSyntax conversion => conversion.Type.ToString(),
                AccessorDeclarationSyntax accessor => accessor.Keyword.Text,
                LocalFunctionStatementSyntax localFunction => localFunction.Identifier.Text,
                _ => node.Kind().ToString()
            };
    }

    private static class CyclomaticCalculator
    {
        public static int Calculate(SyntaxNode node)
        {
            var walker = new ComplexityWalker();
            walker.Visit(node);
            return walker.Complexity;
        }

        private sealed class ComplexityWalker : CSharpSyntaxWalker
        {
            public int Complexity { get; private set; } = 1;

            public override void VisitIfStatement(IfStatementSyntax node)
            {
                Complexity++;
                base.VisitIfStatement(node);
            }

            public override void VisitForStatement(ForStatementSyntax node)
            {
                Complexity++;
                base.VisitForStatement(node);
            }

            public override void VisitForEachStatement(ForEachStatementSyntax node)
            {
                Complexity++;
                base.VisitForEachStatement(node);
            }

            public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
            {
                Complexity++;
                base.VisitForEachVariableStatement(node);
            }

            public override void VisitWhileStatement(WhileStatementSyntax node)
            {
                Complexity++;
                base.VisitWhileStatement(node);
            }

            public override void VisitDoStatement(DoStatementSyntax node)
            {
                Complexity++;
                base.VisitDoStatement(node);
            }

            public override void VisitCatchClause(CatchClauseSyntax node)
            {
                Complexity++;
                base.VisitCatchClause(node);
            }

            public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
            {
                Complexity++;
                base.VisitConditionalExpression(node);
            }

            public override void VisitSwitchStatement(SwitchStatementSyntax node)
            {
                Complexity += node.Sections.SelectMany(section => section.Labels).Count();
                base.VisitSwitchStatement(node);
            }

            public override void VisitSwitchExpression(SwitchExpressionSyntax node)
            {
                Complexity += node.Arms.Count;
                base.VisitSwitchExpression(node);
            }

            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.LogicalAndExpression) ||
                    node.IsKind(SyntaxKind.LogicalOrExpression) ||
                    node.IsKind(SyntaxKind.CoalesceExpression))
                {
                    Complexity++;
                }

                base.VisitBinaryExpression(node);
            }
        }
    }

    private static class HalsteadCalculator
    {
        private static readonly ImmutableHashSet<SyntaxKind> OperatorKinds = new[]
        {
            SyntaxKind.PlusToken,
            SyntaxKind.MinusToken,
            SyntaxKind.AsteriskToken,
            SyntaxKind.SlashToken,
            SyntaxKind.PercentToken,
            SyntaxKind.PlusPlusToken,
            SyntaxKind.MinusMinusToken,
            SyntaxKind.LessThanToken,
            SyntaxKind.LessThanEqualsToken,
            SyntaxKind.GreaterThanToken,
            SyntaxKind.GreaterThanEqualsToken,
            SyntaxKind.EqualsEqualsToken,
            SyntaxKind.ExclamationEqualsToken,
            SyntaxKind.AmpersandAmpersandToken,
            SyntaxKind.BarBarToken,
            SyntaxKind.QuestionQuestionToken,
            SyntaxKind.QuestionToken,
            SyntaxKind.ColonToken,
            SyntaxKind.EqualsToken,
            SyntaxKind.PlusEqualsToken,
            SyntaxKind.MinusEqualsToken,
            SyntaxKind.AsteriskEqualsToken,
            SyntaxKind.SlashEqualsToken,
            SyntaxKind.PercentEqualsToken,
            SyntaxKind.QuestionQuestionEqualsToken,
            SyntaxKind.AmpersandToken,
            SyntaxKind.BarToken,
            SyntaxKind.CaretToken,
            SyntaxKind.TildeToken,
            SyntaxKind.LessThanLessThanToken,
            SyntaxKind.GreaterThanGreaterThanToken
        }.ToImmutableHashSet();

        public static HalsteadMetrics Calculate(SyntaxNode node)
        {
            var distinctOperators = new HashSet<string>();
            var distinctOperands = new HashSet<string>();

            var totalOperators = 0;
            var totalOperands = 0;

            foreach (var token in node.DescendantTokens())
            {
                if (IsOperand(token))
                {
                    distinctOperands.Add(token.Text);
                    totalOperands++;
                }
                else if (IsOperator(token))
                {
                    distinctOperators.Add(token.Text);
                    totalOperators++;
                }
            }

            var n1 = distinctOperators.Count;
            var n2 = distinctOperands.Count;
            var N1 = totalOperators;
            var N2 = totalOperands;

            var vocabulary = n1 + n2;
            var length = N1 + N2;

            var volume = vocabulary > 0 ? length * Math.Log2(vocabulary) : 0;
            var difficulty = n2 > 0 ? (n1 / 2.0) * (N2 / (double)n2) : 0;
            var effort = difficulty * volume;

            return new HalsteadMetrics(n1, n2, N1, N2, volume, difficulty, effort);
        }

        private static bool IsOperand(SyntaxToken token) =>
            token.IsKind(SyntaxKind.IdentifierToken) ||
            token.IsKind(SyntaxKind.NumericLiteralToken) ||
            token.IsKind(SyntaxKind.StringLiteralToken) ||
            token.IsKind(SyntaxKind.CharacterLiteralToken) ||
            token.IsKind(SyntaxKind.TrueKeyword) ||
            token.IsKind(SyntaxKind.FalseKeyword) ||
            token.IsKind(SyntaxKind.NullKeyword);

        private static bool IsOperator(SyntaxToken token)
        {
            if (IsOperand(token))
            {
                return false;
            }

            if (token.IsKeyword())
            {
                return true;
            }

            return OperatorKinds.Contains(token.Kind());
        }
    }

    private sealed record MetricConfig(
        int CyclomaticComplexityThreshold,
        double? HalsteadVolumeThreshold,
        double? HalsteadDifficultyThreshold)
    {
        public static MetricConfig Load(string path)
        {
            if (!File.Exists(path))
            {
                return new MetricConfig(10, null, null);
            }

            var config = JsonSerializer.Deserialize<MetricConfig>(File.ReadAllText(path));
            return config ?? new MetricConfig(10, null, null);
        }
    }

    private sealed record HalsteadMetrics(
        int DistinctOperators,
        int DistinctOperands,
        int TotalOperators,
        int TotalOperands,
        double Volume,
        double Difficulty,
        double Effort);

    private sealed record MethodMetrics(
        string Method,
        string FilePath,
        int CyclomaticComplexity,
        HalsteadMetrics Halstead);

    private sealed record MetricViolation(
        string Method,
        string FilePath,
        string Metric,
        double Value,
        double Threshold);

    private sealed record MetricReport(
        List<MethodMetrics> Methods,
        List<MetricViolation> Violations);
}

// using InterDotRush.Common.Extensions;
// using InterDotRush.Roslyn.Server.Extensions;
// using InterDotRush.Roslyn.Server.Services;
// using InterDotRush.Roslyn.Workspaces.Extensions;
// using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
// using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
// using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
// using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentDiagnostic;
// using EmmyLua.LanguageServer.Framework.Server.Handler;
// using Microsoft.CodeAnalysis;

// namespace InterDotRush.Roslyn.Server.Handlers.TextDocument;

// public class DocumentDiagnosticsHandler : DocumentDiagnosticHandlerBase {
//     private readonly WorkspaceService workspaceService;
//     private readonly CodeAnalysisService codeAnalysisService;
//     private readonly ConfigurationService configurationService;

//     public DocumentDiagnosticsHandler(WorkspaceService workspaceService, CodeAnalysisService codeAnalysisService, ConfigurationService configurationService) {
//         this.codeAnalysisService = codeAnalysisService;
//         this.workspaceService = workspaceService;
//         this.configurationService = configurationService;
//     }

//     public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities) {
//         serverCapabilities.DiagnosticProvider ??= new DiagnosticOptions();
//         serverCapabilities.DiagnosticProvider.Identifier = "InterDotRush";
//         serverCapabilities.DiagnosticProvider.InterFileDependencies = true;
//     }

//     protected override Task<DocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken token) {
//         return SafeExtensions.InvokeAsync<DocumentDiagnosticReport>(new RelatedUnchangedDocumentDiagnosticReport(), async () => {
//             var currentSolutionToken = workspaceService.SolutionToken;
//             var documentIds = GetDocumentIdsWithFilePath(request.TextDocument.Uri.FileSystemPath);
//             if (documentIds == null || workspaceService.Solution == null)
//                 return new RelatedUnchangedDocumentDiagnosticReport();

//             var documents = documentIds.Select(id => workspaceService.Solution.GetDocument(id)).WhereNotNull();
//             var diagnostics = await codeAnalysisService.GetDocumentDiagnosticsAsync(documents, currentSolutionToken, token).ConfigureAwait(false);

//             return new RelatedFullDocumentDiagnosticReport {
//                 Diagnostics = diagnostics.Where(d => !d.IsHiddenInUI()).Select(d => d.ToServerDiagnostic()).ToList(),
//             };
//         });
//     }

//     private IEnumerable<DocumentId>? GetDocumentIdsWithFilePath(string filePath) {
//         var documentIds = workspaceService.Solution?.GetDocumentIdsWithFilePathV2(filePath);
//         if (documentIds == null || !documentIds.Any())
//             return null;

//         if (configurationService.UseMultitargetDiagnostics)
//             return documentIds;

//         return documentIds.Take(1);
//     }
// }
using DeskMatch.CoreService.Application.Workspaces.Models;

namespace DeskMatch.CoreService.Application.Workspaces.Services;

public interface IExcelWorkspaceParser
{
    BulkParseResult Parse(Stream excelStream);
}

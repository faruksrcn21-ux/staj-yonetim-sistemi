using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajEvrak.Api.Dtos;
using StajEvrak.Api.Models;
using StajEvrak.Api.Services;

namespace StajEvrak.Api.Controllers;

[ApiController]
[Route("api/admin/dataset")]
[Authorize(Roles = Roles.Admin)]
public class AdminDatasetController : ControllerBase
{
    private readonly IDatasetImportService _import;

    public AdminDatasetController(IDatasetImportService import) => _import = import;

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ErrorResponse("Lütfen bir dosya seçin."));

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _import.ImportAsync(file.FileName, stream);
            return Ok(result);
        }
        catch (Exception ex) when (ex is NotSupportedException or FormatException)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var records = await _import.GetAllAsync();
        return Ok(records.Select(DatasetRecordResponse.From));
    }
}
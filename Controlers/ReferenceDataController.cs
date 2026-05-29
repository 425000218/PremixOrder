using Dapper;
using Microsoft.AspNetCore.Mvc;
using PremixOrderAPI.Data;

namespace PremixOrderAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReferenceDataController : ControllerBase
{
    private readonly DatabaseConnection _db;
    public ReferenceDataController(DatabaseConnection db) => _db = db;

    [HttpGet("factories")]
    public async Task<IActionResult> GetFactories()
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var rows = await conn.QueryAsync("dbo.sp_GetFactories", commandType: System.Data.CommandType.StoredProcedure);
            return Ok(rows);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2812)
        {
            return StatusCode(500, "Thiếu proc dbo.sp_GetFactories. Hãy chạy Database/UpdateStoredProcedures.sql trong SSMS.");
        }
    }

    [HttpGet("regions")]
    public async Task<IActionResult> GetRegions()
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var rows = await conn.QueryAsync("SELECT RegionID, RegionName FROM dbo.Region ORDER BY RegionName");
            return Ok(rows);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("divisions")]
    public async Task<IActionResult> GetDivisions()
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var rows = await conn.QueryAsync("SELECT DivisionID, DivisionName FROM dbo.Division ORDER BY DivisionName");
            return Ok(rows);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var rows = await conn.QueryAsync("dbo.sp_GetProducts", commandType: System.Data.CommandType.StoredProcedure);
            return Ok(rows);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2812)
        {
            return StatusCode(500, "Thiếu proc dbo.sp_GetProducts. Hãy chạy Database/UpdateStoredProcedures.sql trong SSMS.");
        }
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers()
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var rows = await conn.QueryAsync("dbo.sp_GetSuppliers", commandType: System.Data.CommandType.StoredProcedure);
            return Ok(rows);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2812)
        {
            return StatusCode(500, "Thiếu proc dbo.sp_GetSuppliers. Hãy chạy Database/UpdateStoredProcedures.sql trong SSMS.");
        }
    }

    [HttpDelete("factories/{factoryCode}")]
    public async Task<IActionResult> DeleteFactory(string factoryCode)
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var affected = await conn.ExecuteAsync(
                "DELETE FROM dbo.Factory WHERE FactoryCode = @factoryCode",
                new { factoryCode });

            return affected == 0 ? NotFound(new { message = "Factory not found." }) : Ok(new { message = "Deleted successfully." });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            return StatusCode(400, ex.Message);
        }
    }

    [HttpDelete("products/{productCode}")]
    public async Task<IActionResult> DeleteProduct(string productCode)
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var affected = await conn.ExecuteAsync(
                "DELETE FROM dbo.Product WHERE ProductCode = @productCode",
                new { productCode });

            return affected == 0 ? NotFound(new { message = "Product not found." }) : Ok(new { message = "Deleted successfully." });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            return StatusCode(400, ex.Message);
        }
    }

    [HttpDelete("suppliers/{supplierShortName}")]
    public async Task<IActionResult> DeleteSupplier(string supplierShortName)
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var affected = await conn.ExecuteAsync(
                "DELETE FROM dbo.Supplier WHERE SupplierShortName = @supplierShortName",
                new { supplierShortName });

            return affected == 0 ? NotFound(new { message = "Supplier not found." }) : Ok(new { message = "Deleted successfully." });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            return StatusCode(400, ex.Message);
        }
    }
}
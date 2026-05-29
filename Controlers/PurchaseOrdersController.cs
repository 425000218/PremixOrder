using Dapper;
using Microsoft.AspNetCore.Mvc;
using PremixOrderAPI.Data;
using PremixOrderAPI.Models;
using System.Data;

namespace PremixOrderAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly DatabaseConnection _db;
    public PurchaseOrdersController(DatabaseConnection db) => _db = db;

    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var info = await conn.QuerySingleAsync("dbo.sp_GetDatabaseInfo", commandType: CommandType.StoredProcedure);
            return Ok(info);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? fromCreatedDate = null, [FromQuery] DateTime? toCreatedDate = null)
    {
        try
        {
            var fromDate = (fromCreatedDate ?? DateTime.Today).Date;
            var toDate = (toCreatedDate ?? DateTime.Today).Date;
            if (fromDate > toDate)
            {
                (fromDate, toDate) = (toDate, fromDate);
            }

            using var conn = _db.CreateOpenConnection();
            var rows = await conn.QueryAsync(
                "dbo.sp_GetPurchaseOrdersByCreatedDate",
                new { FromDate = fromDate, ToDate = toDate },
                commandType: CommandType.StoredProcedure);
            var result = rows.GroupBy(x => x.PONumber).Select(g => new
            {
                poNumber = g.Key,
                orderDate = g.First().OrderDate,
                createdDate = g.First().CreatedDate,
                supplierShortName = g.First().SupplierShortName,
                regionName = g.First().RegionName,
                divisionName = g.First().DivisionName,
                details = g.Select(d => new
                {
                    d.PODetailID,
                    d.ProductCode,
                    d.ProductName,
                    d.FactoryCode,
                    d.FactoryName,
                    d.Quantity,
                    d.UnitPrice,
                    d.VATRate,
                    d.DeliveryMonth,
                    d.Note
                })
            });
            return Ok(new
            {
                count = result.Count(),
                fromDate,
                toDate,
                data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreate([FromBody] List<PurchaseOrderRowDto> rows)
    {
        if (rows == null || rows.Count == 0) return BadRequest("No data.");
        foreach (var r in rows)
        {
            if (string.IsNullOrWhiteSpace(r.PONumber)) return BadRequest("PO Number required");
            if (r.Quantity <= 0) return BadRequest("Quantity > 0");
            if (r.UnitPrice < 0) return BadRequest("UnitPrice >= 0");
        }
        var dt = new DataTable();
        dt.Columns.Add("PONumber", typeof(string));
        dt.Columns.Add("OrderDate", typeof(DateTime));
        dt.Columns.Add("SupplierShortName", typeof(string));
        dt.Columns.Add("RegionName", typeof(string));
        dt.Columns.Add("DivisionName", typeof(string));
        dt.Columns.Add("ProductCode", typeof(string));
        dt.Columns.Add("FactoryCode", typeof(string));
        dt.Columns.Add("Quantity", typeof(decimal));
        dt.Columns.Add("UnitPrice", typeof(decimal));
        dt.Columns.Add("VATRate", typeof(string));
        dt.Columns.Add("DeliveryMonth", typeof(string));
        dt.Columns.Add("ExpectedDeliveryDate", typeof(DateTime));
        dt.Columns.Add("Note", typeof(string));
        foreach (var r in rows)
        {
            dt.Rows.Add(r.PONumber, r.OrderDate, r.SupplierShortName, r.RegionName, r.DivisionName,
                        r.ProductCode, r.FactoryCode, r.Quantity, r.UnitPrice, r.VATRate,
                        r.DeliveryMonth, r.ExpectedDeliveryDate, r.Note);
        }
        using var conn = _db.CreateOpenConnection();
        var param = new { Orders = dt.AsTableValuedParameter("dbo.PurchaseOrderRowType") };
        try
        {
            await conn.ExecuteAsync("dbo.sp_BulkInsertPurchaseOrders", param, commandType: CommandType.StoredProcedure);
            return Ok(new { Message = $"Saved {rows.Count} rows." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("row")]
    public async Task<IActionResult> CreateRow([FromBody] PurchaseOrderRowDto row)
    {
        return await SaveRowInternal(row, null);
    }

    [HttpPut("row/{detailId:int}")]
    public async Task<IActionResult> UpdateRow(int detailId, [FromBody] PurchaseOrderRowDto row)
    {
        return await SaveRowInternal(row, detailId);
    }

    [HttpDelete("row/{detailId:int}")]
    public async Task<IActionResult> DeleteRow(int detailId)
    {
        try
        {
            using var conn = _db.CreateOpenConnection();
            var affected = await conn.ExecuteAsync(
                "dbo.sp_DeletePurchaseOrderDetail",
                new { PODetailID = detailId },
                commandType: CommandType.StoredProcedure);

            if (affected == 0)
            {
                return NotFound(new { message = "Không tìm thấy dòng để xoá." });
            }

            return Ok(new { message = "Deleted successfully." });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            return StatusCode(400, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    private async Task<IActionResult> SaveRowInternal(PurchaseOrderRowDto row, int? detailId)
    {
        if (string.IsNullOrWhiteSpace(row.PONumber)) return BadRequest("PO Number required");
        if (row.Quantity <= 0) return BadRequest("Quantity > 0");
        if (row.UnitPrice < 0) return BadRequest("UnitPrice >= 0");

        try
        {
            using var conn = _db.CreateOpenConnection();
            var result = await conn.QuerySingleAsync(
                "dbo.sp_SavePurchaseOrderDetail",
                new
                {
                    PODetailID = detailId,
                    row.PONumber,
                    row.OrderDate,
                    row.SupplierShortName,
                    row.RegionName,
                    row.DivisionName,
                    row.ProductCode,
                    row.FactoryCode,
                    row.Quantity,
                    row.UnitPrice,
                    row.VATRate,
                    row.DeliveryMonth,
                    row.ExpectedDeliveryDate,
                    row.Note
                },
                commandType: CommandType.StoredProcedure);

            return Ok(new { message = detailId == null ? "Created successfully." : "Updated successfully.", data = result });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            return StatusCode(400, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
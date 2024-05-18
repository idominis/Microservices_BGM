﻿using AutoMapper;
using DataAccessService.Data;
using DataAccessService.Dto;
using DataAccessService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly BgmDbContext _context;
        private readonly IMapper _mapper;

        public DataController(BgmDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("save-pod")]
        public async Task<IActionResult> SavePODToDb([FromBody] List<PurchaseOrderDetailDto> podDetails)
        {
            if (podDetails == null || !podDetails.Any())
            {
                return BadRequest("No purchase order details provided.");
            }

            var existingIds = new HashSet<int>(_context.PurchaseOrderDetails.Select(p => p.PurchaseOrderDetailId));
            var newDetails = podDetails
                .Where(dto => !existingIds.Contains(dto.PurchaseOrderDetailId))
                .Select(dto => _mapper.Map<PurchaseOrderDetail>(dto))
                .ToList();

            if (newDetails.Any())
            {
                await _context.PurchaseOrderDetails.AddRangeAsync(newDetails);
                try
                {
                    await _context.SaveChangesAsync();
                    string detailIds = string.Join(", ", newDetails.Select(h => h.PurchaseOrderDetailId));
                    Log.Information($"Purchase order details: {detailIds} loaded and saved successfully!");
                    return Ok("Purchase order details saved successfully!");
                }
                catch (DbUpdateException ex)
                {
                    Log.Error(ex, "Error updating the database with new purchase order details.");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                return Ok("No new purchase orders details to save.");
            }
        }

        [HttpPost("save-poh")]
        public async Task<IActionResult> SavePOHToDb([FromBody] List<PurchaseOrderHeaderDto> podHeaders)
        {
            if (podHeaders == null || !podHeaders.Any())
            {
                return BadRequest("No purchase order details provided.");
            }

            var existingIds = new HashSet<int>(_context.PurchaseOrderHeaders.Select(p => p.PurchaseOrderId));
            var newHeaders = podHeaders
                .Where(dto => !existingIds.Contains(dto.PurchaseOrderId))
                .Select(dto => _mapper.Map<PurchaseOrderHeader>(dto))
                .ToList();

            if (newHeaders.Any())
            {
                await _context.PurchaseOrderHeaders.AddRangeAsync(newHeaders);
                try
                {
                    await _context.SaveChangesAsync();
                    string headerIds = string.Join(", ", newHeaders.Select(h => h.PurchaseOrderId));
                    Log.Information($"Purchase order headers: {headerIds} loaded and saved successfully!");
                    return Ok("Purchase order headers saved successfully!");
                }
                catch (DbUpdateException ex)
                {
                    Log.Error(ex, "Error updating the database with new purchase order headers.");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                return Ok("No new purchase orders headers to save.");
            }
        }
    }
}

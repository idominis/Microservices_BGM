﻿using FileManagementService.Interfaces;
using FileManagementService.Models;
using FileManagementService.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace FileManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileManagementServiceController : ControllerBase
    {
        private readonly IXmlService _xmlService;
        private readonly FileManager _fileManager;

        public FileManagementServiceController(IXmlService xmlService, FileManager fileManager)
        {
            _xmlService = xmlService;
            _fileManager = fileManager;
        }

        [HttpPost("generate-xml")]
        public IActionResult GenerateXMLFiles([FromBody] List<PurchaseOrderSummary> summaries, DateTime? startDate = null, DateTime? endDate = null)
        {
            _xmlService.GenerateXMLFiles(summaries, startDate, endDate);
            return Ok();
        }

        [HttpGet("load-xml")]
        public ActionResult LoadFromXml(string filePath, string typeName)
        {
            try
            {
                var result = _xmlService.LoadFromXml(filePath, typeName);
                if (result == null)
                {
                    return NotFound("Failed to load XML.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("extract-purchase-order-id")]
        public ActionResult<List<int>> ExtractPurchaseOrderIdFromXml(string filePath)
        {
            try
            {
                var ids = _xmlService.ExtractPurchaseOrderIdFromXml(filePath);
                return Ok(ids);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("extract-purchase-order-detail-id")]
        public ActionResult<List<int>> ExtractPurchaseOrderDetailIdFromXml(string filePath)
        {
            try
            {
                var ids = _xmlService.ExtractPurchaseOrderDetailIdFromXml(filePath);
                return Ok(ids);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get-file-paths")]
        public ActionResult GetFilePaths()
        {
            return Ok(new
            {
                BaseDirectoryPath = _fileManager.GetBaseDirectoryPath(),
                BaseHeadersDirectoryPath = _fileManager.GetSpecificLocalPath("headers"),
                BaseDirectoryXmlCreatedPath = _fileManager.GetBaseDirectoryXmlCreatedPath(),
                RemoteDetailsDirectoryPath = _fileManager.GetRemoteDetailsDirectoryPath(),
                RemoteHeadersDirectoryPath = _fileManager.GetRemoteHeadersDirectoryPath(),
                RemoteDirectoryPath = _fileManager.GetSpecificRemotePath("\\Uploaded")
            });
        }
    }
}

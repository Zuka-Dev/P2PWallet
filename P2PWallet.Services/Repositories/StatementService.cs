
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Repositories
{
    public class StatementService : IStatementService
    {
        private readonly P2PWalletDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConverter _pdfConverter;
        private readonly IConfiguration _config;

        public StatementService(P2PWalletDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration config, IConverter pdfConverter)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _pdfConverter = pdfConverter;
            _config = config;
        }
        public async Task<byte[]> GenerateStatement(StatementRequestDTO request)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
            if (userIdClaim == null)
            {
                new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Missing or invalid user ID in JWT token",
                    Data = new { }
                };
            }
            var userId = userIdClaim.Value;
            var user = await _context.Users.Include(x => x.Accounts).FirstOrDefaultAsync(x => Convert.ToString(x.Id) == userId);
            var userAccount = user.Accounts[0];
            var transactions = await _context.Transfers.
                Where(
                x =>
                x.SenderId == userAccount.AccountNumber || x.BeneficiaryId == userAccount.AccountNumber &&
                x.TransactionTime >= request.StartDate &&
                x.TransactionTime <= request.EndDate
                ).OrderByDescending(x => x.TransactionTime).ToListAsync();
            return request.Format.ToLower() == "pdf" ? GeneratePdfStatement(user, transactions, request, userAccount) : GenerateExcelStatement(user, transactions, request,userAccount);
        }

        private byte[] GenerateExcelStatement(User user, List<Transfer> transactions, StatementRequestDTO request, Account userAccount)
        {
            try
            {
                using var workbook = new XSSFWorkbook();
                var sheet = workbook.CreateSheet("Statement");

                // Create styles
                var headerStyle = workbook.CreateCellStyle();
                headerStyle.FillForegroundColor = IndexedColors.Grey40Percent.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;

                var debitStyle = workbook.CreateCellStyle();
                var debitFont = workbook.CreateFont();
                debitFont.Color = IndexedColors.Red.Index;
                debitStyle.SetFont(debitFont);

                var creditStyle = workbook.CreateCellStyle();
                var creditFont = workbook.CreateFont();
                creditFont.Color = IndexedColors.Green.Index;
                creditStyle.SetFont(creditFont);

                // Add account info and date range
                var titleRow = sheet.CreateRow(0);
                titleRow.CreateCell(0).SetCellValue($"Name: {user.FirstName} {user.LastName}");
                var accountInfoRow = sheet.CreateRow(1);
                accountInfoRow.CreateCell(0).SetCellValue($"Account Number: {userAccount.AccountNumber}");
                accountInfoRow.CreateCell(1).SetCellValue($"Currency: {userAccount.Currency}");
                var dateRangeRow = sheet.CreateRow(2);
                dateRangeRow.CreateCell(0).SetCellValue($"Period: {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}");

                // Calculate opening and closing balance
                decimal openingBalance = userAccount.Balance;
                foreach (var transaction in transactions)
                {
                    if (transaction.SenderId == userAccount.AccountNumber)
                        openingBalance += transaction.Amount;
                    else
                        openingBalance -= transaction.Amount;
                }
                decimal closingBalance = userAccount.Balance;

                var balanceRow = sheet.CreateRow(3);
                balanceRow.CreateCell(0).SetCellValue($"Opening Balance: {openingBalance:C}");
                balanceRow.CreateCell(1).SetCellValue($"Closing Balance: {closingBalance:C}");

                // Create header row
                var headerRow = sheet.CreateRow(5);
                var headers = new[] { "Date", "Description", "Amount", "Balance" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = headerRow.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = headerStyle;
                }

                decimal runningBalance = openingBalance;
                for (int i = 0; i < transactions.Count; i++)
                {
                    var row = sheet.CreateRow(i + 6);  // Start from row 6 to account for added info rows
                    var transaction = transactions[i];

                    row.CreateCell(0).SetCellValue(transaction.TransactionTime.ToString("yyyy-MM-dd HH:mm:ss"));

                    var descriptionCell = row.CreateCell(1);
                    var isDebit = transaction.SenderId == userAccount.AccountNumber;
                    descriptionCell.SetCellValue(isDebit ? "Debit" : "Credit");
                    descriptionCell.CellStyle = isDebit ? debitStyle : creditStyle;

                    var amount = isDebit ? -transaction.Amount : transaction.Amount;
                    var amountCell = row.CreateCell(2);
                    amountCell.SetCellValue((double)amount);
                    amountCell.CellStyle = isDebit ? debitStyle : creditStyle;

                    runningBalance += amount;
                    row.CreateCell(3).SetCellValue((double)runningBalance);
                }

                // Auto-size columns
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                using var ms = new MemoryStream();
                workbook.Write(ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                // Log the exception details here
                throw;
            }
        }
        private byte[] GeneratePdfStatement(User user, List<Transfer> transactions, StatementRequestDTO request, Account userAccount)
        {
            try
            {
                var html = GenerateHtmlStatement(user, transactions, request, userAccount);
                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                };
                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = html
                };
                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };
                return _pdfConverter.Convert(pdf);
            }catch(Exception ex)
            {
                throw;
            }
        }

        private string GenerateHtmlStatement(User user, List<Transfer> transactions, StatementRequestDTO request, Account userAccount)
        {
            string templatePath = _config.GetSection("EmailTemplates:StatementTemplatePath").Value!;
            string htmlTemplate = File.ReadAllText(templatePath);
            htmlTemplate = htmlTemplate.Replace("[[AccountNumber]]", userAccount.AccountNumber)
                                  .Replace("[[FullName]]", $"{user.FirstName} {user.LastName}")
                                  .Replace("[[StartDate]]", request.StartDate.ToString("yyyy-MM-dd"))
                                  .Replace("[[EndDate]]", request.EndDate.ToString("yyyy-MM-dd"));
            var transactionRows = new StringBuilder();
            decimal runningBalance = userAccount.Balance;
            decimal openingBalance = userAccount.Balance;

            transactionRows.Append($"<p><strong>Opening Balance:</strong> {openingBalance:C}</p>");
            foreach (var transaction in transactions)
            {
                var amount = transaction.SenderId == userAccount.AccountNumber ? -transaction.Amount : transaction.Amount;
                runningBalance -= amount;
                transactionRows.Append("<tr>");
                transactionRows.Append($"<td>{transaction.TransactionTime:yyyy-MM-dd HH:mm:ss}</td>");
                transactionRows.Append($"<td>{(transaction.SenderId == userAccount.AccountNumber ? "Debit" : "Credit")}</td>");
                transactionRows.Append($"<td class='{(amount < 0 ? "debit" : "credit")}'>{amount:C}</td>");
                transactionRows.Append($"<td>{runningBalance:C}</td>");
                transactionRows.Append("</tr>");
            }
            decimal closingBalance = userAccount.Balance;
            transactionRows.Append($"<p><strong>Closing Balance:</strong> {closingBalance:C}</p>");
            htmlTemplate = htmlTemplate.Replace("[[TransactionRows]]", transactionRows.ToString());
            return htmlTemplate;
        }
    }
}

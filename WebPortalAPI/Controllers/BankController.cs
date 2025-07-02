using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebPortalAPI.Services;
using Microsoft.EntityFrameworkCore;
using WebPortalAPI.Models;
using System.Text.Json;

[ApiController]
[Route("api/bank")]
[Authorize(Roles = "Bank")]
public class BankController : ControllerBase
{
    private readonly PmfdatabaseContext _context;

    public BankController(PmfdatabaseContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Verifies a challan for payment processing
    /// </summary>
    /// <param name="challanNo">The challan number to verify</param>
    /// <returns>Challan details if valid, error if invalid</returns>
    [HttpGet("verify/{challanNo}")]
    public async Task<IActionResult> VerifyChallan(string challanNo)
    {
        // Validate challan number format
        if (!int.TryParse(challanNo, out int challanId))
        {
            return BadRequest(new
            {
                message = "Invalid challan number format.",
                error = "INVALID_CHALLAN_FORMAT"
            });
        }

        // Fetch challan with related data
        var challan = await _context.Challans
            .Include(c => c.Applicant)
            .Include(c => c.FeeTitle)
            .Include(c => c.BankTransactions)
            .FirstOrDefaultAsync(c => c.ChallanNo == challanId);

        // Validation 1: Challan Not Found
        if (challan == null)
        {
            return NotFound(new
            {
                message = "Challan not found.",
                error = "CHALLAN_NOT_FOUND"
            });
        }

        // Validation 2: Already Paid
        if (challan.IsPaid == true)
        {
            var lastTransaction = challan.BankTransactions
                .OrderByDescending(bt => bt.TransactionDate)
                .FirstOrDefault();

            return BadRequest(new
            {
                message = "This challan has already been paid.",
                error = "ALREADY_PAID",
                paymentDetails = new
                {
                    challanNo = challan.ChallanNo,
                    paidAt = lastTransaction?.TransactionDate,
                    transactionId = lastTransaction?.TransactionId,
                    bankName = lastTransaction?.BankName
                }
            });
        }

        // Validation 3: Expired
        var today = DateOnly.FromDateTime(DateTime.Now);
        bool isExpired = challan.IsExpired == true || 
            (challan.FeeTitle.HasExpiry && challan.FeeTitle.ExpiryDate < today);

        if (isExpired)
        {
            return BadRequest(new
            {
                message = "This challan has expired.",
                error = "CHALLAN_EXPIRED",
                expiryDetails = new
                {
                    challanNo = challan.ChallanNo,
                    generatedDate = challan.GeneratedDate,
                    expiryDate = challan.FeeTitle.ExpiryDate
                }
            });
        }

        // All validations passed, return challan details
        var details = !string.IsNullOrEmpty(challan.Details) 
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(challan.Details)
            : null;

        return Ok(new
        {
            message = "Challan verified successfully.",
            data = new
            {
                challanInfo = new
                {
                    challanNo = challan.ChallanNo,
                    generatedDate = challan.GeneratedDate,
                    status = "VALID"
                },
                applicantInfo = new
                {
                    id = challan.ApplicantId,
                    name = challan.Applicant.FullName,
                    cnic = challan.Applicant.Cnic,
                    contactNumber = challan.Applicant.MobileNo,
                    email = challan.Applicant.Email
                },
                feeInfo = new
                {
                    titleId = challan.FeeTitleId,
                    title = challan.FeeTitle.Title,
                    amount = challan.FeeAmount,
                    expiryDate = challan.FeeTitle.HasExpiry ? challan.FeeTitle.ExpiryDate : null,
                    description = challan.FeeTitle.Description
                },
                additionalDetails = details
            }
        });
    }

    /// <summary>
    /// Submits a payment for a challan
    /// </summary>
    /// <param name="paymentDto">Payment details including challan number and branch information</param>
    /// <returns>Payment confirmation details</returns>
    [HttpPost("pay")]
    public async Task<IActionResult> SubmitPayment([FromBody] PaymentDTO paymentDto)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(paymentDto.ChallanNo) ||
                string.IsNullOrWhiteSpace(paymentDto.BranchName) ||
                string.IsNullOrWhiteSpace(paymentDto.BranchCode))
            {
                return BadRequest(new
                {
                    message = "All fields (challan number, branch name, and branch code) are required.",
                    error = "INVALID_INPUT"
                });
            }

            // Parse challan number
            if (!int.TryParse(paymentDto.ChallanNo, out int challanId))
            {
                return BadRequest(new
                {
                    message = "Invalid challan number format.",
                    error = "INVALID_CHALLAN_FORMAT"
                });
            }

            // Fetch challan with related data
            var challan = await _context.Challans
                .Include(c => c.FeeTitle)
                .FirstOrDefaultAsync(c => c.ChallanNo == challanId);

            // Validate challan exists
            if (challan == null)
            {
                return NotFound(new
                {
                    message = "Challan not found.",
                    error = "CHALLAN_NOT_FOUND"
                });
            }

            // Check if already paid
            if (challan.IsPaid == true)
            {
                return BadRequest(new
                {
                    message = "This challan has already been paid.",
                    error = "ALREADY_PAID"
                });
            }

            // Check if expired
            var today = DateOnly.FromDateTime(DateTime.Now);
            bool isExpired = challan.IsExpired == true || 
                (challan.FeeTitle.HasExpiry && challan.FeeTitle.ExpiryDate < today);

            if (isExpired)
            {
                return BadRequest(new
                {
                    message = "This challan has expired.",
                    error = "CHALLAN_EXPIRED"
                });
            }

            // Create bank transaction
            var transaction = new BankTransaction
            {
                ChallanNo = challanId,
                ChallanDate = challan.GeneratedDate,
                ChallanAmount = challan.FeeAmount,
                FeeTitle = challan.FeeTitle.Title,
                PaidDate = DateOnly.FromDateTime(DateTime.Now),
                BranchName = paymentDto.BranchName,
                BranchCode = paymentDto.BranchCode
            };

            // Update challan status
            challan.IsPaid = true;

            // Save changes
            _context.BankTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Return success response
            return Ok(new
            {
                message = "Payment processed successfully.",
                data = new
                {
                    transactionId = transaction.TransactionId,
                    challanNo = transaction.ChallanNo,
                    amount = transaction.ChallanAmount,
                    paidDate = transaction.PaidDate,
                    branchInfo = new
                    {
                        name = transaction.BranchName,
                        code = transaction.BranchCode
                    },
                    feeTitle = transaction.FeeTitle
                }
            });
        }
        catch (Exception ex)
        {
            // Log the exception details here
            return StatusCode(500, new
            {
                message = "An error occurred while processing the payment.",
                error = "INTERNAL_SERVER_ERROR"
            });
        }
    }
}

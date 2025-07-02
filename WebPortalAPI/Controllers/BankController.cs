using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebPortalAPI.Services;
using Microsoft.EntityFrameworkCore;
using WebPortalAPI.Models;
using WebPortalAPI.DTOs;
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
                .OrderByDescending(bt => bt.PaidDate)
                .FirstOrDefault();

            return BadRequest(new
            {
                message = "This challan has already been paid.",
                error = "ALREADY_PAID",
                paymentDetails = new
                {
                    challanNo = challan.ChallanNo,
                    paidDate = lastTransaction?.PaidDate,
                    transactionId = lastTransaction?.TransactionId,
                    bankName = lastTransaction?.BranchName
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
                    description = challan.FeeTitle.Title
                },
                additionalDetails = details
            }
        });
    }

    /// <summary>
    /// Submits a payment for a challan
    /// </summary>
    /// <param name="challanNo">The challan number to mark as paid</param>
    /// <returns>Payment confirmation details</returns>
    [HttpPost("pay/{challanNo}")]
    public async Task<IActionResult> SubmitPayment([FromRoute] string challanNo)
    {
        try
        {
            // Parse challan number
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

            // Get bank details from authenticated user
            var bankUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

            // Create bank transaction
            var transaction = new BankTransaction
            {
                ChallanNo = challan.ChallanNo,
                ChallanDate = challan.GeneratedDate,
                ChallanAmount = challan.FeeAmount,
                FeeTitle = challan.FeeTitle.Title,
                PaidDate = DateOnly.FromDateTime(DateTime.Now),
                BranchName = bankUser.Username,  // Using bank username as branch name
                BranchCode = "001"  // You can modify this as needed
            };

            // Add transaction and update challan
            challan.IsPaid = true;
            _context.BankTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment processed successfully",
                data = new
                {
                    transactionId = transaction.TransactionId,
                    challanNo = transaction.ChallanNo,
                    amount = transaction.ChallanAmount,
                    paidDate = transaction.PaidDate,
                    branchName = transaction.BranchName,
                    branchCode = transaction.BranchCode
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while processing the payment.",
                error = ex.Message
            });
        }
    }
} 
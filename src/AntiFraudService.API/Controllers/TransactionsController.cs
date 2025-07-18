using AntiFraudService.Application.DTOs;
using AntiFraudService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AntiFraudService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Crea una nueva transacción con estado "pending"
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _transactionService.CreateTransactionAsync(dto);
        return CreatedAtAction(nameof(GetTransaction), new { id }, null);
    }

    /// <summary>
    /// Consulta una transacción por su ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var transaction = await _transactionService.GetTransactionAsync(id);
        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }

    /// <summary>
    /// Lista todas las transacciones (para debugging)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTransactions()
    {
        var transactions = await _transactionService.GetAllTransactionsAsync();
        return Ok(transactions);
    }
}

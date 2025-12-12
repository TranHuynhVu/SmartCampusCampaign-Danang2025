using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCD_2025_BE.Entities.Domains;
using SCD_2025_BE.Entities.DTO;
using SCD_2025_BE.Repositories;
using System.Security.Claims;

namespace SCD_2025_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Categories
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
        {
            var categories = await _unitOfWork.Categories.GetActiveCategoriesAsync();

            var response = categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                UpdatedBy = c.UpdatedBy,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });

            return Ok(response);
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoryResponseDto>> GetCategory(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if (category == null || category.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy danh mục." });
            }

            var response = new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                UpdatedBy = category.UpdatedBy,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(response);
        }

        // POST: api/Categories
        [HttpPost]
        [Authorize(Roles = "Admin,Company")]
        public async Task<ActionResult<CategoryResponseDto>> CreateCategory(CategoryDto categoryDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var category = new Category
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var response = new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                UpdatedBy = category.UpdatedBy,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
        }

        // POST: api/Categories/update/5
        [HttpPost("update/{id}")]
        [Authorize(Roles = "Admin,Company")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto categoryDto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if (category == null || category.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy danh mục." });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = userId;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Categories/delete/5
        [HttpPost("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if (category == null || category.DeletedAt != null)
            {
                return NotFound(new { Message = "Không tìm thấy danh mục." });
            }

            category.DeletedAt = DateTime.UtcNow;
            category.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}

using ECommerce_web.Repository;
using ECommerce_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Areas.Admin.Controllers
{
	[Area("Admin")]
	//[Route("Admin/Slider")]
	//[Authorize(Roles = "Publisher")]
	public class SliderController : Controller
	{
		private readonly DataContext _dataContext;
		public SliderController(DataContext dataContext)
		{
			_dataContext = dataContext;
		}

		[Route("Index")]
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Sliders.OrderByDescending(c => c.Id).ToListAsync());
		}
	}
}

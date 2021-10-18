﻿namespace UpSkill.Web.Areas.Admin.Course
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using ViewModels.Course;
    using Services.Data.Contracts.Course;

    using static Common.GlobalConstants.CompaniesConstants;
    using static Common.GlobalConstants.ControllerRoutesConstants;
    using System.Collections.Generic;

    [AllowAnonymous]
    public class CoursesController : AdministrationBaseController
    {
        private readonly ICoursesService coursesService;

        public CoursesController(ICoursesService coursesService)
        {
            this.coursesService = coursesService;
        }

        [HttpGet]
        [Route(DetailsRoute)]
        public async Task<DetailsViewModel> Details(int id)
        => await this.coursesService
                     .GetByIdAsync<DetailsViewModel>(id);


        [HttpPost]
        [Route(CreateRoute)]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            var result = await this.coursesService.CreateAsync(model);

            if (result.Failure)
            {
                return BadRequest(result.Error);
            }

            return Ok(SuccesfullyCreated);
        }

        [HttpPut]
        [Route(EditRoute)]
        public async Task<IActionResult> Edit(EditCourseViewModel model)
        {
            var result = await this.coursesService.EditAsync(model);

            if (result.Failure)
            {
                return BadRequest(result.Error);
            }

            return Ok(SuccesfullyEdited);
        }

        [HttpDelete]
        [Route(DeleteRoute)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await this.coursesService.DeleteAsync(id);

            if (result.Failure)
            {
                return BadRequest(result.Error);
            }

            return Ok(SuccesfullyDeleted);
        }

        [HttpGet]
        [Route(GetAllRoute)]
        public async Task<IEnumerable<DetailsViewModel>> GetAll()
          => await this.coursesService.GetAllAsync<DetailsViewModel>();

        [HttpGet]
        [Route(DetailsRoute)]
        public async Task<DetailsViewModel> GetDetails(int id)
            => await this.coursesService.GetByIdAsync<DetailsViewModel>(id);
    }
}

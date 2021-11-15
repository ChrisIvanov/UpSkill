﻿namespace UpSkill.Web.Areas.Admin.Company
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using UpSkill.Services.Data.Contracts.Company;
    using UpSkill.Web.Infrastructure.Extensions;
    using UpSkill.Web.ViewModels.Company;

    using static Common.GlobalConstants.ControllerRoutesConstants;
    using static Common.GlobalConstants.ControllersResponseMessages;

    public class CompaniesController : AdministrationBaseController
    {
        private readonly ICompanyService companyService;

        public CompaniesController(
            ICompanyService companyService)
        {
            this.companyService = companyService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCompanyRequestModel model)
        {
            var result = await this.companyService.CreateAsync(model);

            if (result.Failure)
            {
                NLogExtensions.GetInstance().Error(model, new Exception(result.Error));

                return this.BadRequest(result.Error);
            }

            NLogExtensions.GetInstance().Info(model);

            return this.StatusCode(201, SuccesfullyCreated);
        }

        [HttpPut]
        public async Task<IActionResult> Edit(UpdateCompanyRequestModel model, int id)
        {
            var result = await this.companyService.EditAsync(model, id);

            if (result.Failure)
            {
                NLogExtensions.GetInstance().Error(model, new Exception(result.Error));

                return this.BadRequest(result.Error);
            }

            NLogExtensions.GetInstance().Info(model);

            return this.Ok(SuccesfullyEdited);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await this.companyService.DeleteAsync(id);

            if (result.Failure)
            {
                NLogExtensions.GetInstance().Error(id, new Exception(result.Error));

                return this.BadRequest(result.Error);
            }

            NLogExtensions.GetInstance().Info(id);

            return this.Ok(SuccesfullyDeleted);
        }

        [HttpGet]
        [Route(GetAllRoute)]
        public async Task<IEnumerable<CompanyListingModel>> GetAll()
        {
            NLogExtensions.GetInstance().Info("Entering getAll action");

            return await this.companyService.GetAllAsync<CompanyListingModel>();
        }

        [HttpGet]
        [Route(DetailsRoute)]
        public async Task<CompanyDetailsModel> GetDetails(int id)
        {
            NLogExtensions.GetInstance().Info("Entering GetDetails action");

            return await this.companyService.GetByIdAsync<CompanyDetailsModel>(id);
        }
    }
}

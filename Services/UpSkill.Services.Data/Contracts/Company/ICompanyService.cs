﻿namespace UpSkill.Services.Data.Contracts.Company 
{
    using System.Threading.Tasks; 

    using UpSkill.Common;
    using UpSkill.Data.Models;
    using UpSkill.Web.ViewModels.Company;

    public interface ICompanyService
    {
        Task<Result> CreateAsync(CreateCompanyRequestModel model);

        Task<Result> EditAsync(UpdateCompanyRequestModel model);

        Task<Result> DeleteAsync(int id);

        Task<Company> GetCompanyByIdAsync(int id);
    }
}

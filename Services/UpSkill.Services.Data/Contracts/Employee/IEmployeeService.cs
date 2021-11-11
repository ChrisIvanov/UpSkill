﻿namespace UpSkill.Services.Data.Contracts.Employee
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEmployeeService
    {
        Task<IEnumerable<TModel>> GetAllCoursesAsync<TModel>(string userId);

        Task<TModel> GetByIdCourseAsync<TModel>(string userId, int courseId);
    }
}

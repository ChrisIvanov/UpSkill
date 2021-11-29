﻿namespace UpSkill.Web.Tests.Routes
{
    using MyTested.AspNetCore.Mvc;
    using UpSkill.Web.Areas.Owner.Coach;
    using UpSkill.Web.ViewModels.Owner;
    using Xunit;

    using static Comman.TestConstants.Coach;

    public class CoachesRouteTest
    {
        [Fact]
        public void PostAddingCoachToOwnerShouldBeRouteCorrectly()
           => MyRouting
           .Configuration()
           .ShouldMap(request => request
           .WithLocation(TestAddCoachToOwnerRouteCoach)
           .WithMethod(HttpMethod.Post))
           .To<CoachesController>(c => c.AddCoachToOwner(With.Any<AddCoachToCompanyModel>()));

        [Fact]
        public void GetGetAllCompanyCoachrShouldBeRouteCorrectly()
           => MyRouting
           .Configuration()
           .ShouldMap(request => request
           .WithLocation(TestGetAllRouteCompanyCoach)
           .WithMethod(HttpMethod.Get))
           .To<CoachesController>(c => c.GetAll());
    }
}

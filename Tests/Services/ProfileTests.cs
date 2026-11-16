using GiaPha.BusinessObjects.DTOs.Profiles;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Helpers;
using GiaPha.Services.Implementations;
namespace GiaPha.UnitTests.Services;
public class ProfileTests
{
 [Theory][InlineData(" Đặng   ĐẢM ","dang dam")][InlineData("Nguyễn Phúc","nguyen phuc")][InlineData(null,"")]
 public void NormalizeVietnamese(string? text,string expected)=>Assert.Equal(expected,VietnameseTextNormalizer.Normalize(text));
 [Fact]public void PrivateLivingRedactsButDeceasedDoesNot()
 {
 var service=new PrivacyFilterService();var p=new PersonProfileDto{IsLiving=true,PrivacyLevel=PrivacyLevel.Private,HuyName="Húy",BirthDate=new(){Year=2000},PortraitUrl="photo",PhoneNumber="123",Biography="bio"};
 service.Apply(p,[]);Assert.True(p.IsRedacted);Assert.Null(p.BirthDate);Assert.Null(p.PhoneNumber);Assert.Null(p.PortraitUrl);
 var dead=new PersonProfileDto{PrivacyLevel=PrivacyLevel.Private,PhoneNumber="123"};service.Apply(dead,[]);Assert.Equal("123",dead.PhoneNumber);
 }
 [Fact]public void FamilyKeepsOnlyBirthYearForOrdinaryMember()
 {
 var p=new PersonProfileDto{IsLiving=true,PrivacyLevel=PrivacyLevel.Family,BirthDate=new(){Year=2000,Month=2,Day=3},PhoneNumber="123"};new PrivacyFilterService().Apply(p,[]);Assert.Equal(2000,p.BirthDate?.Year);Assert.Null(p.BirthDate?.Day);Assert.Null(p.PhoneNumber);
 }
}

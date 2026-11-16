using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Implementations;
namespace GiaPha.UnitTests.Services;
public class KinshipTests
{
 [Theory][InlineData(0,"Bản thân")][InlineData(1,"Cha")][InlineData(2,"Ông nội")][InlineData(3,"Cụ nội")][InlineData(4,"Kỵ nội")][InlineData(5,null)]
 public void LinealAndScope(int distance,string? expected)
 {
 var list=new List<Person>();for(int i=0;i<=distance;i++){var p=new Person{FullName=$"P{i}",Generation=i+1};p.Path="/"+string.Join('/',list.Select(x=>x.Id).Append(p.Id))+"/";list.Add(p);}
 var r=new KinshipResolver().Resolve(list[^1],list[0],list,[],[]);Assert.Equal(expected,r.FromCallsTo);Assert.Equal(distance>4,r.IsOutOfScope);
 }
 [Theory][InlineData(true,Gender.Male,"Bác (anh của cha)")][InlineData(true,Gender.Female,"Bác (chị của cha)")][InlineData(false,Gender.Male,"Chú ruột")][InlineData(false,Gender.Female,"Cô ruột")]
 public void UncleTermsUseSiblingOrder(bool older,Gender gender,string expected)
 {
 var root=new Person();root.Path=$"/{root.Id}/";var father=new Person();father.Path=$"{root.Path}{father.Id}/";var child=new Person();child.Path=$"{father.Path}{child.Id}/";var uncle=new Person{Gender=gender};uncle.Path=$"{root.Path}{uncle.Id}/";
 var r=new KinshipResolver().Resolve(child,uncle,[root,father,child,uncle],[new(){ParentId=root.Id,ChildId=father.Id,SiblingOrder=2},new(){ParentId=root.Id,ChildId=uncle.Id,SiblingOrder=older?1:3}],[]);Assert.Equal(expected,r.FromCallsTo);Assert.Equal("Cháu",r.ToCallsFrom);
 }
 [Theory][InlineData(1,Gender.Male,"Anh ruột")][InlineData(1,Gender.Female,"Chị ruột")][InlineData(2,Gender.Male,"Anh họ (con bác)")][InlineData(2,Gender.Female,"Chị họ (con bác)")][InlineData(3,Gender.Male,"Anh họ")][InlineData(3,Gender.Female,"Chị họ")][InlineData(4,Gender.Male,"Anh họ")][InlineData(4,Gender.Female,"Chị họ")]
 public void CollateralGenerations(int depth,Gender gender,string expected)
 {
 var root=new Person();root.Path=$"/{root.Id}/";var people=new List<Person>{root};var relations=new List<ParentChild>();
 Person Branch(int order){var parent=root;for(int i=0;i<depth;i++){var p=new Person{Gender=gender,Path=""};p.Path=$"{parent.Path}{p.Id}/";relations.Add(new(){ParentId=parent.Id,ChildId=p.Id,SiblingOrder=order});people.Add(p);parent=p;}return parent;}
 var a=Branch(2);var b=Branch(1);Assert.Equal(expected,new KinshipResolver().Resolve(a,b,people,relations,[]).FromCallsTo);
 }
}

using System.Text.Json;
using GiaPha.BusinessObjects.Entities;
namespace GiaPha.Services.Implementations;
public sealed record KinshipPathNode(Guid PersonId,string FullName,Gender Gender,int Generation,bool IsLiving,string? PortraitUrl,string RoleInPath,string StepLabel,string? RelationToNext);
public sealed class KinshipResultDto
{
 public Guid FromPersonId {get;set;}
 public Guid ToPersonId {get;set;}
 public bool IsOutOfScope {get;set;}
 public string? OutOfScopeReasonCode {get;set;}
 public string? OutOfScopeMessage {get;set;}
 public Guid? CommonAncestorId {get;set;}
 public int UpSteps {get;set;}
 public int DownSteps {get;set;}
 public int GenerationDelta=>UpSteps-DownSteps;
 public string BranchComparison {get;set;}="Lineal";
 public string SpouseRole {get;set;}="Blood";
 public string? Term {get;set;}
 public string? FromCallsTo {get;set;}
 public string? ToCallsFrom {get;set;}
 public string[] RegionalAliases {get;set;}=[];
 public string Summary {get;set;}="";
 public string? AdoptionNote {get;set;}
 public IReadOnlyList<KinshipPathNode> Path {get;set;}=[];
}
public sealed class KinshipResolver
{
 private sealed record TermEntry(string Name,string[] RegionalAliases);
 private static readonly Dictionary<string,TermEntry> Terms=Load();
 private static Dictionary<string,TermEntry> Load(){using var stream=typeof(KinshipResolver).Assembly.GetManifestResourceStream("GiaPha.Services.Data.KinshipTerms.json")!;return JsonSerializer.Deserialize<Dictionary<string,TermEntry>>(stream,new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;}
 private static TermEntry Term(int u,int d,bool older,Gender gender,bool spouse)=>Terms[$"{u}-{d}-{older}-{gender==Gender.Male}-{spouse}".ToLowerInvariant()];
 public KinshipResultDto Resolve(Person from,Person to,IReadOnlyList<Person> people,IReadOnlyList<ParentChild> relations,IReadOnlyList<Union> unions)
 {
 var result=new KinshipResultDto{FromPersonId=from.Id,ToPersonId=to.Id};
 KinshipResultDto Outside(string reason){result.IsOutOfScope=true;result.OutOfScopeReasonCode=reason;result.OutOfScopeMessage="Quan hệ ngoài phạm vi tra cứu phụ hệ 4 đời.";return result;}
 if(unions.Any(u=>u.HusbandId==from.Id&&u.WifeId==to.Id || u.HusbandId==to.Id&&u.WifeId==from.Id)){result.SpouseRole="DirectSpouse";result.FromCallsTo=to.Gender==Gender.Female?"Vợ":"Chồng";result.ToCallsFrom=from.Gender==Gender.Female?"Vợ":"Chồng";result.Term=result.FromCallsTo;result.Summary=$"{from.FullName} gọi {to.FullName} là {result.FromCallsTo}.";return result;}
 static Guid[] Segments(string path)=>path.Split('/',StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToArray();
 var a=Segments(from.Path);var b=Segments(to.Path);var target=to;bool spouse=false;
 int Common(Guid[] x,Guid[] y){int i=0;while(i<x.Length&&i<y.Length&&x[i]==y[i])i++;return i-1;}
 int lca=Common(a,b);
 if(lca<0){var ids=unions.Where(u=>u.HusbandId==to.Id||u.WifeId==to.Id).Select(u=>u.HusbandId==to.Id?u.WifeId:u.HusbandId);target=people.FirstOrDefault(p=>ids.Contains(p.Id)&&Common(a,Segments(p.Path))>=0)??to;if(target.Id!=to.Id){b=Segments(target.Path);lca=Common(a,b);spouse=true;result.SpouseRole="SpouseOfTarget";}}
 if(lca<0)return Outside("NO_COMMON_PATERNAL_ANCESTOR");
 int up=a.Length-1-lca,down=b.Length-1-lca;result.UpSteps=up;result.DownSteps=down;result.CommonAncestorId=a[lca];
 if(up>4||down>4)return Outside("EXCEEDS_4_GENERATIONS");
 bool older=false;
 if(up>0&&down>0){var ra=relations.FirstOrDefault(r=>r.ParentId==a[lca]&&r.ChildId==a[lca+1]&&r.RelationType==RelationType.Biological);var rb=relations.FirstOrDefault(r=>r.ParentId==a[lca]&&r.ChildId==b[lca+1]&&r.RelationType==RelationType.Biological);if(ra==null||rb==null||ra.SiblingOrder==rb.SiblingOrder)return Outside("UNKNOWN_SIBLING_ORDER");older=rb.SiblingOrder<ra.SiblingOrder;result.BranchComparison=older?"Older":"Younger";}
 if(from.Id==to.Id)result.SpouseRole="Self";
 var term=Term(up,down,older,to.Gender,spouse);result.FromCallsTo=term.Name;result.Term=term.Name;result.RegionalAliases=term.RegionalAliases;
 result.ToCallsFrom=Term(down,up,!older,from.Gender,false).Name;
 if(up==1&&down==1&&older)result.ToCallsFrom=from.Gender==Gender.Male?"Em trai ruột":"Em gái ruột";
 if(up==2&&down==1)result.ToCallsFrom="Cháu";
 if(up==1&&down==2)result.ToCallsFrom=older?(from.Gender==Gender.Male?"Chú":"Cô"):"Bác";
 if(relations.Any(r=>r.RelationType!=RelationType.Biological&&((r.ParentId==from.Id&&r.ChildId==to.Id)||(r.ParentId==to.Id&&r.ChildId==from.Id))))result.AdoptionNote="Lưu ý: Có quan hệ nhận nuôi/kế tự trực tiếp (Cha nuôi – Con nuôi).";
 var idsPath=a.Skip(lca).Reverse().Concat(b.Skip(lca+1)).ToList();if(spouse)idsPath.Add(to.Id);
 result.Path=idsPath.Select((id,i)=>{var p=people.First(x=>x.Id==id);return new KinshipPathNode(id,p.FullName,p.Gender,p.Generation,p.IsLiving,p.PortraitUrl,id==from.Id?"From":id==to.Id?"To":id==a[lca]?"CommonAncestor":"Ascendant",$"Đời {p.Generation}",i==idsPath.Count-1?null:i<up?"Con của":"Cha của");}).ToArray();
 result.Summary=$"{from.FullName} gọi {to.FullName} là {result.FromCallsTo}; {to.FullName} gọi {from.FullName} là {result.ToCallsFrom}.";return result;
 }
}

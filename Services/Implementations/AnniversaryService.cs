using System.Text;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace GiaPha.Services.Implementations;
public sealed record AnniversaryItem(Guid PersonId,string FullName,string? HuyName,string? ThuyName,int Generation,Guid? BranchId,string BranchName,string? PortraitUrl,int LunarDay,int LunarMonth,int LunarYear,string LunarDateDisplay,DateOnly SolarDate,string SolarDateDisplay,int YearsSinceDeath,bool IsAdjustedFromLeapMonth,bool IsAdjustedFromDay30To29,string? AdjustmentNote);
public sealed record ClanAnniversaryCalendarDto(Guid ClanId,string ClanName,int SolarYear,int? LunarMonthFilter,int TotalEvents,IReadOnlyList<AnniversaryItem> Items,IReadOnlyList<AnniversarySkipWarning> Warnings);
public sealed class AnniversaryService(IF3Repository repo,IAnniversaryCalculator calculator)
{
 public async Task<ClanAnniversaryCalendarDto> GetAsync(Guid clan,int? year,int? month,Guid? branch,CancellationToken ct=default)
 {
 int y=year??DateTime.UtcNow.AddHours(7).Year;
 if(y<1800 || y>2199)throw new DomainValidationException("YEAR_OUT_OF_RANGE","Năm tra cứu phải từ 1800 đến 2199.");
 if(month is <1 or >12)throw new DomainValidationException("INVALID_LUNAR_MONTH","Tháng âm lịch phải từ 1 đến 12.");
 var name=await repo.Clans.Where(c=>c.Id==clan).Select(c=>c.Name).SingleOrDefaultAsync(ct)??throw new NotFoundException("Không tìm thấy dòng họ.");
 var people=await repo.People.AsNoTracking().Where(p=>p.ClanId==clan && p.Status==PersonStatus.Active && (!branch.HasValue || p.BranchId==branch)).ToListAsync(ct);
 var branches=await repo.Branches.Where(b=>b.ClanId==clan).ToDictionaryAsync(b=>b.Id,b=>b.Name,ct);
 var items=new List<AnniversaryItem>();var warnings=new List<AnniversarySkipWarning>();
 foreach(var p in people)
 {
 var result=calculator.CalculateForSolarYear(p,y);if(result.Warning!=null)warnings.Add(result.Warning);
 foreach(var o in result.OccurrencesInSolarYear.Where(o=>month==null || o.ObservedLunarDate.Month==month))
 items.Add(new(p.Id,p.FullName,p.HuyName,p.ThuyName,p.Generation,p.BranchId,p.BranchId is {} id?branches.GetValueOrDefault(id,"Chi gốc"):"Chi gốc",p.PortraitUrl,o.ObservedLunarDate.Day,o.ObservedLunarDate.Month,o.ObservedLunarDate.Year,o.ObservedLunarDate.ToString(),o.SolarDate,o.SolarDate.ToString("dd/MM/yyyy"),o.YearsSinceDeath,o.IsAdjustedFromLeapMonth,o.IsAdjustedFromDay30To29,o.IsAdjustedFromLeapMonth?"Cúng vào tháng thường":o.IsAdjustedFromDay30To29?"Tháng thiếu: cúng ngày 29":null));
 }
 return new(clan,name,y,month,items.Count,items.OrderBy(i=>i.LunarMonth).ThenBy(i=>i.LunarDay).ThenBy(i=>i.SolarDate).ToArray(),warnings);
 }
 public static string ExportIcs(ClanAnniversaryCalendarDto calendar)
 {
 static string Escape(string s)=>s.Replace("\\","\\\\").Replace("\r\n","\n").Replace("\r","\n").Replace("\n","\\n").Replace(";","\\;").Replace(",","\\,");
 var lines=new List<string>{"BEGIN:VCALENDAR","VERSION:2.0","PRODID:-//VGMS//Vietnamese Genealogy Management System//VI","CALSCALE:GREGORIAN","METHOD:PUBLISH","X-WR-CALNAME:"+Escape("Lịch giỗ - "+calendar.ClanName),"X-WR-TIMEZONE:Asia/Ho_Chi_Minh"};
 foreach(var x in calendar.Items){lines.AddRange(new[]{"BEGIN:VEVENT",$"UID:anniversary-{calendar.ClanId}-{x.PersonId}-{x.SolarDate:yyyy-MM-dd}@vgms.local",$"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}",$"DTSTART;VALUE=DATE:{x.SolarDate:yyyyMMdd}",$"DTEND;VALUE=DATE:{x.SolarDate.AddDays(1):yyyyMMdd}","SUMMARY:"+Escape($"Giỗ Đời {x.Generation}: {x.FullName} ({x.LunarDay}/{x.LunarMonth} ÂL)"),"DESCRIPTION:"+Escape($"Ngày giỗ lần thứ {x.YearsSinceDeath}.\nNgày âm lịch: {x.LunarDateDisplay}"),"BEGIN:VALARM","ACTION:DISPLAY","TRIGGER:-P7D","DESCRIPTION:"+Escape("Nhắc giỗ: "+x.FullName),"END:VALARM","END:VEVENT"});}
 lines.Add("END:VCALENDAR");
 // RFC 5545: fold by UTF-8 octets, never split a Unicode rune.
 var output=new StringBuilder();foreach(var line in lines){int length=0;foreach(var rune in line.EnumerateRunes()){int n=rune.Utf8SequenceLength;if(length+n>75){output.Append("\r\n ");length=1;}output.Append(rune);length+=n;}output.Append("\r\n");}return output.ToString();
 }
}

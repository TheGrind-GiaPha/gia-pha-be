using System.Text;
using GiaPha.Services.Implementations;
namespace GiaPha.UnitTests.Services;
public class CalendarTests
{
 [Fact]public void IcsHasStableUidAllDayAlarmEscapingAndUtf8Folding()
 {
 var clan=Guid.NewGuid();var person=Guid.NewGuid();var item=new AnniversaryItem(person,"Nguyễn; Văn, A",null,null,1,null,"Chi gốc",null,19,12,2024,"19/12/2024 (ÂL)",new(2025,1,18),"18/01/2025",205,false,false,null);
 var text=AnniversaryService.ExportIcs(new(clan,new string('ễ',100),2025,null,1,[item],[]));
 Assert.Contains($"UID:anniversary-{clan}-{person}-2025-01-18@vgms.local",text.Replace("\r\n ",""));Assert.Contains("DTSTART;VALUE=DATE:20250118",text);Assert.Contains("DTEND;VALUE=DATE:20250119",text);Assert.Contains("TRIGGER:-P7D",text);Assert.Contains("Nguyễn\\; Văn\\, A",text);
 Assert.All(text.Split("\r\n"),line=>Assert.True(Encoding.UTF8.GetByteCount(line)<=75));
 }
}

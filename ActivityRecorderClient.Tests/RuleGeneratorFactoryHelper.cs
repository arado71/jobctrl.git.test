using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tct.Tests
{
	public static class RuleGeneratorFactoryHelper
	{
		public static readonly string CurrentGenerators = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ArrayOfRuleGeneratorData xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://jobctrl.com/"">
  <RuleGeneratorData>
    <Name>IgnoreRuleGenerator</Name>
    <Parameters>{""IgnoreCase"":true,""ProcessNamePattern"":{""MatchingPattern"":""^(winword|excel|powerpnt)[.]exe$"",""NegateMatch"":false},""TitlePattern"":{""MatchingPattern"":""^(Opening|Megnyit\\u00E1s)\\s-\\s(?:Microsoft\\s)?(Word|Excel|PowerPoint)(?:\\s\\([\\p{L}\\s-]+\\))?$"",""NegateMatch"":false},""UrlPattern"":{""MatchingPattern"":""^.*$"",""NegateMatch"":false}}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>ReplaceGroupRuleGenerator</Name>
    <Parameters>{""IgnoreCase"":true,""ProcessNameParams"":[{""MatchingPattern"":""^excel[.]exe$"",""ReplaceGroupName"":null}],""TitleParams"":[{""MatchingPattern"":""^(?(?=(Microsoft Excel(?&lt;optBrac&gt;\\s\\([\\p{L}\\s-]+\\))?\\s-\\s))(Microsoft Excel(?&lt;optBrac&gt;\\s\\([\\p{L}\\s-]+\\))?\\s-\\s"",""ReplaceGroupName"":null},{""MatchingPattern"":""(?&lt;file&gt;.+?)"",""ReplaceGroupName"":""file""},{""MatchingPattern"":""(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?(?:[:]\\d+)?(?&lt;optBrac&gt;\\s\\s?\\[[\\p{L}\\s-]+\\])*$)|("",""ReplaceGroupName"":null},{""MatchingPattern"":""(?&lt;file&gt;.+?)"",""ReplaceGroupName"":""file""},{""MatchingPattern"":""(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?(?:[:]\\d+)?(?&lt;optBrac&gt;\\s\\s?\\[[\\p{L}\\s-]+\\])*(?&lt;optBrac&gt;\\s\\([\\p{L}\\s-]+\\))?\\s-\\sExcel(?&lt;optBrac&gt;\\s\\([\\p{L}\\s-]+\\))?$))"",""ReplaceGroupName"":null}],""UrlParams"":[{""MatchingPattern"":""^.*$"",""ReplaceGroupName"":null}]}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>ReplaceGroupRuleGenerator</Name>
    <Parameters>{""IgnoreCase"":true,""ProcessNameParams"":[{""MatchingPattern"":""^winword[.]exe$"",""ReplaceGroupName"":null}],""TitleParams"":[{""MatchingPattern"":""^"",""ReplaceGroupName"":null},{""MatchingPattern"":""(?&lt;file&gt;.+?)"",""ReplaceGroupName"":""file""},{""MatchingPattern"":""(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?((?&lt;optPar&gt;\\s\\([\\p{L}\\s-]+\\))?(?&lt;optBrac&gt;\\s\\[[\\p{L}\\s-]+\\])?)*(?:[:]\\d+)?\\s-(?&lt;optMs&gt;\\sMicrosoft)?\\sWord(?&lt;optParEnd&gt;\\s\\([\\p{L}\\s-]+\\))?$"",""ReplaceGroupName"":null}],""UrlParams"":[{""MatchingPattern"":""^.*$"",""ReplaceGroupName"":null}]}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>ReplaceGroupRuleGenerator</Name>
    <Parameters>{""IgnoreCase"":true,""ProcessNameParams"":[{""MatchingPattern"":""^powerpnt[.]exe$"",""ReplaceGroupName"":null}],""TitleParams"":[{""MatchingPattern"":""^(?(?=((?&lt;optMs&gt;Microsoft\\s)?(?&lt;optOf&gt;Office\\s)?PowerPoint(?&lt;optText&gt;.*?)\\s[-\\u2013]\\s\\[))((?&lt;optMs&gt;Microsoft\\s)?(?&lt;optOf&gt;Office\\s)?PowerPoint(?&lt;optText&gt;.*?)\\s[-\\u2013]\\s\\["",""ReplaceGroupName"":null},{""MatchingPattern"":""(?&lt;file&gt;.+?)"",""ReplaceGroupName"":""file""},{""MatchingPattern"":""(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?(?&lt;optBrac&gt;\\s\\s?\\[[\\p{L}\\s-]+\\])*(?:[:]\\d+)?\\](?&lt;optMsE&gt;\\s-\\sMicrosoft PowerPoint)?$)|("",""ReplaceGroupName"":null},{""MatchingPattern"":""(?&lt;file&gt;.+?)"",""ReplaceGroupName"":""file""},{""MatchingPattern"":""(?&lt;optNum&gt;\\s?\\(\\d{1,2}\\)|\\[\\d{1,2}\\])?(?&lt;optExt&gt;\\.\\p{L}{1,4})?((?&lt;optPar&gt;\\s\\([\\p{L}\\s-]+\\))?(?&lt;optBrac&gt;\\s\\[[\\p{L}\\s-]+\\])?)*(?:[:]\\d+)?\\s-\\s(?&lt;optMs&gt;Microsoft\\s)?PowerPoint(?&lt;optParEnd&gt;\\s\\([\\p{L}\\s-]+\\))?$))"",""ReplaceGroupName"":null}],""UrlParams"":[{""MatchingPattern"":""^.*$"",""ReplaceGroupName"":null}]}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>IgnoreRuleGenerator</Name>
    <Parameters>{""IgnoreCase"":true,""ProcessNamePattern"":{""MatchingPattern"":""^(winword|excel|powerpnt)[.]exe$"",""NegateMatch"":false},""TitlePattern"":{""MatchingPattern"":""^.*$"",""NegateMatch"":false},""UrlPattern"":{""MatchingPattern"":""^.*$"",""NegateMatch"":false}}</Parameters>
  </RuleGeneratorData>
  <RuleGeneratorData>
    <Name>SimpleRuleGenerator</Name>
    <Parameters>{""IgnoreCase"":true}</Parameters>
  </RuleGeneratorData>
</ArrayOfRuleGeneratorData>";
	}
}
